using ChurnPrediction.ML.Models;
using ChurnPrediction.ML.Training;
using Microsoft.ML;
using Microsoft.ML.Data;

var mlContext = new MLContext(seed: 0);

var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "telco_churn.csv");
IDataView fullData = mlContext.Data.LoadFromTextFile<ChurnData>(dataPath, hasHeader: true, separatorChar: ',');

var split = mlContext.Data.TrainTestSplit(fullData, testFraction: 0.2, seed: 0);
IDataView trainData = split.TrainSet;
IDataView testData = split.TestSet;

Console.WriteLine("Data loaded and split.");

// All categorical (string) columns that need one-hot encoding
var categoricalColumns = new[]
{
    "Gender", "SeniorCitizen", "Married", "Dependents", "ReferredAFriend",
    "Offer", "PhoneService", "MultipleLines", "InternetService", "InternetType",
    "OnlineSecurity", "OnlineBackup", "DeviceProtectionPlan", "PremiumTechSupport",
    "StreamingTV", "StreamingMovies", "StreamingMusic", "UnlimitedData",
    "Contract", "PaperlessBilling", "PaymentMethod"
};

// All numeric columns that need normalization
var numericColumns = new[]
{
    "Age", "NumberOfDependents", "NumberOfReferrals", "TenureInMonths",
    "AvgMonthlyLongDistanceCharges", "AvgMonthlyGBDownload", "MonthlyCharge",
    "TotalCharges", "TotalRefunds", "TotalExtraDataCharges",
    "TotalLongDistanceCharges"
    // "SatisfactionScore" removed — testing for leakage
};

var oneHotPairs = categoricalColumns
    .Select(c => new InputOutputColumnPair(c + "Encoded", c))
    .ToArray();

var normalizePairs = numericColumns
    .Select(c => new InputOutputColumnPair(c + "Normalized", c))
    .ToArray();

var featureColumns = categoricalColumns.Select(c => c + "Encoded")
    .Concat(numericColumns.Select(c => c + "Normalized"))
    .ToArray();

var pipeline = mlContext.Transforms.CustomMapping(
        new ChurnLabelMappingFactory().GetMapping(), contractName: "ChurnLabelMapping")
    .Append(mlContext.Transforms.Categorical.OneHotEncoding(oneHotPairs))
    .Append(mlContext.Transforms.NormalizeMinMax(normalizePairs))
    .Append(mlContext.Transforms.Concatenate("Features", featureColumns));

// ============================================================
// Phase 4: Model Training
// ============================================================

// Append a binary classification trainer to the same transform pipeline.
// SdcaLogisticRegression is a fast, interpretable baseline — a good
// starting point before comparing against more complex trainers later.
var trainingPipeline = pipeline.Append(
    mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
        labelColumnName: "Label",
        featureColumnName: "Features"));

Console.WriteLine("Training model...");

// Fit() runs the ENTIRE pipeline — custom mapping, one-hot encoding,
// normalization, AND the trainer — against trainData in one pass.
// This produces a single ITransformer that encapsulates everything.
var trainedModel = trainingPipeline.Fit(trainData);

Console.WriteLine("Model training complete.");

// Quick smoke test: transform a few rows through the trained model
// and inspect the output schema, just to confirm predictions exist
// before we build proper evaluation in Phase 5.
var predictions = trainedModel.Transform(testData);
var samplePreview = predictions.Preview(maxRows: 3);

// ============================================================
// Phase 5: Evaluation
// ============================================================

var metrics = mlContext.BinaryClassification.Evaluate(
    predictions, labelColumnName: "Label");

Console.WriteLine("\n--- Model Evaluation Metrics ---");
Console.WriteLine($"Accuracy:  {metrics.Accuracy:P2}");
Console.WriteLine($"AUC:       {metrics.AreaUnderRocCurve:P2}");
Console.WriteLine($"F1 Score:  {metrics.F1Score:P2}");
Console.WriteLine($"Precision: {metrics.PositivePrecision:P2}");
Console.WriteLine($"Recall:    {metrics.PositiveRecall:P2}");

Console.WriteLine("\n--- Confusion Matrix ---");
Console.WriteLine(metrics.ConfusionMatrix.GetFormattedConfusionTable());

// ============================================================
// Threshold Tuning — explore the precision/recall tradeoff
// ============================================================

var predictionResults = mlContext.Data
    .CreateEnumerable<PredictionResult>(predictions, reuseRowObject: false)
    .ToList();

Console.WriteLine("\n--- Threshold Tuning ---");
Console.WriteLine("Threshold | Precision | Recall  | F1");

foreach (var threshold in new[] { 0.5f, 0.4f, 0.3f, 0.25f, 0.2f })
{
    int tp = predictionResults.Count(p => p.Label && p.Probability >= threshold);
    int fp = predictionResults.Count(p => !p.Label && p.Probability >= threshold);
    int fn = predictionResults.Count(p => p.Label && p.Probability < threshold);

    double precision = (tp + fp == 0) ? 0 : tp / (double)(tp + fp);
    double recall = (tp + fn == 0) ? 0 : tp / (double)(tp + fn);
    double f1 = (precision + recall == 0) ? 0 : 2 * precision * recall / (precision + recall);

    Console.WriteLine($"{threshold:F2}      | {precision:P1}    | {recall:P1}  | {f1:P1}");
}
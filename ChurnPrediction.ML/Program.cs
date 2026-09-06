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
    "TotalLongDistanceCharges", "SatisfactionScore"
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

Console.WriteLine("\n--- Sample Predictions ---");
foreach (var row in samplePreview.RowView)
{
    var label = row.Values.First(c => c.Key == "Label").Value;
    var predictedLabel = row.Values.First(c => c.Key == "PredictedLabel").Value;
    var score = row.Values.First(c => c.Key == "Score").Value;
    var probability = row.Values.First(c => c.Key == "Probability").Value;

    Console.WriteLine(
        $"Actual: {label} | Predicted: {predictedLabel} | " +
        $"Score: {score} | Probability: {probability}");
}
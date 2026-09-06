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

// Smoke test: fit the pipeline (no trainer yet) and transform one batch,
// just to confirm every step works before we add a trainer in Phase 4.
var preview = pipeline.Fit(trainData).Transform(trainData);
var firstRow = preview.Preview(maxRows: 1).RowView.First();

var labelValue = firstRow.Values.First(c => c.Key == "Label").Value;
var featuresValue = firstRow.Values.First(c => c.Key == "Features").Value;

Console.WriteLine($"Label column value: {labelValue}");
Console.WriteLine($"Features column type: {featuresValue.GetType()}");
using ChurnPrediction.ML.Models;
using ChurnPrediction.ML.Training;
using Microsoft.ML;
using Xunit;

namespace ChurnPrediction.Tests;

public class ModelIntegrationTests
{
    private static string GetModelPath()
    {
        // Walk up from the test's output directory to the solution root,
        // then down to where the real model.zip lives.
        return Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "ChurnPrediction.Api", "MLModels", "model.zip");
    }

    [Fact]
    public void LoadedModel_OnHighRiskProfile_PredictsChurn()
    {
        // Arrange
        var mlContext = new MLContext();
        var modelPath = GetModelPath();

        Assert.True(File.Exists(modelPath), $"model.zip not found at {modelPath} — run Phase 6 training first.");

        var model = mlContext.Model.Load(modelPath, out _);
        var predictionEngine = mlContext.Model
            .CreatePredictionEngine<ChurnData, ChurnPredictionOutput>(model);

        // A profile with known strong churn signal: month-to-month,
        // very short tenure, no add-on services — matches the manual
        // Swagger test that already confirmed WillChurn: true in practice.
        var highRiskCustomer = new ChurnData
        {
            Gender = "Male",
            Age = 45,
            SeniorCitizen = "No",
            Married = "No",
            Dependents = "No",
            NumberOfDependents = 0,
            ReferredAFriend = "No",
            NumberOfReferrals = 0,
            TenureInMonths = 2,
            Offer = "None",
            PhoneService = "Yes",
            AvgMonthlyLongDistanceCharges = 10,
            MultipleLines = "No",
            InternetService = "Yes",
            InternetType = "Fiber Optic",
            AvgMonthlyGBDownload = 20,
            OnlineSecurity = "No",
            OnlineBackup = "No",
            DeviceProtectionPlan = "No",
            PremiumTechSupport = "No",
            StreamingTV = "No",
            StreamingMovies = "No",
            StreamingMusic = "No",
            UnlimitedData = "No",
            Contract = "Month-to-Month",
            PaperlessBilling = "Yes",
            PaymentMethod = "Mailed Check",
            MonthlyCharge = 95,
            TotalCharges = 190,
            TotalRefunds = 0,
            TotalExtraDataCharges = 0,
            TotalLongDistanceCharges = 20,
            SatisfactionScore = 0,
            ChurnLabel = "No"
        };

        // Act
        var prediction = predictionEngine.Predict(highRiskCustomer);

        // Assert — using the same 0.30 operating threshold as the API
        Assert.True(prediction.Probability >= 0.30f,
            $"Expected high-risk profile to exceed 0.30 threshold, but got {prediction.Probability}");
    }
}
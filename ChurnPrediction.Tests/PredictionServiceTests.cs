using ChurnPrediction.Api.DTOs;
using ChurnPrediction.Api.Services;
using ChurnPrediction.ML.Models;
using ChurnPrediction.ML.Training;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChurnPrediction.Tests;

public class PredictionServiceTests
{
    private static ChurnRequest SampleRequest() => new()
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
        TotalLongDistanceCharges = 20
    };

    [Fact]
    public void Predict_WhenProbabilityAboveThreshold_ReturnsWillChurnTrue()
    {
        // Arrange: mock the model to return a probability above the 0.30 threshold
        var mockPredictor = new Mock<IModelPredictor>();
        mockPredictor
            .Setup(p => p.Predict(It.IsAny<ChurnData>()))
            .Returns(new ChurnPredictionOutput { Probability = 0.75f, PredictedLabel = true });

        var loggerMock = new Mock<ILogger<PredictionService>>();
        var service = new PredictionService(mockPredictor.Object, loggerMock.Object);

        // Act
        var result = service.Predict(SampleRequest());

        // Assert
        Assert.True(result.WillChurn);
        Assert.Equal(0.75f, result.Probability);
        Assert.Equal(0.30f, result.ThresholdUsed);
    }

    [Fact]
    public void Predict_WhenProbabilityBelowThreshold_ReturnsWillChurnFalse()
    {
        // Arrange: mock the model to return a probability below the 0.30 threshold
        var mockPredictor = new Mock<IModelPredictor>();
        mockPredictor
            .Setup(p => p.Predict(It.IsAny<ChurnData>()))
            .Returns(new ChurnPredictionOutput { Probability = 0.15f, PredictedLabel = false });

        var loggerMock = new Mock<ILogger<PredictionService>>();
        var service = new PredictionService(mockPredictor.Object, loggerMock.Object);

        // Act
        var result = service.Predict(SampleRequest());

        // Assert
        Assert.False(result.WillChurn);
        Assert.Equal(0.15f, result.Probability);
    }

    [Fact]
    public void Predict_WhenProbabilityExactlyAtThreshold_ReturnsWillChurnTrue()
    {
        // Boundary test: threshold comparison uses >=, so exactly 0.30 should churn.
        // This kind of edge-case test is exactly what catches off-by-one/boundary bugs.
        var mockPredictor = new Mock<IModelPredictor>();
        mockPredictor
            .Setup(p => p.Predict(It.IsAny<ChurnData>()))
            .Returns(new ChurnPredictionOutput { Probability = 0.30f, PredictedLabel = false });

        var loggerMock = new Mock<ILogger<PredictionService>>();
        var service = new PredictionService(mockPredictor.Object, loggerMock.Object);

        var result = service.Predict(SampleRequest());

        Assert.True(result.WillChurn);
    }
}
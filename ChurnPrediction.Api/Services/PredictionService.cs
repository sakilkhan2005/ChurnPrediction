using ChurnPrediction.Api.DTOs;
using ChurnPrediction.ML.Models;
using ChurnPrediction.ML.Training;
using Microsoft.Extensions.ML;

namespace ChurnPrediction.Api.Services;

public class PredictionService : IPredictionService
{
    // The operating threshold selected during Phase 5 evaluation —
    // NOT ML.NET's default 0.5. Applied manually here rather than
    // trusting the model's own PredictedLabel output.
    private const float OperatingThreshold = 0.30f;
 
    private readonly IModelPredictor _modelPredictor;
    private readonly ILogger<PredictionService> _logger;

    public PredictionService(IModelPredictor modelPredictor, ILogger<PredictionService> logger)
    {
        _modelPredictor = modelPredictor;
        _logger = logger;
    }

    public ChurnResponse Predict(ChurnRequest request)
    {
        var input = MapToChurnData(request);
        var prediction = _modelPredictor.Predict(input);        

        var response = new ChurnResponse
        {
            Probability = prediction.Probability,
            WillChurn = prediction.Probability >= OperatingThreshold,
            ThresholdUsed = OperatingThreshold
        };

        _logger.LogInformation(
            "Prediction made — Probability: {Probability}, WillChurn: {WillChurn}",
            response.Probability, response.WillChurn);

        return response;
    }

    private static ChurnData MapToChurnData(ChurnRequest request) => new()
    {
        Gender = request.Gender,
        Age = request.Age,
        SeniorCitizen = request.SeniorCitizen,
        Married = request.Married,
        Dependents = request.Dependents,
        NumberOfDependents = request.NumberOfDependents,
        ReferredAFriend = request.ReferredAFriend,
        NumberOfReferrals = request.NumberOfReferrals,
        TenureInMonths = request.TenureInMonths,
        Offer = request.Offer,
        PhoneService = request.PhoneService,
        AvgMonthlyLongDistanceCharges = request.AvgMonthlyLongDistanceCharges,
        MultipleLines = request.MultipleLines,
        InternetService = request.InternetService,
        InternetType = request.InternetType,
        AvgMonthlyGBDownload = request.AvgMonthlyGBDownload,
        OnlineSecurity = request.OnlineSecurity,
        OnlineBackup = request.OnlineBackup,
        DeviceProtectionPlan = request.DeviceProtectionPlan,
        PremiumTechSupport = request.PremiumTechSupport,
        StreamingTV = request.StreamingTV,
        StreamingMovies = request.StreamingMovies,
        StreamingMusic = request.StreamingMusic,
        UnlimitedData = request.UnlimitedData,
        Contract = request.Contract,
        PaperlessBilling = request.PaperlessBilling,
        PaymentMethod = request.PaymentMethod,
        MonthlyCharge = request.MonthlyCharge,
        TotalCharges = request.TotalCharges,
        TotalRefunds = request.TotalRefunds,
        TotalExtraDataCharges = request.TotalExtraDataCharges,
        TotalLongDistanceCharges = request.TotalLongDistanceCharges,

        // Placeholders — not used at prediction time, but required by the
        // model's baked-in schema (see Phase 7 design note in README).
        SatisfactionScore = 0,
        ChurnLabel = "No"
    };
}
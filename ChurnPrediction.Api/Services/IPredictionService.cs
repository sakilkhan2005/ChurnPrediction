using ChurnPrediction.Api.DTOs;

namespace ChurnPrediction.Api.Services;

public interface IPredictionService
{
    ChurnResponse Predict(ChurnRequest request);
}
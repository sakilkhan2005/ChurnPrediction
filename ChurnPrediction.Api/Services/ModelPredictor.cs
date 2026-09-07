using ChurnPrediction.ML.Models;
using ChurnPrediction.ML.Training;
using Microsoft.Extensions.ML;

namespace ChurnPrediction.Api.Services;

public class ModelPredictor : IModelPredictor
{
    private readonly PredictionEnginePool<ChurnData, ChurnPredictionOutput> _pool;

    public ModelPredictor(PredictionEnginePool<ChurnData, ChurnPredictionOutput> pool)
    {
        _pool = pool;
    }

    public ChurnPredictionOutput Predict(ChurnData input) =>
        _pool.Predict(modelName: "ChurnModel", example: input);
}
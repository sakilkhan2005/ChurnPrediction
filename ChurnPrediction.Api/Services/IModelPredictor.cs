using ChurnPrediction.ML.Models;
using ChurnPrediction.ML.Training;

namespace ChurnPrediction.Api.Services;

// Thin wrapper so PredictionService depends on an interface, not the
// concrete, non-mockable PredictionEnginePool directly — this is what
// makes the threshold logic unit-testable in isolation.
public interface IModelPredictor
{
    ChurnPredictionOutput Predict(ChurnData input);
}
namespace ChurnPrediction.Api.DTOs;

public class ChurnResponse
{
    public bool WillChurn { get; set; }
    public float Probability { get; set; }
    public float ThresholdUsed { get; set; }
}
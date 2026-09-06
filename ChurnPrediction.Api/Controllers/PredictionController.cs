using ChurnPrediction.Api.DTOs;
using ChurnPrediction.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChurnPrediction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    private readonly IPredictionService _predictionService;

    public PredictionController(IPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    /// <summary>
    /// Predicts customer churn risk based on account and service attributes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChurnResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ChurnResponse> Predict([FromBody] ChurnRequest request)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        var result = _predictionService.Predict(request);
        return Ok(result);
    }
}
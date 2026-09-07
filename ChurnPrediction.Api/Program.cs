using ChurnPrediction.Api.Services;
using ChurnPrediction.ML.Models;
using ChurnPrediction.ML.Training;
using Microsoft.Extensions.ML;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var modelPath = Path.Combine(AppContext.BaseDirectory, "MLModels", "model.zip");

builder.Services.AddPredictionEnginePool<ChurnData, ChurnPredictionOutput>()
    .FromFile(modelName: "ChurnModel", filePath: modelPath, watchForChanges: true);
builder.Services.AddScoped<IModelPredictor, ModelPredictor>();
builder.Services.AddScoped<IPredictionService, PredictionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
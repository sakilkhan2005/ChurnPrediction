using ChurnPrediction.ML.Models;
using Microsoft.ML;

// MLContext is the entry point for all ML.NET operations — think of it like
// a DbContext, but for machine learning pipelines. Seeding it makes results
// reproducible across runs, which matters when you're comparing trainers later.
var mlContext = new MLContext(seed: 0);

var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "telco_churn.csv");

// IDataView is ML.NET's lazy, columnar data structure — it isn't a List<T>,
// it's evaluated on demand as the pipeline consumes it.
IDataView fullData = mlContext.Data.LoadFromTextFile<ChurnData>(
    path: dataPath,
    hasHeader: true,
    separatorChar: ',');

Console.WriteLine("Data loaded successfully.");

// Sanity check: preview the first 5 rows to confirm columns mapped correctly
var preview = fullData.Preview(maxRows: 5);
foreach (var row in preview.RowView)
{
    foreach (var col in row.Values)
    {
        Console.Write($"{col.Key}: {col.Value} | ");
    }
    Console.WriteLine();
    Console.WriteLine("---");
}
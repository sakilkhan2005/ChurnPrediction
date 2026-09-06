using System;
using System.IO;
using System.Linq;

var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "telco_churn.csv");
var lines = File.ReadAllLines(csvPath);

var header = lines[0].Split(',');
var dataRows = lines.Skip(1).ToArray();

Console.WriteLine($"Total columns: {header.Length}");
Console.WriteLine($"Total rows: {dataRows.Length}");
Console.WriteLine();

Console.WriteLine("--- Column Index Map ---");
for (int i = 0; i < header.Length; i++)
{
    Console.WriteLine($"{i}: {header[i]}");
}
Console.WriteLine();

// Look up indices BY NAME instead of hardcoding — safer since you edited the file
int churnLabelIndex = Array.IndexOf(header, "Churn Label");
int totalChargesIndex = Array.IndexOf(header, "Total Charges");

if (churnLabelIndex == -1 || totalChargesIndex == -1)
{
    Console.WriteLine("ERROR: Expected column not found. Check exact header spelling above.");
    return;
}

Console.WriteLine($"'Churn Label' found at index {churnLabelIndex}");
Console.WriteLine($"'Total Charges' found at index {totalChargesIndex}");

var churnCounts = dataRows
    .Select(row => row.Split(',')[churnLabelIndex])
    .GroupBy(v => v)
    .Select(g => new { Value = g.Key, Count = g.Count() });

Console.WriteLine("\n--- Churn Label Distribution ---");
foreach (var c in churnCounts)
{
    var pct = (c.Count / (double)dataRows.Length) * 100;
    Console.WriteLine($"{c.Value}: {c.Count} ({pct:F1}%)");
}

var blankTotalCharges = dataRows
    .Select(row => row.Split(',')[totalChargesIndex])
    .Count(v => string.IsNullOrWhiteSpace(v));

Console.WriteLine($"\nBlank/empty 'Total Charges' values: {blankTotalCharges}");
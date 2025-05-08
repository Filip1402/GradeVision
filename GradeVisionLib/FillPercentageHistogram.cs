using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FillPercentageHistogram
{
    public static void GenerateHistogramAndSaveImage(List<double> fillPercentages, double threshold, string outputFileName)
    {
        var model = new PlotModel
        {
            Title = "Fill Percentage Histogram",
            Background = OxyColors.White,
            Axes =
            {
                new LinearAxis { Position = AxisPosition.Bottom, Title = "Fill % Bin", Minimum = 0, Maximum = 100 },
                new LinearAxis { Position = AxisPosition.Left, Title = "Count" }
            }
        };

        int[] bins = new int[101];
        foreach (var fill in fillPercentages)
        {
            int bin = (int)Math.Floor(fill);
            if (bin is >= 0 and <= 100)
                bins[bin]++;
        }

        var bars = new RectangleBarSeries
        {
            FillColor = OxyColors.SkyBlue,
            StrokeColor = OxyColors.Black,
            StrokeThickness = 1
        };

        for (int i = 0; i <= 100; i++)
            bars.Items.Add(new RectangleBarItem(i, 0, i + 1, bins[i]));

        var thresholdLine = new LineSeries
        {
            Color = OxyColors.Red,
            StrokeThickness = 2,
            LineStyle = LineStyle.Solid,
            Title = $"Threshold = {threshold:F2}",
            Points =
            {
                new DataPoint(threshold, 0),
                new DataPoint(threshold, bins.Max())
            }
        };

        model.Series.Add(bars);
        model.Series.Add(thresholdLine);

        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "ProcessedImages", outputFileName, "histogram.png");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var stream = File.Create(path);
        new PngExporter(600, 400, 96).Export(model, stream);

        Console.WriteLine($"Histogram saved to: {path}");
    }
}
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class HistogramGenerator
{
    public static double CalculateFillPercentageTreshold(List<double> fillPercentages)
    {
        var histogram = new List<int>(new int[101]);
        fillPercentages.ForEach(fill => histogram[(int)Math.Floor(fill)]++);

        var startOfFirstPeak = histogram.FindIndex(x => x > 0);
        var endOfFirstPeak = histogram.Skip(startOfFirstPeak + 1).ToList().FindIndex(x => x == 0) + startOfFirstPeak + 1;

        while (endOfFirstPeak + 2 < histogram.Count &&
               (histogram[endOfFirstPeak + 1] > 0 || histogram[endOfFirstPeak + 2] > 0))
        {
            endOfFirstPeak++;
        }

        return endOfFirstPeak;
    }

    public static void GenerateHistogramAndSaveImage(List<double> fillPercentages, double threshold, string outputFileName)
    {
        var bins = ComputeBins(fillPercentages);
        var model = CreatePlotModel(bins, threshold);
        var filePath = GetOutputFilePath(outputFileName);
        SavePlotModelAsPng(model, filePath);
    }

    private static int[] ComputeBins(List<double> fillPercentages)
    {
        int[] bins = new int[101];
        foreach (var fill in fillPercentages)
        {
            int bin = (int)Math.Floor(fill);
            if (bin is >= 0 and <= 100)
                bins[bin]++;
        }
        return bins;
    }

    private static PlotModel CreatePlotModel(int[] bins, double threshold)
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
        return model;
    }

    private static string GetOutputFilePath(string outputFileName)
    {
        string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "ProcessedImages", outputFileName);

        Directory.CreateDirectory(folderPath);
        return Path.Combine(folderPath, "histogram.png");
    }

    private static void SavePlotModelAsPng(PlotModel model, string filePath)
    {
        using var stream = File.Create(filePath);
        new PngExporter(600, 400, 96).Export(model, stream);
    }
}

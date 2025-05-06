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
            Background = OxyColors.White
        };

        // Axes
        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Fill % Bin",
            Minimum = 0,
            Maximum = 100
        });

        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Count"
        });

        // Histogram data
        int[] histogramData = new int[101];
        foreach (var fill in fillPercentages)
        {
            int bin = (int)Math.Floor(fill);
            if (bin >= 0 && bin <= 100)
                histogramData[bin]++;
        }

        // Use RectangleBarSeries to simulate a histogram
        var barSeries = new RectangleBarSeries
        {
            FillColor = OxyColors.SkyBlue,
            StrokeColor = OxyColors.Black,
            StrokeThickness = 1
        };

        for (int i = 0; i <= 100; i++)
        {
            double x0 = i;
            double x1 = i + 1;
            double y0 = 0;
            double y1 = histogramData[i];
            barSeries.Items.Add(new RectangleBarItem(x0, y0, x1, y1));
        }

        model.Series.Add(barSeries);

        // Add vertical threshold line
        var line = new LineSeries
        {
            Color = OxyColors.Red,
            StrokeThickness = 2,
            LineStyle = LineStyle.Solid,
            Title = $"Threshold = {threshold:F2}"
        };

        line.Points.Add(new DataPoint(threshold, 0));
        line.Points.Add(new DataPoint(threshold, histogramData.Max()));
        model.Series.Add(line);

        // Export the image using PngExporter
        var exporter = new PngExporter(600, 400, 96);
        string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProcessedImages");
        outputPath = Path.Combine(outputPath, outputFileName);
        outputPath = Path.Combine(outputPath, "histogram.png");
        string directory = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (var stream = File.Create(outputPath))
        {
            exporter.Export(model, stream);
        }

        Console.WriteLine($"Histogram saved to: {outputPath}");
    }
}

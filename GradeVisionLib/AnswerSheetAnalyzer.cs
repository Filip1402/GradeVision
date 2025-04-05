using Emgu.CV;
using Emgu.CV.Structure;
using GradeVisionLib.Impl;
using GradeVisionLib.Interfaces;
using GradeVisionLib.Models;

namespace GradeVisionLib
{
    public class AnswerSheetAnalyzer
    {
        private readonly IImageProcessor _imageProcessor;
        private readonly string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProcessedImages");
        private int stepCounter = 1;
        private string LatestFileName = "";
        private Dictionary<int, List<DetectedCircleBase>> ControlAnswers;
        public AnswerSheetAnalyzer(IImageProcessor imageProcessor, string imageName)
        {
            _imageProcessor = imageProcessor;
            outputDir += "/" + imageName;
            // Ensure output directory exists
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
        }

        public (string, string, double) ProcessControlSheet(string imagePath)
        {
            ImageData image = _imageProcessor.LoadImage(imagePath);
            SaveStep(image, "00_Raw.png");

            var preprocessingSteps = new List<Func<ImageData, ImageData>>
                {
                    ConvertToGrayscale,
                    CorrectPerspective,
                    CorrectRotation,
                    Denoise,
                    ApplyThresholding,
                };

            foreach (var step in preprocessingSteps)
            {
                image = step(image);
            }

            (image, ControlAnswers) = CircleDetection(image);
            return (outputDir + "/" + LatestFileName, "grade", 0);
        }

        public (string, string, double) ProcessAnswerSheet(string imagePath)
        {
            ImageData image = _imageProcessor.LoadImage(imagePath);
            SaveStep(image, "00_Raw.png");

            var preprocessingSteps = new List<Func<ImageData, ImageData>>
                {
                    ConvertToGrayscale,
                    CorrectPerspective,
                    CorrectRotation,
                    Denoise,
                    ApplyThresholding,
                };

            foreach (var step in preprocessingSteps)
            {
                image = step(image);
            }

            (image, var studentAnswer) = CircleDetection(image);
            TestGrader grader = new TestGrader(
                new GradeScale(new List<string> { "1", "2", "3", "4", "5" }, new List<double> { 50.00, 63.00, 75.00, 85.00 }),
                studentAnswer,
                ControlAnswers
                );

            var (grade, score) = grader.GetGrade();

            return (outputDir + "/" + LatestFileName, grade, score);
        }
        private ImageData ConvertToGrayscale(ImageData image) => ProcessStep(image, _imageProcessor.ConvertToGrayscale);

        private ImageData CorrectRotation(ImageData image) => ProcessStep(image, _imageProcessor.CorrectRotation);

        private ImageData Denoise(ImageData image) => ProcessStep(image, _imageProcessor.Denoise);

        private ImageData CorrectPerspective(ImageData image) => ProcessStep(image, _imageProcessor.CorrectPerspective);

        private ImageData ApplyThresholding(ImageData image) => ProcessStep(image, _imageProcessor.ApplyThresholding);

        private (ImageData, Dictionary<int, List<DetectedCircleBase>>) CircleDetection(ImageData image) => ProcessStep(image, _imageProcessor.CircleDetection);

        private ImageData ProcessStep(ImageData image, Func<ImageData, ImageData> processingFunc)
        {
            ImageData result = processingFunc(image);
            string functionName = processingFunc.Method.Name;
            SaveStep(result, GetStepFileName(functionName));
            return result;
        }

        private (ImageData, Dictionary<int, List<DetectedCircleBase>>) ProcessStep(ImageData image, Func<ImageData, (ImageData, Dictionary<int, List<DetectedCircleBase>>)> processingFunc)
        {
            (ImageData, Dictionary<int, List<DetectedCircleBase>>) results = processingFunc(image);
            string functionName = processingFunc.Method.Name;
            SaveStep(results.Item1, GetStepFileName(functionName));
            return results;
        }

        private void SaveStep(ImageData image, string fileName)
        {
            SaveImage(image, fileName);
        }

        private string SaveImage(ImageData image, string fileName)
        {
            string filePath = Path.Combine(outputDir, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            CvInvoke.Imwrite(filePath, (image as EmguCvImage).ToMat());
            return filePath;
        }

        private string GetStepFileName(string stepName)
        {
            var fileName = $"{stepCounter++.ToString("D2")}_{stepName}.png";
            LatestFileName = fileName;
            return fileName;
        }
    }
}

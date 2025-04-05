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
        private int stepCounter = 1;
        private string LatestFileName = "";
        private Dictionary<int, List<DetectedCircleBase>> ControlAnswers;
        private string imageName; 
        public AnswerSheetAnalyzer(IImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
        }

        public (string, string, double) ProcessControlSheet(string imagePath)
        {
            imageName = "control";
            string outputDir = PrepareOutputDirectory(imageName);
            ImageData image = ProcessImage(imagePath, outputDir);
            (image, ControlAnswers) = CircleDetection(image);
            string imagePathWithFileName = Path.Combine(outputDir, LatestFileName);
            return (imagePathWithFileName, "grade", 0);
        }

        public (string, string, double) ProcessAnswerSheet(string imagePath, string imageName)
        {
            this.imageName = imageName;
            string outputDir = PrepareOutputDirectory(imageName);
            ImageData image = ProcessImage(imagePath, outputDir);
            (image, var studentAnswer) = CircleDetection(image);

            TestGrader grader = new TestGrader(
                new GradeScale(new List<string> { "1", "2", "3", "4", "5" }, new List<double> { 50.00, 63.00, 75.00, 85.00 }),
                studentAnswer,
                ControlAnswers
            );

            var (grade, score) = grader.GetGrade();

            // Use Path.Combine to build the correct file path
            string imagePathWithFileName = Path.Combine(outputDir, LatestFileName);
            return (imagePathWithFileName, grade, score);
        }


        private ImageData ProcessImage(string imagePath, string outputDir)
        {
            // Load initial image
            ImageData image = _imageProcessor.LoadImage(imagePath);
            SaveStep(image, outputDir + "/" + "00_Raw.png");

            // List of preprocessing steps
            var preprocessingSteps = new List<Func<ImageData, ImageData>>
            {
                ConvertToGrayscale,
                CorrectPerspective,
                CorrectRotation,
                Denoise,
                ApplyThresholding
            };

            // Apply all preprocessing steps
            foreach (var step in preprocessingSteps)
            {
                image = step(image);
            }

            return image;
        }

        private string PrepareOutputDirectory(string imageName)
        {
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProcessedImages", imageName);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            return outputDir;
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
            System.Diagnostics.Debug.WriteLine($"Processing step: {functionName}");
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
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProcessedImages", imageName, fileName);
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

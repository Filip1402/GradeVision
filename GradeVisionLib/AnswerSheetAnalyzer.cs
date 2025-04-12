using Emgu.CV;
using Emgu.CV.Structure;
using GradeVisionLib.Impl;
using GradeVisionLib.Interfaces;
using GradeVisionLib.Models;
using Lombok.NET;
using System.ComponentModel.DataAnnotations;

namespace GradeVisionLib
{
    [RequiredArgsConstructor(MemberType = MemberType.Field, AccessTypes = AccessTypes.Private)]
    public partial class AnswerSheetAnalyzer
    {
        private readonly IImageProcessor _imageProcessor;

        private int stepCounter = 1;
        private string currentImageName;

        public (ImageData, Dictionary<int, List<DetectedCircleBase>> ControlAnswers) ProcessControlSheet(string imagePath)
        {
            currentImageName = "control";
            string outputDir = PrepareOutputDirectory(currentImageName);
            var (rawImage, proccedImage) = ProcessImage(imagePath, outputDir);
            (proccedImage, var controlAnswers) = CircleDetection(proccedImage);
            (proccedImage) = AnswerVisualization(rawImage, controlAnswers);
            return (proccedImage, controlAnswers);
        }

        public (ImageData, string, double) ProcessAnswerSheet(string imagePath, string imageName, Dictionary<int, List<DetectedCircleBase>> controlAnswers)
        {
            this.currentImageName = imageName;
            string outputDir = PrepareOutputDirectory(imageName);
            var (rawImage, proccedImage) = ProcessImage(imagePath, outputDir);
            (proccedImage, var studentAnswers) = CircleDetection(proccedImage);

            TestGrader grader = new TestGrader(
                new GradeScale(new List<string> { "1", "2", "3", "4", "5" }, new List<double> { 50.00, 63.00, 75.00, 85.00 }),
                studentAnswers,
                controlAnswers
            );

            var (grade, score) = grader.GetGrade();

            (proccedImage) = AnswerVisualization(rawImage, studentAnswers);
            (proccedImage) = GradeVisualization(proccedImage, grade, score);

            return (proccedImage, grade, score);
        }

        private (ImageData, ImageData) ProcessImage(string imagePath, string outputDir)
        {
            ImageData rawImage = _imageProcessor.LoadImage(imagePath);
            ImageData proccedImage = rawImage.Clone();
            SaveStep(proccedImage, outputDir + "/" + "00_Raw.png");

            var preprocessingSteps = new List<Func<ImageData, ImageData>>
                {
                    ConvertToGrayscale,
                    CorrectPerspective,
                    CorrectRotation,
                    Denoise,
                    ApplyThresholding
                };

            foreach (var step in preprocessingSteps)
            {
                proccedImage = step(proccedImage);

                if (step == CorrectRotation)
                {
                    rawImage = proccedImage.Clone();
                }
            }

            return (rawImage, proccedImage);
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

        private ImageData AnswerVisualization(ImageData inputImage, Dictionary<int, List<DetectedCircleBase>> questionAnswers)
        {
            return ProcessStep(inputImage, questionAnswers, _imageProcessor.VisualizeAnswers);
        }
        private ImageData GradeVisualization(ImageData inputImage, string grade, double score)
        {
            return ProcessStep(inputImage, grade, score, _imageProcessor.VisualizeGrade);
        }

        private ImageData ProcessStep(ImageData image, string grade, double score, Func<ImageData, string, double, ImageData> processingFunc)
        {
            ImageData result = processingFunc(image, grade, score);
            string functionName = processingFunc.Method.Name;
            System.Diagnostics.Debug.WriteLine($"Processing step: {functionName}");
            SaveStep(result, GetStepFileName(functionName));
            return result;
        }

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
            System.Diagnostics.Debug.WriteLine($"Processing step: {functionName}");
            SaveStep(results.Item1, GetStepFileName(functionName));
            return results;
        }

        private ImageData ProcessStep(ImageData image, Dictionary<int, List<DetectedCircleBase>> questionAnswers, Func<ImageData, Dictionary<int, List<DetectedCircleBase>>, ImageData> processingFunc)
        {
            ImageData result = processingFunc(image, questionAnswers);
            string functionName = processingFunc.Method.Name;
            System.Diagnostics.Debug.WriteLine($"Processing step: {functionName}");
            SaveStep(result, GetStepFileName(functionName));

            return result;
        }

        private void SaveStep(ImageData image, string fileName)
        {
            SaveImage(image, fileName);
        }

        private string SaveImage(ImageData image, string fileName)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProcessedImages", currentImageName, fileName);
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
            return fileName;
        }
    }
}

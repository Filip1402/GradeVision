using Emgu.CV;
using Emgu.CV.Structure;
using GradeVisionLib.Impl;
using GradeVisionLib.Interfaces;
using GradeVisionLib.Models;
using Lombok.NET;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace GradeVisionLib
{
    [RequiredArgsConstructor(MemberType = MemberType.Field, AccessTypes = AccessTypes.Private)]
    public partial class AnswerSheetGrader
    {
        private readonly ImageProcessorBase _imageProcessor;
        private readonly string outputPath;
        private int stepCounter = 0;
        private string currentImageName;
        private const string CONTROL_FILE_NAME = "control.png";

        public (ImageData, Dictionary<int, List<DetectedCircleBase>> ControlAnswers) ProcessControlSheet(ImageData inputImage)
        {
            currentImageName = CONTROL_FILE_NAME;
            inputImage.Name = CONTROL_FILE_NAME;
            string outputDir = PrepareOutputDirectory(currentImageName);
            var (rawImage, proccedImage) = ProcessImage(inputImage, outputDir);
            (proccedImage, var controlAnswers) = ProcessStep(() => _imageProcessor.CircleDetection(proccedImage), "CircleDetection");
            proccedImage = ProcessStep(() => _imageProcessor.VisualizeDetectedCircles(rawImage, controlAnswers), "VisualizeControlCircles");

            ResetStepCounter();
            return (proccedImage, controlAnswers);
        }

        public (ImageData, string, double) ProcessAnswerSheet(ImageData inputImage, Dictionary<int, List<DetectedCircleBase>> controlAnswers, GradeScale gradeScale)
        {
            currentImageName = inputImage.Name;
            string outputDir = PrepareOutputDirectory(currentImageName);
            var (rawImage, proccedImage) = ProcessImage(inputImage, outputDir);
            (proccedImage, var studentAnswers) = ProcessStep(() => _imageProcessor.CircleDetection(proccedImage), "CircleDetection");

            GradeCalculator grader = new GradeCalculator(
                gradeScale,
                studentAnswers,
                controlAnswers
            );

            var (grade, score) = grader.GetGrade();

            ProcessStep(() => _imageProcessor.VisualizeDetectedCircles(rawImage, studentAnswers), "VisualizeStudentCircles");
            proccedImage = ProcessStep(() => _imageProcessor.VisualizeGrade(rawImage, studentAnswers, controlAnswers, grade, score), "VisualizeGrade");

            ResetStepCounter();
            return (proccedImage, grade, score);
        }

        private (ImageData, ImageData) ProcessImage(ImageData rawImage, string outputDir)
        {
            ImageData proccedImage = rawImage.Clone();

            var preprocessingSteps = new List<(string Name, Func<ImageData, ImageData> Step)>
            {
                ("Raw", image => image),
                ("ConvertToGrayscale", image => _imageProcessor.ConvertToGrayscale(image)),
                ("CorrectPerspective", image => _imageProcessor.CorrectPerspective(image)),
                ("CorrectRotation", image => _imageProcessor.CorrectRotation(image)),
                ("Denoise", image => _imageProcessor.Denoise(image)),
                ("ApplyThresholding", image => _imageProcessor.ApplyThresholding(image))
            };

            foreach (var (name, stepFunc) in preprocessingSteps)
            {
                proccedImage = ProcessStep(() => stepFunc(proccedImage), name);

                if (name == "CorrectRotation")
                {
                    rawImage = proccedImage.Clone();
                }
            }

            return (rawImage, proccedImage);

        }
        private T ProcessStep<T>(Func<T> processingFunc, string? stepName = null)
        {
            var result = processingFunc();
            var methodName = stepName ?? processingFunc.Method.Name;
            DebugProcessStepIfNeeded(result, methodName);
            return result;
        }

        #region Helper methods
        private void DebugProcessStepIfNeeded<T>(T? result, string methodName)
        {
            if (_imageProcessor.isDebugModeEnabled)
            {
                switch (result)
                {
                    case ImageData image:

                        SaveStep(image, GetStepFileName(methodName));

                        break;
                    case ValueTuple<ImageData, Dictionary<int, List<DetectedCircleBase>>> tuple:

                        SaveStep(tuple.Item1, GetStepFileName(methodName));

                        break;
                }
                System.Diagnostics.Debug.WriteLine($"Processing step: {methodName}");
            }
        }

        private void SaveStep(ImageData image, string fileName)
        {
            SaveImage(image, fileName);
        }

        private string PrepareOutputDirectory(string imageName)
        {
            string outputDir = Path.Combine(outputPath, imageName);
            if (_imageProcessor.isDebugModeEnabled &&  !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            return outputDir;
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

        private void ResetStepCounter()
        {
            stepCounter = 0;
        }

        #endregion
    }
}

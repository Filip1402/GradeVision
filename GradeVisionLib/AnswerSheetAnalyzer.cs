using Emgu.CV;
using GradeVisionLib.Interfaces;

namespace GradeVisionLib
{
    public class AnswerSheetAnalyzer
    {
        private readonly IImageProcessor _imageProcessor;
        private readonly string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProcessedImages");
        private int stepCounter = 1;

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
        public string ProcessAnswerSheet(string imagePath)
        {
            Mat image = _imageProcessor.LoadImage(imagePath);
            SaveStep(image, "00_Raw.png");

            // List of processing steps in order
            var steps = new List<Func<Mat, Mat>>
            {
                ConvertToGrayscale,
                CorrectPerspective,
                CorrectRotation,
                Denoise,
                ApplyThresholding,
                ApplyContours,
                //DetectAndCropAnswerRegion,
                ApplyCannyEdgeDetection
            };

            foreach (var step in steps)
            {
                image = step(image);
            }

            return DetectXMarks(image);
        }
        private Mat ConvertToGrayscale(Mat image) => ProcessStep(image, _imageProcessor.ConvertToGrayscale);

        private Mat CorrectRotation(Mat image) => ProcessStep(image, _imageProcessor.CorrectRotation);

        private Mat Denoise(Mat image) => ProcessStep(image, _imageProcessor.Denoise);

        private Mat CorrectPerspective(Mat image) => ProcessStep(image, _imageProcessor.CorrectPerspective);

        private Mat ApplyThresholding(Mat image) => ProcessStep(image, _imageProcessor.ApplyThresholding);

        private Mat ApplyContours(Mat image) => ProcessStep(image, _imageProcessor.ApplyContours);


        private Mat ApplyCannyEdgeDetection(Mat image) => ProcessStep(image, _imageProcessor.ApplyCannyEdgeDetection);

        private string DetectXMarks(Mat image)
        {
            return _imageProcessor.DetectXMarks(image, image);
        }

        private Mat ProcessStep(Mat image, Func<Mat, Mat> processingFunc)
        {
            Mat result = processingFunc(image);
            string functionName = processingFunc.Method.Name;
            SaveStep(result, GetStepFileName(functionName));
            return result;
        }

        private void SaveStep(Mat image, string fileName)
        {
            SaveImage(image, fileName);
        }

        private string SaveImage(Mat image, string fileName)
        {
            string filePath = Path.Combine(outputDir, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            CvInvoke.Imwrite(filePath, image);
            return filePath;
        }

        private string GetStepFileName(string stepName)
        {
            return $"{stepCounter++.ToString("D2")}_{stepName}.png";
        }
    }
}

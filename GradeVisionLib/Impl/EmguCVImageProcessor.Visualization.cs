using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GradeVisionLib.Interfaces;
using System.IO;


namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
        public ImageData VisualizeAnswers(ImageData inputImage, Dictionary<int, List<DetectedCircleBase>> questionAnswers)
        {

            var inputMat = (inputImage as EmguCvImage).ToMat();
            inputMat = ConvertToColorIfNeeded(inputMat);
            var outputMat = inputMat.Clone();


            foreach (var question in questionAnswers)
            {
                foreach (var circle in question.Value)
                {
                    // Draw circle based on whether it is marked or not
                    CvInvoke.Circle(
                        outputMat,
                        new System.Drawing.Point((int)circle.X, (int)circle.Y),
                        (int)circle.Radius,
                        circle.IsMarked ? MARKED_CIRCLE_COLOR : UNMARKED_CIRCLE_COLOR,
                        2 // thickness
                    );
                }
            }

            return EmguCvImage.FromMat(outputMat); // Return the image with drawn circles
        }

        // This method visualizes the grade and score on the image
        public ImageData VisualizeGrade(ImageData inputImage, string grade, double score)
        {
            var inputMat = (inputImage as EmguCvImage).ToMat();
            var outputMat = inputMat.Clone(); // Clone to keep the original image unchanged

            // Set the grade and score text
            string gradeText = $"Grade: {grade}";
            string scoreText = $"Score: {score:F2}"; // Display score with two decimal places

            // Font parameters
            var fontFace = FontFace.HersheySimplex;
            var fontScale = 1.0;
            var fontThickness = 2;

            // Set positions for text in the top-right corner
            var gradePosition = new System.Drawing.Point(outputMat.Width - 300, 50);
            var scorePosition = new System.Drawing.Point(outputMat.Width - 300, 100);

            // Draw the grade and score text on the image
            CvInvoke.PutText(outputMat, gradeText, gradePosition, fontFace, fontScale, new MCvScalar(0, 0, 255), fontThickness);
            CvInvoke.PutText(outputMat, scoreText, scorePosition, fontFace, fontScale, new MCvScalar(0, 0, 255), fontThickness);

            return EmguCvImage.FromMat(outputMat); // Return the image with grade and score
        }

        private Mat ConvertToColorIfNeeded(Mat inputMat)
        {
            // If the input image is grayscale (1 channel), convert it to BGR (3 channels)
            if (inputMat.NumberOfChannels == 1)
            {
                CvInvoke.CvtColor(inputMat, inputMat, ColorConversion.Gray2Bgr); // Convert grayscale to BGR
            }
            return inputMat;
        }
    }
}

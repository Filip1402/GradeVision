using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GradeVisionLib.Interfaces;
using System.Drawing;
using System.IO;


namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : ImageProcessorBase
    {   
        private static readonly FontFace FONT = FontFace.HersheySimplex;
        private static readonly double FONT_SCALE = 1f;
        private static readonly int FONT_THICKNESS = 2;

        override public ImageData VisualizeDetectedCircles(ImageData inputImage, Dictionary<int, List<DetectedCircleBase>> questionAnswers)
        {

            var inputMat = getMat(inputImage);
            inputMat = ConvertToColorIfNeeded(inputMat);
            var outputMat = inputMat.Clone();

            foreach (var question in questionAnswers)
            {
                foreach (var circle in question.Value)
                {
                    var color = circle.IsMarked ? GREEN_EMGU_CV_COLOR : RED_EMGU_CV_COLOR;
                    DrawCircle(outputMat, circle, color, 2);
                }
            }
            return EmguCvImage.FromMat(outputMat, inputImage.Name);
        }

        override public ImageData VisualizeGrade(ImageData inputImage, Dictionary<int, List<DetectedCircleBase>> questionAnswers, 
            Dictionary<int, List<DetectedCircleBase>> controlAnswers,  string grade, double score)
        {
            var inputMat = getMat(inputImage);
            var outputMat = inputMat.Clone();
            RenderAnswerFeedbackIfPossible(questionAnswers, controlAnswers, score, outputMat);
            DrawGradeAndScore(grade, score, outputMat);

            return EmguCvImage.FromMat(outputMat, inputImage.Name);
        }

        private void RenderAnswerFeedbackIfPossible(
            Dictionary<int, List<DetectedCircleBase>> questionAnswers,
            Dictionary<int, List<DetectedCircleBase>> controlAnswers,
            double score,
            Mat outputMat)
        {
            if (score == -100) return;

            for (int i = 0; i < questionAnswers.Count; i++)
            {
                var userCircles = questionAnswers.ElementAt(i).Value;
                var correctCircles = controlAnswers.ElementAt(i).Value;

                userCircles
                    .Select((circle, j) => new
                    {
                        Circle = circle,
                        Color = correctCircles.ElementAt(j).IsMarked == true
                            ? GREEN_EMGU_CV_COLOR
                            : RED_EMGU_CV_COLOR
                    })
                    .Where(x => x.Circle.IsMarked)
                    .ToList()
                    .ForEach(x => DrawCircle(outputMat, x.Circle, x.Color, 2));
            }
        }

        private void DrawGradeAndScore(string grade, double score, Mat outputMat)
        {
            string gradeText = $"Grade: {grade}";
            string scoreText = $"Score: {score:F2}";

            var gradePosition = new Point(outputMat.Width - 300, 50);
            var scorePosition = new Point(outputMat.Width - 300, 100);

            CvInvoke.PutText(outputMat, gradeText, gradePosition, FONT, FONT_SCALE, RED_EMGU_CV_COLOR, FONT_THICKNESS);
            CvInvoke.PutText(outputMat, scoreText, scorePosition, FONT, FONT_SCALE, RED_EMGU_CV_COLOR, FONT_THICKNESS);
        }

        private Mat ConvertToColorIfNeeded(Mat inputMat)
        {
            if (inputMat.NumberOfChannels == 1)
            {
                CvInvoke.CvtColor(inputMat, inputMat, ColorConversion.Gray2Bgr);
            }
            return inputMat;
        }
    }
}

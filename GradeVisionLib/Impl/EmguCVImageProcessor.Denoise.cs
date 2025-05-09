using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GradeVisionLib.Interfaces;
using System.Drawing;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : ImageProcessorBase
    {
        private const double NOISE_LEVEL_THRESHOLD = 1.5;
        private const float DENOISE_STRENGH = 10;

        override public ImageData Denoise(ImageData inputImage)
        {
            var inputMat = getMat(inputImage);
            var noiseLevel = EstimateNoiseLevel(inputMat);

            if (noiseLevel >= NOISE_LEVEL_THRESHOLD)
            {
                inputMat = ApplyNonLocalMeansDenoising(inputMat);
                AddOperationTextifNeeded(inputMat, $"{noiseLevel:F2} NL-Means");
            }
            else
            {
                AddOperationTextifNeeded(inputMat, $"{noiseLevel:F2} No Denoise");
            }

            return EmguCvImage.FromMat(inputMat, inputImage.Name);
        }

        #region Supporting Methods

        private double EstimateNoiseLevel(Mat image)
        {
            using (Mat temp = new Mat())
            {
                CvInvoke.MedianBlur(image, temp, 3);
                CvInvoke.Subtract(image, temp, temp);
                temp.ConvertTo(temp, DepthType.Cv32F);

                var mean = new MCvScalar();
                var stddev = new MCvScalar();
                CvInvoke.MeanStdDev(temp, ref mean, ref stddev);
                return stddev.V0;
            }
        }

        private Mat ApplyNonLocalMeansDenoising(Mat image)
        {
            var result = new Mat();
            CvInvoke.FastNlMeansDenoising(image, result,
                h: DENOISE_STRENGH,
                templateWindowSize: 5,
                searchWindowSize: 15);
            return result;
        }

        private void AddOperationTextifNeeded(Mat image, string operationName)
        {
            if (!isDebugModeEnabled)
                return;

            var text = operationName;
            CvInvoke.PutText(
                image,
                text,
                new Point(10, 30), 
                FONT,
                FONT_SCALE,
                GREEN_EMGU_CV_COLOR,
                2);
        }

        #endregion
    }
}

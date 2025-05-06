using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GradeVisionLib.Interfaces;
using System.Drawing;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
        private const double NOISE_LEVEL_THRESHOLD = 1.5;
        private const float DENOISE_STRENGH = 10;


        public ImageData Denoise(ImageData inputImage)
        {
            var inputMat = (inputImage as EmguCvImage).ToMat();
            double noiseLevel = EstimateNoiseLevel(inputMat);

            if (noiseLevel >= NOISE_LEVEL_THRESHOLD)
            {
                inputMat = ApplyNonLocalMeansDenoising(inputMat);
                //debug
                AddOperationText(inputMat, $"{noiseLevel:F2} NL-Means");
            }
            else
            {
                //debug
                AddOperationText(inputMat, $"{noiseLevel:F2} No Denoise");
            }

            return EmguCvImage.FromMat(inputMat, inputImage.Name);
        }

        #region Supporting Methods

        private double EstimateNoiseLevel(Mat image)
        {
            using (Mat temp = new Mat())
            {
                CvInvoke.MedianBlur(image, temp, 3);
                CvInvoke.Subtract(image, temp, temp); // Subtract original from blurred
                temp.ConvertTo(temp, DepthType.Cv32F);

                MCvScalar mean = new MCvScalar();
                MCvScalar stddev = new MCvScalar();
                CvInvoke.MeanStdDev(temp, ref mean, ref stddev);
                return stddev.V0;
            }
        }

        private Mat ApplyNonLocalMeansDenoising(Mat image)
        {
            Mat result = new Mat();
            CvInvoke.FastNlMeansDenoising(image, result,
                h: DENOISE_STRENGH,
                templateWindowSize: 5,
                searchWindowSize: 15);
            return result;
        }

        private void AddOperationText(Mat image, string operationName)
        {
            string text = operationName;
            var font = new Font("Arial", 40);
            CvInvoke.PutText(image, text,
                new Point(10, 30), FontFace.HersheySimplex,
                fontScale: 1.0,
                color: new MCvScalar(0, 255, 0), thickness: 2);
        }

        #endregion
    }
}

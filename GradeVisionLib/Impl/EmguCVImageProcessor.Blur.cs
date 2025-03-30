using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GradeVisionLib.Interfaces;
using System.Drawing;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
        public Mat Denoise(Mat image)
        {
            double noiseLevel = EstimateNoiseLevel(image);

            if (noiseLevel >= 2) // High/mid noise 
            {
                image = ApplyNonLocalMeansDenoising(image, h: 10.0); // Mild edge-preserving denoising
                AddOperationText(image, $"{noiseLevel:F2} NL-Means");
            }
            else // Very clean
            {
                // Skip denoising to preserve maximum detail
                AddOperationText(image, $"{noiseLevel:F2} No Denoise");
            }

            return image;
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

        private Mat ApplyNonLocalMeansDenoising(Mat image, double h = 3.0)
        {
            Mat result = new Mat();
            CvInvoke.FastNlMeansDenoising(image, result,
                h: (float)h,
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
                color: new MCvScalar(0, 255, 0), thickness: 2); // Green text overlay
        }

        #endregion
    }
}

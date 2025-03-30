using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GradeVisionLib.Interfaces;
using System.Drawing;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
        public Mat ApplyBlur(Mat image)
        {
            // Estimate noise level (using standard deviation of pixel intensities)
            double noiseLevel = EstimateNoiseLevel(image);

            // Apply optimal blur or sharpen based on noise level
            if (noiseLevel > 50)
            {
                image = ApplyGaussianBlur(image, "Blurred");
            }
            else if (noiseLevel < 10)
            {
                AddOperationText(image, "Needs sharpening");
            }
            else
            {
                image = ApplyBilateralFilter(image, "Bilateral");
            }

            return image;
        }


        #region Blur and Noise Estimation
        private double EstimateNoiseLevel(Mat image)
        {

            MCvScalar mean = new MCvScalar();
            MCvScalar stddev = new MCvScalar();

            CvInvoke.MeanStdDev(image, ref mean, ref stddev);

            return stddev.V0;
        }

        private Mat ApplyGaussianBlur(Mat image, string operationName)
        {

            int kernelSize = 5;
            Mat blurredImage = new Mat();
            CvInvoke.GaussianBlur(image, blurredImage, new Size(kernelSize, kernelSize), 0);

            AddOperationText(blurredImage, operationName);

            return blurredImage;
        }

        private Mat ApplyBilateralFilter(Mat image, string operationName)
        {
            Mat filteredImage = new Mat();
            CvInvoke.BilateralFilter(image, filteredImage, 9, 75, 75, Emgu.CV.CvEnum.BorderType.Default);

            AddOperationText(filteredImage, operationName);

            return filteredImage;
        }

        private void AddOperationText(Mat image, string operationName)
        {
            string text = operationName;
            var font = new Font("Arial", 40);
            CvInvoke.PutText(image, text, new Point(10, 30), FontFace.HersheySimplex, 1.0, new MCvScalar(0, 255, 0), 2); // Green text
        }

        #endregion

    }
}

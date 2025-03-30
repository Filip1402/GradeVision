using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using GradeVisionLib.Interfaces;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
        public Mat ApplyContours(Mat inputMat)
        {
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                Mat hierarchy = new Mat();
                CvInvoke.FindContours(inputMat, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                // Check if contours are found
                if (contours.Size == 0)
                {
                    Console.WriteLine("No contours detected.");
                    return inputMat; // Return original image if no contours are found
                }

                // Create a white background for output image
                Mat outputMat = new Mat(inputMat.Size, DepthType.Cv8U, 3); // Create a white image
                outputMat.SetTo(new MCvScalar(255, 255, 255)); // Set all pixels to white

                // Draw contours on the white background
                for (int i = 0; i < contours.Size; i++)
                {
                    // Draw each contour with a specific color (e.g., green) and thickness
                    CvInvoke.DrawContours(outputMat, contours, i, new MCvScalar(0, 255, 0), 2); // Green color for contours
                }

                return outputMat;

            }
        }
    }
}

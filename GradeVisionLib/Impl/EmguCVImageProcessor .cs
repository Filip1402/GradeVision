using Emgu.CV;
using Emgu.CV.Structure;
using GradeVisionLib.Interfaces;
using System.Drawing;
using System.IO;


namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {

        public ImageData LoadImage(string imagePath)
        {
            return EmguCvImage.FromFile(imagePath);
        }

        private string SaveImage(Mat image, string fileName)
        {
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProcessedImages");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            string filePath = Path.Combine(outputDir, fileName);
            CvInvoke.Imwrite(filePath, image);
            return filePath;
        }
        private void DrawCircle(Mat mat, DetectedCircleBase circle, MCvScalar color, int thickness = 1)
        {
            CvInvoke.Circle(
                mat,
                new Point((int)circle.X, (int)circle.Y),
                (int)circle.Radius,
                color,
                thickness
            );
        }

        private Mat getMat(ImageData image)
        {
            return (image as EmguCvImage).ToMat();
        }
    }
}

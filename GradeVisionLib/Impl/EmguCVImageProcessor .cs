using Emgu.CV;
using GradeVisionLib.Interfaces;
using System.IO;


namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {

        public Mat LoadImage(string imagePath)
        {
            return CvInvoke.Imread(imagePath);
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
    }
}

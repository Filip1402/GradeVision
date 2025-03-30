using Emgu.CV;
using GradeVisionLib.Interfaces;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
        public Mat ApplyCannyEdgeDetection(Mat image)
        {
            Mat edges = new Mat();
            CvInvoke.Canny(image, edges, 50, 150);
            return edges;
        }
    }
}

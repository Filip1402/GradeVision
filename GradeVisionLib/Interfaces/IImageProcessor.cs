using Emgu.CV;
using Emgu.CV.Structure;

namespace GradeVisionLib.Interfaces
{
    public interface IImageProcessor
    {
        Mat LoadImage(string imagePath);
        Mat CorrectRotation(Mat image);
        Mat ConvertToGrayscale(Mat inputMat);
        Mat Denoise(Mat inputMat);
        Mat ApplyThresholding(Mat inputMat);
        (Mat, Dictionary<int, List<DetectedCircleBase>>) CircleDetection(Mat inputMat);
        Mat CorrectPerspective(Mat inputMat);

    }
}

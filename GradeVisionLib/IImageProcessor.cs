using Emgu.CV;
using System.Drawing;

namespace GradeVisionLib
{
    public interface IImageProcessor
    {
        Mat LoadImage(string imagePath);
        Mat CorrectRotation(Mat image);
        Mat ConvertToGrayscale(Mat inputMat);
        Mat ApplyBlur(Mat inputMat);
        Mat ApplyThresholding(Mat inputMat);

        Mat ApplyContours(Mat inputMat);
        Rectangle DetectAnswerRegion(Mat binaryImage, Mat baseImage);
        Mat ApplyCannyEdgeDetection(Mat image);
        string DetectXMarks(Mat edges, Mat baseImage);

    }
}

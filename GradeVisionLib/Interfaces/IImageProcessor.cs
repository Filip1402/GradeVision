using Emgu.CV;

namespace GradeVisionLib.Interfaces
{
    public interface IImageProcessor
    {
        Mat LoadImage(string imagePath);
        Mat CorrectRotation(Mat image);
        Mat ConvertToGrayscale(Mat inputMat);
        Mat Denoise(Mat inputMat);
        Mat ApplyThresholding(Mat inputMat);

        Mat ApplyContours(Mat inputMat);

        Mat CorrectPerspective(Mat inputMat);
        Mat ApplyCannyEdgeDetection(Mat image);
        string DetectXMarks(Mat edges, Mat baseImage);

    }
}

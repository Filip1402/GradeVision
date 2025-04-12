using Emgu.CV;
using Emgu.CV.Structure;
using GradeVisionLib.Impl;

namespace GradeVisionLib.Interfaces
{
    public interface IImageProcessor
    {
        ImageData LoadImage(string imagePath);
        ImageData CorrectRotation(ImageData inputImage);
        ImageData ConvertToGrayscale(ImageData inputImage);
        ImageData Denoise(ImageData inputImage);
        ImageData ApplyThresholding(ImageData inputImage);
        (ImageData, Dictionary<int, List<DetectedCircleBase>>) CircleDetection(ImageData inputImage);
        ImageData CorrectPerspective(ImageData inputImage);
        ImageData VisualizeAnswers(ImageData inputImage, Dictionary<int, List<DetectedCircleBase>> questionAnswers);
        ImageData VisualizeGrade(ImageData inputImage, string grade, double score);
    }
}

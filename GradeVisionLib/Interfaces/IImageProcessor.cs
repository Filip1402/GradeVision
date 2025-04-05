using Emgu.CV;
using Emgu.CV.Structure;
using GradeVisionLib.Impl;

namespace GradeVisionLib.Interfaces
{
    public interface IImageProcessor
    {
        ImageData LoadImage(string imagePath);
        ImageData CorrectRotation(ImageData image);
        ImageData ConvertToGrayscale(ImageData inputMat);
        ImageData Denoise(ImageData inputMat);
        ImageData ApplyThresholding(ImageData inputMat);
        (ImageData, Dictionary<int, List<DetectedCircleBase>>) CircleDetection(ImageData inputMat);
        ImageData CorrectPerspective(ImageData inputMat);

    }
}

using Emgu.CV;
using Emgu.CV.Structure;
using GradeVisionLib.Impl;
using Lombok.NET;

namespace GradeVisionLib.Interfaces
{
    [RequiredArgsConstructor(AccessTypes =AccessTypes.Public)]
    public abstract partial class ImageProcessorBase
    {
        public readonly bool isDebugModeEnabled;
        public abstract ImageData LoadImage(string imagePath);
        public abstract ImageData CorrectRotation(ImageData inputImage);
        public abstract ImageData ConvertToGrayscale(ImageData inputImage);
        public abstract ImageData Denoise(ImageData inputImage);
        public abstract ImageData ApplyThresholding(ImageData inputImage);
        public abstract (ImageData, Dictionary<int, List<DetectedCircleBase>>) CircleDetection(ImageData inputImage);
        public abstract ImageData CorrectPerspective(ImageData inputImage);
        public abstract ImageData VisualizeDetectedCircles(ImageData inputImage, Dictionary<int, List<DetectedCircleBase>> questionAnswers);
        public abstract ImageData VisualizeGrade(ImageData inputImage, Dictionary<int, List<DetectedCircleBase>> questionAnswers,
             Dictionary<int, List<DetectedCircleBase>> controlAnswers, string grade, double score);
    }
}

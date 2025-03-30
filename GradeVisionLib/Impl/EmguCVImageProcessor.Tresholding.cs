using Emgu.CV;
using Emgu.CV.CvEnum;
using GradeVisionLib.Interfaces;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
	{
		public Mat ApplyThresholding(Mat inputMat)
		{
			Mat threshMat = new Mat();
			CvInvoke.AdaptiveThreshold(inputMat, threshMat, 255, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 5, 5);
			return threshMat;
		}
	}
}

using Emgu.CV;
using Emgu.CV.CvEnum;
using GradeVisionLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
        public ImageData ConvertToGrayscale(ImageData inputImage)
        {
            var inputMat = (inputImage as EmguCvImage).ToMat();
            Mat grayMat = new Mat();
            CvInvoke.CvtColor(inputMat, grayMat, ColorConversion.Bgr2Gray);
            return EmguCvImage.FromMat(grayMat, inputImage.Name);
        }
    }
}

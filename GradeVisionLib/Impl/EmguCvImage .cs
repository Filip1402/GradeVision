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
    public class EmguCvImage : ImageData
    {
        private Mat _mat;

        public EmguCvImage() : base("",0, 0, 0, null)
        {
            _mat = new Mat();
        }
        public EmguCvImage(Mat mat, string name) : base(name, mat.Width, mat.Height, mat.NumberOfChannels, GetDataFromMat(mat))
        {
            _mat = mat ?? throw new ArgumentNullException(nameof(mat));
        }
        public static EmguCvImage FromFile(string imagePath)
        {
            string fileName = Path.GetFileName(imagePath);
            Mat image = CvInvoke.Imread(imagePath, ImreadModes.Color);
            if (image.IsEmpty)
                throw new ArgumentException("Failed to load image from path.");
            return new EmguCvImage(image, fileName);
        }

        public static EmguCvImage FromMat(Mat mat, string imageName)
        {
            return new EmguCvImage(mat, imageName);
        }

        public Mat ToMat()
        {
            return _mat;
        }

        private static byte[] GetDataFromMat(Mat mat)
        {
            int totalBytes = mat.Width * mat.Height * mat.NumberOfChannels;
            byte[] buffer = new byte[totalBytes];
            System.Runtime.InteropServices.Marshal.Copy(mat.DataPointer, buffer, 0, totalBytes);
            return buffer;
        }

        public override ImageData Clone()
        {
            return new EmguCvImage(this._mat,this.Name);
        }
    }
}

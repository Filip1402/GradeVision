using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeVisionLib.Interfaces
{
    public abstract class ImageData
    {
        public int Width { get; }
        public int Height { get; }
        public int Channels { get; }
        public byte[] Data { get; }

        public ImageData(int width, int height, int channels, byte[] data)
        {
            Width = width;
            Height = height;
            Channels = channels;
            Data = data;
        }
        public abstract ImageData Clone();

    }
}

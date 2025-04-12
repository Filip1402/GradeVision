using Lombok.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeVisionLib.Interfaces
{
    public abstract class DetectedCircleBase
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Radius { get; set; }
        public bool IsMarked { get; set; }

        protected DetectedCircleBase(float x, float y, float radius)
        {
            X = x;
            Y = y;
            Radius = radius;
            IsMarked = false;
        }

        protected DetectedCircleBase() { }

        public void SetToMarked()
        {
            IsMarked = true;
        }
    }
}

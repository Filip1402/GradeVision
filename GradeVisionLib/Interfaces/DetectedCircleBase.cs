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
        public float X { get; }
        public float Y { get; }
        public float Radius { get; }
        public bool IsMarked { get; private set; }

        protected DetectedCircleBase(float x, float y, float radius)
        {
            X = x;
            Y = y;
            Radius = radius;
            IsMarked = false;
        }

        public void SetToMarked()
        {
            IsMarked = true;
        }
    }
}

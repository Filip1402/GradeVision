using Emgu.CV.Structure;
using GradeVisionLib.Interfaces;
using Lombok.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeVisionLib.Impl
{
    public class EmguCVCircle : DetectedCircleBase
    {
        private EmguCVCircle(float x, float y, float radius) : base(x, y, radius)
        {
        }

        public static EmguCVCircle FromCircleF(CircleF circle)
        {
            return new EmguCVCircle(circle.Center.X, circle.Center.Y, circle.Radius);
        }
    }
}

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

        public CircleRelation GetRelationTo(DetectedCircleBase other)
        {
            var dx = this.X - other.X;
            var dy = this.Y - other.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            var r1 = this.Radius;
            var r2 = other.Radius;

            if (distance <= r1 - r2)
            {
                return CircleRelation.OtherInsideThis;
            }
            else if (distance <= r2 - r1)
            {
                return CircleRelation.ThisInsideOther;
            }
            else if (distance < r1 + r2)
            {
                return CircleRelation.Intersecting;
            }
            else if (distance == r1 + r2)
            {
                return CircleRelation.Touching;
            }
            else
            {
                return CircleRelation.Separate;
            }
        }
    }

    public enum CircleRelation
    {
        OtherInsideThis,
        ThisInsideOther,
        Intersecting,
        Touching,
        Separate
    }
}


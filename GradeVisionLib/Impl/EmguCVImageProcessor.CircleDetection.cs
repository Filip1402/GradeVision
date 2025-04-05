using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using GradeVisionLib.Interfaces;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
        private static readonly MCvScalar UNMARKED_CIRCLE_COLOR = new MCvScalar(0, 0, 255);
        private static readonly MCvScalar MARKED_CIRCLE_COLOR = new MCvScalar(0, 255, 0);

        private const int MIN_CIRCLE_RADIUS = 8;
        private const int MAX_CIRCLE_RADIUS = 50;
        private const int MAX_VERTICAL_GROUP_DISTANCE = 15; //inPx
        public (ImageData, Dictionary<int, List<DetectedCircleBase>>) CircleDetection(ImageData input)
        {
            var inputMat = (input as EmguCvImage).ToMat();
            var outputMat = new EmguCvImage().ToMat();
            //for debug
            CvInvoke.CvtColor(inputMat, outputMat, ColorConversion.Gray2Bgr);

            var allCircles = DetectAllCircles(inputMat, outputMat);

            var sortedCircleGroups = GroupAndSortCirclesByYPosition(allCircles);

            var filteredGroups = FilterInvalidGroups(sortedCircleGroups, outputMat);

            double averageFillPercent = filteredGroups.Any()
                                        ? filteredGroups.SelectMany(group => group.Value)
                                        .Average(circle => GetFillPercentage(inputMat, (EmguCVCircle)circle))
                                        : 0;

            foreach (var group in filteredGroups)
            {
                foreach (EmguCVCircle circle in group.Value)
                {
                    double fillPercent = GetFillPercentage(inputMat, circle);
                    if (fillPercent > averageFillPercent)
                    {
                        circle.SetToMarked();
                    }
                    //debug
                    var color = fillPercent > averageFillPercent ?
                        MARKED_CIRCLE_COLOR :
                        UNMARKED_CIRCLE_COLOR;

                    CvInvoke.Circle(outputMat,
                        new Point((int)circle.X, (int)circle.Y),
                        (int)circle.Radius,
                        color,
                        2);
                }
            }

            return (EmguCvImage.FromMat(outputMat), filteredGroups);
        }

        private List<DetectedCircleBase> DetectAllCircles(Mat grayMat, Mat outputImage)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(grayMat, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            var circles = new List<DetectedCircleBase>();

            foreach (var contour in contours.ToArrayOfArray())
            {
                using (VectorOfPoint contourPoints = new VectorOfPoint(contour))
                {

                    if (contourPoints.Size >= 10)
                    {
                        RotatedRect ellipse = CvInvoke.FitEllipse(contourPoints);
                        var circle = new CircleF(ellipse.Center, (float)(ellipse.Size.Width / 2));
                        if (circle.Radius > MIN_CIRCLE_RADIUS && circle.Radius < MAX_CIRCLE_RADIUS)
                        {

                            circles.Add(EmguCVCircle.FromCircleF(circle));
                            //debug
                            CvInvoke.Circle(outputImage,
                                new Point((int)circle.Center.X, (int)circle.Center.Y),
                                (int)circle.Radius,
                                new MCvScalar(255, 255, 0),
                                2);
                        }
                    }
                }
            }
            return circles;
        }

        private Dictionary<int, List<DetectedCircleBase>> GroupAndSortCirclesByYPosition(List<DetectedCircleBase> circles)
        {
            var circleGroups = new Dictionary<int, List<DetectedCircleBase>>();
            foreach (EmguCVCircle circle in circles)
            {
                int y = (int)circle.Y;
                int? closestGroupKey = FindClosestGroupKey(y, MAX_VERTICAL_GROUP_DISTANCE, circleGroups.Keys.ToList());

                if (closestGroupKey.HasValue)
                {
                    circleGroups[closestGroupKey.Value].Add(circle);
                }
                else
                {
                    circleGroups[y] = new List<DetectedCircleBase> { circle };
                }
            }
            return circleGroups.OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Value);
        }

        private int? FindClosestGroupKey(int currentY, int maxDistance, List<int> yPositions)
        {
            int? bestYPos = null;

            foreach (int yPos in yPositions)
            {
                if (Math.Abs(yPos - currentY) <= maxDistance)
                {
                    bestYPos = yPos;
                    break;
                }
            }

            return bestYPos;
        }

        private Dictionary<int, List<DetectedCircleBase>> FilterInvalidGroups(Dictionary<int, List<DetectedCircleBase>> sortedGroups, Mat outputImage)
        {
            var validGroups = new Dictionary<int, List<DetectedCircleBase>>();
            foreach (var group in sortedGroups)
            {
                var circles = group.Value;

                var sortedCircles = SortCirclesByXPosition(circles);
                var nonNestedCircles = GetOnlyNonNestedCircles(sortedCircles);
                var uniformCircles = GetCirclesWithUniformRadius(nonNestedCircles);

                if (isCircleGroupValid(uniformCircles, outputImage))
                    validGroups.Add(group.Key, uniformCircles);
            }

            return validGroups;
        }

        private List<DetectedCircleBase> SortCirclesByXPosition(List<DetectedCircleBase> circles)
        {
            return circles.OrderBy(c => c.X).ToList();
        }

        private List<DetectedCircleBase> GetOnlyNonNestedCircles(List<DetectedCircleBase> circles)
        {
            return circles
                .Where(circle => !circles.Any(other => !circle.Equals(other) && IsInside(circle, other) && circle.Radius < other.Radius))
                .ToList();
        }

        private List<DetectedCircleBase> GetCirclesWithUniformRadius(List<DetectedCircleBase> circles)
        {
            var averageRadius = circles.Average(circle => circle.Radius);
            return circles.Where(circle => circle.Radius >= (averageRadius * 0.65) && circle.Radius <= (averageRadius * 1.2)).ToList();
        }

        private bool isCircleGroupValid(List<DetectedCircleBase> circles, Mat outputImage)
        {
            if (circles.Count < 4)
                return false;

            var deltas = circles
                .Zip(circles.Skip(1), (prev, curr) => curr.X - prev.X)
                .ToList();

            var averageDeltaX = deltas.Average();
            var tolerance = averageDeltaX * 0.05;

            bool isValid = !deltas.Any(delta => Math.Abs(delta - averageDeltaX) > tolerance);
            if (!isValid)
            {
                foreach (var circle in circles)
                {
                    CvInvoke.Circle(outputImage,
                    new Point((int)circle.X, (int)circle.Y),
                    (int)circle.Radius,
                    new MCvScalar(0, 255, 255),
                    2);
                }

            }
            return isValid;
        }

        private bool IsInside(DetectedCircleBase inner, DetectedCircleBase outer)
        {
            var dx = inner.X - outer.X;
            var dy = inner.Y - outer.Y;
            var distanceSquared = dx * dx + dy * dy;

            var largerRadius = Math.Max(inner.Radius, outer.Radius);

            return distanceSquared < (largerRadius * largerRadius);
        }

        private double GetFillPercentage(Mat thresholdedImage, EmguCVCircle circle)
        {
            using (Mat mask = new Mat(thresholdedImage.Size, DepthType.Cv8U, 1))
            {
                mask.SetTo(new MCvScalar(0));
                CvInvoke.Circle(mask,
                    new Point((int)circle.X, (int)circle.Y),
                    (int)circle.Radius,
                    new MCvScalar(255),
                    -1);

                MCvScalar mean = CvInvoke.Mean(thresholdedImage, mask);
                return (mean.V0 / 255.0) * 100;
            }
        }
    }
}

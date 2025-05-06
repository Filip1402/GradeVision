using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using GradeVisionLib.Interfaces;
using System;
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

        private const int MIN_CIRCLE_RADIUS = 10;
        private const int MAX_CIRCLE_RADIUS = 50;
        private const int MAX_VERTICAL_GROUP_DISTANCE = 15;//px
        private const double POSITION_DUPLICATE_DIFFERENCE_THRESHOLD = 8f;
        private const double RADIUS_DUPLICATE_DIFFERENCE_THRESHOLD = 5f;
        public (ImageData, Dictionary<int, List<DetectedCircleBase>>) CircleDetection(ImageData input)
        {
            var inputMat = (input as EmguCvImage).ToMat();
            var outputMat = new EmguCvImage().ToMat();
            //for debug
            CvInvoke.CvtColor(inputMat, outputMat, ColorConversion.Gray2Bgr);
            var allCircles = DetectAllCircles(inputMat, outputMat);

            var sortedCircleGroups = GroupAndSortCirclesByYPosition(allCircles);
            var random = new Random();

            foreach (var group in sortedCircleGroups)
            {
                // Generate a unique random color for this group (unmarked circles)
                var groupColor = new MCvScalar(
                random.Next(50, 256), // Avoid very dark colors
                    random.Next(50, 256),
                    random.Next(50, 256)
                );

                foreach (EmguCVCircle circle in group.Value)
                {

                    CvInvoke.Circle(outputMat,
                        new Point((int)circle.X, (int)circle.Y),
                        (int)circle.Radius,
                        groupColor,
                        1);
                }

            }

            var filteredGroups = FilterInvalidGroups(sortedCircleGroups, outputMat);

            var fillPercentages = filteredGroups.SelectMany(group => group.Value)
                                                .Select(c => GetFillPercentage(inputMat, (EmguCVCircle)c))
                                                .ToList();

            double threshold = CalculateFillPercentageTreshold(fillPercentages);

            foreach (var group in filteredGroups)
            {
                // Generate a unique random color for this group (unmarked circles)
                var groupColor = new MCvScalar(
                random.Next(50, 256), // Avoid very dark colors
                    random.Next(50, 256),
                    random.Next(50, 256)
                );

                foreach (EmguCVCircle circle in group.Value)
                {
                    double fillPercent = Math.Floor(GetFillPercentage(inputMat, circle));

                    bool isMarked = fillPercent > threshold;
                    if (isMarked)
                    {
                        circle.SetToMarked();
                    }

                    // Use marked color or group-specific color
                    var color = isMarked ? MARKED_CIRCLE_COLOR : groupColor;

                    CvInvoke.Circle(outputMat,
                        new Point((int)circle.X, (int)circle.Y),
                        (int)circle.Radius,
                        color,
                        1);
                }

                FillPercentageHistogram.GenerateHistogramAndSaveImage(
                fillPercentages,
                threshold,
                input.Name);
            }
            return (EmguCvImage.FromMat(outputMat, input.Name), filteredGroups);
        }

        private double CalculateFillPercentageTreshold(List<double> fillPercentages)
        {
            var histogram = new List<int>(new int[101]);
            fillPercentages.ForEach(fill => histogram[(int)Math.Floor(fill)]++);

            var startOfFirstPeak = histogram.FindIndex(x => x > 0);
            var endOfFirstPeak = histogram.Skip(startOfFirstPeak+1).ToList().FindIndex(x => x == 0) + startOfFirstPeak+1;
            
            while (endOfFirstPeak + 2 < histogram.Count &&
                   (histogram[endOfFirstPeak + 1] > 0 || histogram[endOfFirstPeak + 2] > 0))
            {
                endOfFirstPeak++;
            }

            return endOfFirstPeak;
        }

        private List<DetectedCircleBase> DetectAllCircles(Mat grayMat, Mat outputImage)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(grayMat, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            var rawCircles = new List<DetectedCircleBase>();

            foreach (var contour in contours.ToArrayOfArray())
            {
                using (VectorOfPoint contourPoints = new VectorOfPoint(contour))
                {
                    if (contourPoints.Size >= 20)
                    {
                        RotatedRect ellipse = CvInvoke.FitEllipse(contourPoints);
                        var circle = new CircleF(ellipse.Center, (float)(ellipse.Size.Width / 2));
                        if (circle.Radius > MIN_CIRCLE_RADIUS && circle.Radius < MAX_CIRCLE_RADIUS)
                        {
                            rawCircles.Add(EmguCVCircle.FromCircleF(circle));
                        }
                    }
                }
            }
            var nonDuplicateCircles = FilterDuplicateCircles(rawCircles);

            return GetOnlyNonNestedCircles(nonDuplicateCircles, outputImage);
        }

        private static List<DetectedCircleBase> FilterDuplicateCircles(List<DetectedCircleBase> rawCircles)
        {
            var filteredCircles = new List<DetectedCircleBase>();


            foreach (var circle in rawCircles)
            {
                bool isDuplicate = filteredCircles.Any(existing =>
                {
                    float dx = circle.X - existing.X;
                    float dy = circle.Y - existing.Y;
                    float dr = circle.Radius - existing.Radius;
                    return (dx * dx + dy * dy) <= (POSITION_DUPLICATE_DIFFERENCE_THRESHOLD * POSITION_DUPLICATE_DIFFERENCE_THRESHOLD)
                           && Math.Abs(dr) <= RADIUS_DUPLICATE_DIFFERENCE_THRESHOLD;
                });

                if (!isDuplicate)
                {
                    filteredCircles.Add(circle);
                }
            }

            return filteredCircles;
        }

        private Dictionary<int, List<DetectedCircleBase>> GroupAndSortCirclesByYPosition(List<DetectedCircleBase> circles)
        {
            var circleGroups = new Dictionary<int, List<DetectedCircleBase>>();
            foreach (EmguCVCircle circle in circles)
            {
                int y = (int)circle.Y;
                int? closestGroupKey = FindClosestGroupKey(y, circleGroups.Keys.ToList());

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

        private int? FindClosestGroupKey(int currentY, List<int> yPositions)
        {
            int? bestYPos = null;
            int smallestDifference = int.MaxValue;

            foreach (int yPos in yPositions)
            {
                int diff = Math.Abs(yPos - currentY);
                if (diff <= MAX_VERTICAL_GROUP_DISTANCE && diff < smallestDifference)
                {
                    smallestDifference = diff;
                    bestYPos = yPos;
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
                var nonNestedCircles = GetOnlyNonNestedCircles(sortedCircles, outputImage, sortedCircles.Average(it => it.Y));
                var uniformCircles = GetCirclesWithUniformRadius(nonNestedCircles);

                if (isCircleGroupValid(uniformCircles, outputImage))
                    validGroups.Add(group.Key, uniformCircles);
            }
            return validGroups;
        }

        private List<DetectedCircleBase> SortCirclesByXPosition(List<DetectedCircleBase> circles) { return circles.OrderBy(c => c.X).ToList(); }

        private List<DetectedCircleBase> GetOnlyNonNestedCircles(List<DetectedCircleBase> circles, Mat outputImage, double averageYPosition = -1)
        {
            var result = circles
                .Where(circle => !circles.Any(other =>
                    !circle.Equals(other) &&
                    (circle.GetRelationTo(other) == CircleRelation.ThisInsideOther && circle.Radius < other.Radius ||
                    IsCircleOverlaping(averageYPosition, circle, other, circle.GetRelationTo(other)))
                ))
                .ToList();
            //debug logic
            var nestedCircles = circles
                .Where(circle => !result.Contains(circle))
                .ToList();

            nestedCircles.ForEach(circle =>
            {
                CvInvoke.Circle(outputImage,
                                new Point((int)circle.X, (int)circle.Y),
                                (int)circle.Radius,
                                new MCvScalar(255, 0, 0),
                                1);
            });

            return result;
        }

        private static bool IsCircleOverlaping(double averageYPosition, DetectedCircleBase circle, DetectedCircleBase other, CircleRelation relation)
        {
            return averageYPosition != -1 && relation == CircleRelation.Intersecting && (Math.Abs(circle.Y - averageYPosition)) > (Math.Abs(other.Y - averageYPosition));
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
            var tolerance = averageDeltaX * 0.10;

            bool isValid = !deltas.Any(delta => Math.Abs(delta - averageDeltaX) > tolerance);
            //debug logic
            if (!isValid)
            {
                foreach (var circle in circles)
                {
                    CvInvoke.Circle(outputImage,
                    new Point((int)circle.X, (int)circle.Y),
                    (int)circle.Radius,
                    new MCvScalar(0, 255, 255),
                    1);
                }

            }
            return isValid;
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

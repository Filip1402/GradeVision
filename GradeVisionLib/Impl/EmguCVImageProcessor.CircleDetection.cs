﻿using Emgu.CV;
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
    public partial class EmguCVImageProcessor : ImageProcessorBase
    {
        private static readonly MCvScalar RED_EMGU_CV_COLOR = new MCvScalar(0, 0, 255);
        private static readonly MCvScalar GREEN_EMGU_CV_COLOR = new MCvScalar(0, 255, 0);
        private static readonly MCvScalar BLUE_EMGU_CV_COLOR = new MCvScalar(255, 0, 0);
        private static readonly MCvScalar YELLOW_EMGU_CV_COLOR = new MCvScalar(0, 255, 255);

        private static readonly int MIN_CIRCLE_RADIUS = 10;
        private static readonly int MAX_CIRCLE_RADIUS = 50;
        private static readonly int MAX_VERTICAL_GROUP_DISTANCE = 15;
        private static readonly double POSITION_DUPLICATE_DIFFERENCE_THRESHOLD = 8f;
        private static readonly double RADIUS_DUPLICATE_DIFFERENCE_THRESHOLD = 5f;

        override public (ImageData, Dictionary<int, List<DetectedCircleBase>>) CircleDetection(ImageData input)
        {
            var inputMat = getMat(input);
            var outputMat = new EmguCvImage().ToMat();
            //for debug
            CvInvoke.CvtColor(inputMat, outputMat, ColorConversion.Gray2Bgr);
            var allCircles = DetectAllCircles(inputMat, outputMat);

            var sortedCircleGroups = GroupAndSortCirclesByYPosition(allCircles);
            var random = new Random();

            DrawGroupedCircles(sortedCircleGroups, outputMat, random);

            var filteredGroups = FilterInvalidGroups(sortedCircleGroups, outputMat);

            var fillPercentages = filteredGroups.SelectMany(group => group.Value)
                                                .Select(c => GetFillPercentage(inputMat, (EmguCVCircle)c))
                                                .ToList();

            var threshold = HistogramGenerator.CalculateFillPercentageTreshold(fillPercentages);
            GenerateAndSaveFillPercentageHistogramIfNeeded(input, fillPercentages, threshold);

            DrawFilteredAndMarkGroups(filteredGroups, random, inputMat, threshold, outputMat);
            return (EmguCvImage.FromMat(outputMat, input.Name), filteredGroups);
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

        private void DrawGroupedCircles(Dictionary<int, List<DetectedCircleBase>> groups, Mat outputMat, Random random)
        {
            foreach (var group in groups)
            {
                var groupColor = new MCvScalar(
                    random.Next(50, 256),
                    random.Next(50, 256),
                    random.Next(50, 256)
                );

                foreach (EmguCVCircle circle in group.Value)
                {
                    DrawCircle(outputMat, circle, groupColor);
                }
            }
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

            if (isDebugModeEnabled)
            {
                var nestedCircles = circles
                    .Where(circle => !result.Contains(circle))
                    .ToList();
                nestedCircles.ForEach(circle => { DrawCircle(outputImage, circle, BLUE_EMGU_CV_COLOR); });
            }

            return result;
        }

        private static bool IsCircleOverlaping(double averageYPosition, DetectedCircleBase circle, DetectedCircleBase other, CircleRelation relation)
        {
            return averageYPosition != -1 && relation == CircleRelation.Intersecting && (Math.Abs(circle.Y - averageYPosition)) > (Math.Abs(other.Y - averageYPosition));
        }

        private List<DetectedCircleBase> GetCirclesWithUniformRadius(List<DetectedCircleBase> circles)
        {
            var averageRadius = circles.Average(circle => circle.Radius);
            return circles.Where(circle => circle.Radius >= (averageRadius * 0.5) && circle.Radius <= (averageRadius * 1.3)).ToList();
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

            if (!isValid && isDebugModeEnabled)
            {
                foreach (var circle in circles)
                {
                    DrawCircle(outputImage, circle, YELLOW_EMGU_CV_COLOR);
                }

            }
            return isValid;
        }

        private double GetFillPercentage(Mat thresholdedImage, EmguCVCircle circle)
        {
            using (Mat mask = new Mat(thresholdedImage.Size, DepthType.Cv8U, 1))
            {
                mask.SetTo(new MCvScalar(0));
                DrawCircle(mask, circle, new MCvScalar(255), -1);
                MCvScalar mean = CvInvoke.Mean(thresholdedImage, mask);
                return (mean.V0 / 255.0) * 100;
            }
        }
        private void GenerateAndSaveFillPercentageHistogramIfNeeded(ImageData input, List<double> fillPercentages, double threshold)
        {
            if (isDebugModeEnabled)
            {
                HistogramGenerator.GenerateHistogramAndSaveImage(
                    fillPercentages,
                    threshold,
                    input.Name);
            }
        }
        private void DrawFilteredAndMarkGroups(Dictionary<int, List<DetectedCircleBase>> filteredGroups, Random random, Mat inputMat, double threshold,
            Mat outputMat)
        {
            foreach (var group in filteredGroups)
            {
                var groupColor = new MCvScalar(
                    random.Next(50, 256),
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

                    var color = isMarked ? GREEN_EMGU_CV_COLOR : groupColor;

                    DrawCircle(outputMat, circle, color);
                }

            }
        }
    }
}

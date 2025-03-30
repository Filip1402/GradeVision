using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using GradeVisionLib.Interfaces;
using System;
using System.Drawing;
using System.Linq;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
        private const double MinContourArea = 1000;  // Minimum contour area for filtering

        public Mat CorrectPerspective(Mat image)
        {
            Mat edges = DetectEdges(image);

            VectorOfPoint largestRectangle = FindLargestRectangleContour(edges);

            if (largestRectangle == null)
                return image;

            return ApplyPerspectiveCorrection(image, largestRectangle);
        }

        private Mat DetectEdges(Mat image)
        {
            Mat edges = new Mat();
            CvInvoke.Canny(image, edges, 100, 200);
            return edges;
        }

        private VectorOfPoint FindLargestRectangleContour(Mat edges)
        {
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(edges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                double maxArea = 0;
                VectorOfPoint largestRectangle = null;

                for (int i = 0; i < contours.Size; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    {
                        double area = CvInvoke.ContourArea(contour);
                        if (area < MinContourArea) continue; // Ignore small contours

                        using (VectorOfPoint approx = new VectorOfPoint())
                        {
                            CvInvoke.ApproxPolyDP(contour, approx, 0.02 * CvInvoke.ArcLength(contour, true), true);

                            if (approx.Size == 4 && area > maxArea) // Ensure it's a quadrilateral
                            {
                                maxArea = area;
                                largestRectangle = new VectorOfPoint(approx.ToArray()); // Clone the data
                            }
                        }
                    }
                }

                return largestRectangle;
            }
        }

        private Mat ApplyPerspectiveCorrection(Mat image, VectorOfPoint largestRectangle)
        {
            PointF[] srcPoints = OrderCorners(largestRectangle.ToArray());

            float detectedWidth = (float)Distance(srcPoints[0], srcPoints[1]);
            float detectedHeight = (float)Distance(srcPoints[1], srcPoints[2]);

            const float A4_WIDTH = 2480f;
            const float A4_HEIGHT = 3508f;

            float scaleFactor = Math.Min(detectedWidth / A4_WIDTH, detectedHeight / A4_HEIGHT);

            float maxWidth = A4_WIDTH * scaleFactor;
            float maxHeight = A4_HEIGHT * scaleFactor;

            PointF[] dstPoints = GetDestinationPoints(maxWidth, maxHeight);

            Mat transformMatrix = CvInvoke.GetPerspectiveTransform(srcPoints, dstPoints);

            Mat output = new Mat();
            CvInvoke.WarpPerspective(image, output, transformMatrix, new Size((int)maxWidth, (int)maxHeight));

            return output;
        }

        private PointF[] OrderCorners(Point[] points)
        {
            if (points.Length != 4)
                throw new ArgumentException("OrderCorners requires exactly 4 points.");

            PointF[] orderedPoints = points.Select(p => new PointF(p.X, p.Y)).ToArray();

            orderedPoints = orderedPoints.OrderBy(p => p.Y).ToArray();

            PointF[] topTwo = orderedPoints.Take(2).OrderBy(p => p.X).ToArray(); // Left to right
            PointF[] bottomTwo = orderedPoints.Skip(2).OrderBy(p => p.X).ToArray(); // Left to right

            float detectedWidth = (float)Distance(topTwo[0], topTwo[1]);
            float detectedHeight = (float)Distance(topTwo[0], bottomTwo[0]);

            if (detectedHeight >= detectedWidth)
            {
                return new PointF[]
                {
                topTwo[0],
                topTwo[1],
                bottomTwo[0],
                bottomTwo[1]
                };
            }
            else
            {
                return new PointF[]
                {
            bottomTwo[0], // New top-left (was bottom-left)
            topTwo[0],    // New top-right (was top-left)
            bottomTwo[1], // New bottom-left (was bottom-right)
            topTwo[1]     // New bottom-right (was top-right)
                };
            }

        }

        private PointF[] GetDestinationPoints(float maxWidth, float maxHeight)
        {
            return new PointF[]
            {
                new PointF(0, 0),                      // Top-left
                new PointF(maxWidth - 1, 0),           // Top-right
                new PointF(0, maxHeight - 1),          // Bottom-left
                new PointF(maxWidth - 1, maxHeight - 1) // Bottom-right
            };
        }

        private double Distance(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}

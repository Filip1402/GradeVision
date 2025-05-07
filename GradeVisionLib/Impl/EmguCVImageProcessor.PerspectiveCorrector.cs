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
        private const double MinContourArea = 1000;

        public ImageData CorrectPerspective(ImageData inputImage)
        {
            var inputMat = getMat(inputImage);
            Mat cannyEdges = DetectEdgesCanny(inputMat);
            Mat adaptiveEdges = DetectEdgesAdaptive(inputMat);

            VectorOfPoint cannyRect = FindLargestRectangleContour(cannyEdges);
            VectorOfPoint adaptiveRect = FindLargestRectangleContour(adaptiveEdges);

            #region debug
            Mat outputImage = inputMat.Clone();
            if (cannyRect != null)
            {
                CvInvoke.DrawContours(outputImage, new VectorOfVectorOfPoint(new[] { cannyRect }), -1, new MCvScalar(0, 255, 0), 2);
            }

            if (adaptiveRect != null)
            {
                CvInvoke.DrawContours(outputImage, new VectorOfVectorOfPoint(new[] { adaptiveRect }), -1, new MCvScalar(255, 255, 255), 2);
            }
            SaveImage(outputImage, "Perspective_rects.png");
            #endregion

            VectorOfPoint bestRect = ChooseBestRectangle(cannyRect, adaptiveRect);

            if (bestRect == null)
                return EmguCvImage.FromMat(inputMat, inputImage.Name);

            return EmguCvImage.FromMat(ApplyPerspectiveCorrection(inputMat, bestRect), inputImage.Name);
        }

        private Mat DetectEdgesCanny(Mat image)
        {
            // Step 1: Perform Canny edge detection
            Mat edges = new Mat();
            CvInvoke.Canny(image, edges, 50, 150);

            // Step 2: Apply dilation to fill in gaps in contours
            Mat dilatedEdges = new Mat();
            Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            CvInvoke.Dilate(edges, dilatedEdges, structuringElement, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(1));

            return dilatedEdges;
        }

        private Mat DetectEdgesAdaptive(Mat image)
        {
            Mat thresh = new Mat();
            CvInvoke.AdaptiveThreshold(image, thresh, 255, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 11, 2);

            Mat morph = new Mat();
            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            CvInvoke.MorphologyEx(thresh, morph, MorphOp.Close, kernel, new Point(-1, -1), 2, BorderType.Default, new MCvScalar());

            return morph;
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
                        if (area < MinContourArea) continue;

                        using (VectorOfPoint approx = new VectorOfPoint())
                        {
                            CvInvoke.ApproxPolyDP(contour, approx, 0.02 * CvInvoke.ArcLength(contour, true), true);

                            if (approx.Size == 4 && area > maxArea)
                            {
                                maxArea = area;
                                largestRectangle = new VectorOfPoint(approx.ToArray());
                            }
                        }
                    }
                }

                return largestRectangle;
            }
        }

        private VectorOfPoint ChooseBestRectangle(VectorOfPoint rect1, VectorOfPoint rect2)
        {
            if (rect1 == null) return rect2;
            if (rect2 == null) return rect1;

            double area1 = CvInvoke.ContourArea(rect1);
            double area2 = CvInvoke.ContourArea(rect2);

            double aspect1 = GetAspectRatio(rect1);
            double aspect2 = GetAspectRatio(rect2);

            bool isValid1 = aspect1 is > 0.5 and < 2.0;
            bool isValid2 = aspect2 is > 0.5 and < 2.0;

            if (isValid1 && isValid2)
            {
                double uniformity1 = GetUniformity(rect1);
                double uniformity2 = GetUniformity(rect2);
                return uniformity1 < uniformity2 ? rect1 : rect2;
            }

            if (isValid1) return rect1;
            if (isValid2) return rect2;

            return area1 > area2 ? rect1 : rect2;
        }

        private double GetUniformity(VectorOfPoint rect)
        {
            Rectangle boundingBox = CvInvoke.BoundingRectangle(rect);

            double topWidth = Math.Abs(rect.ToArray()[0].X - rect.ToArray()[1].X);
            double bottomWidth = Math.Abs(rect.ToArray()[2].X - rect.ToArray()[3].X);
            double widthDiff = Math.Abs(topWidth - bottomWidth);

            double leftHeight = Math.Abs(rect.ToArray()[0].Y - rect.ToArray()[3].Y);
            double rightHeight = Math.Abs(rect.ToArray()[1].Y - rect.ToArray()[2].Y);
            double heightDiff = Math.Abs(leftHeight - rightHeight);

            return Math.Min(widthDiff, heightDiff);
        }


        private double GetAspectRatio(VectorOfPoint contour)
        {
            RotatedRect box = CvInvoke.MinAreaRect(contour);
            float w = box.Size.Width;
            float h = box.Size.Height;
            return w > h ? w / h : h / w;
        }

        private Mat ApplyPerspectiveCorrection(Mat image, VectorOfPoint largestRectangle)
        {
            PointF[] srcPoints = OrderCorners(largestRectangle.ToArray());

            float detectedWidth = (float)CalculateDistance(srcPoints[0], srcPoints[1]);
            float detectedHeight = (float)CalculateDistance(srcPoints[1], srcPoints[2]);

            const float A4_WIDTH = 2480f;
            const float A4_HEIGHT = 3508f;

            float scaleFactor = Math.Min(detectedWidth / A4_WIDTH, detectedHeight / A4_HEIGHT);

            float maxWidth = A4_WIDTH * scaleFactor;
            float maxHeight = A4_HEIGHT * scaleFactor;

            const float padding = 25f;
            maxWidth += padding * 2;
            maxHeight += padding * 2;

            PointF[] dstPoints = GetDestinationPointsWithPadding(maxWidth, maxHeight, padding);

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

            PointF[] topTwo = orderedPoints.Take(2).OrderBy(p => p.X).ToArray();
            PointF[] bottomTwo = orderedPoints.Skip(2).OrderBy(p => p.X).ToArray();

            float detectedWidth = (float)CalculateDistance(topTwo[0], topTwo[1]);
            float detectedHeight = (float)CalculateDistance(topTwo[0], bottomTwo[0]);

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
                    bottomTwo[0],
                    topTwo[0],
                    bottomTwo[1],
                    topTwo[1]
                };
            }
        }

        private PointF[] GetDestinationPointsWithPadding(float maxWidth, float maxHeight, float padding)
        {
            return
            [
                new PointF(padding, padding),  // Top-left corner with padding
                new PointF(maxWidth - 1 - padding , padding),  // Top-right corner with padding
                new PointF(padding, maxHeight - 1 - padding ),  // Bottom-left corner with padding
                new PointF(maxWidth - 1 - padding , maxHeight  - 1 - padding )  // Bottom-right corner with padding
            ];
        }

        private double CalculateDistance(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}

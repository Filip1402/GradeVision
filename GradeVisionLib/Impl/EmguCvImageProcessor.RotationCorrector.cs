using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using GradeVisionLib.Interfaces;
using System;
using System.Drawing;

namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : ImageProcessorBase
    {
        override public ImageData CorrectRotation(ImageData inputImage)
        {
            var inputMat = getMat(inputImage);
            var thresholded = ApplyThresholding(inputImage);
            RotatedRect nameRect = DetectNameRectangle(thresholded, inputMat);

            bool isUpsideDown = nameRect.Center.Y > inputMat.Height / 2;

            if (isUpsideDown)
            {
                inputMat = RotateImage(inputMat, 180);
            }

            return EmguCvImage.FromMat(inputMat, inputImage.Name);
        }

        private Mat RotateImage(Mat image, double angle)
        {
            PointF center = new PointF(image.Width / 2, image.Height / 2);
            Mat rotationMatrix = new Mat();
            CvInvoke.GetRotationMatrix2D(center, angle, 1.0, rotationMatrix);

            Mat rotatedImage = new Mat();
            CvInvoke.WarpAffine(image, rotatedImage, rotationMatrix, image.Size, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(255, 255, 255));

            return rotatedImage;
        }

        private RotatedRect DetectNameRectangle(ImageData thresholdedImageData, Mat visualization)
        {
            var thresholded = (thresholdedImageData as EmguCvImage).ToMat();
            var contours = new VectorOfVectorOfPoint();
            var hierarchy = new Mat();
            CvInvoke.FindContours(thresholded, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            RotatedRect bestRect = new RotatedRect();
            double maxArea = 0;

            for (int i = 0; i < contours.Size; i++)
            {
                using (VectorOfPoint contour = contours[i])
                using (VectorOfPoint hull = new VectorOfPoint())
                {
                    CvInvoke.ConvexHull(contour, hull, false); // Smooth out contour shape

                    using (VectorOfPoint approx = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(hull, approx, 0.02 * CvInvoke.ArcLength(hull, true), true); // Reduce jagged edges

                        RotatedRect rect = CvInvoke.MinAreaRect(approx);
                        double area = rect.Size.Width * rect.Size.Height;

                        if (area < 5000) continue;

                        double aspectRatio = Math.Max(rect.Size.Width, rect.Size.Height) / Math.Min(rect.Size.Width, rect.Size.Height);
                        if (aspectRatio > 7 && aspectRatio < 9 && area > maxArea)
                        {
                            maxArea = area;
                            bestRect = rect;
                        }
                    }
                }
            }
            VisualizeDetectedRectanglesForPerspectiveCorrectionIfNeeded(thresholdedImageData, visualization, bestRect);
            return bestRect;
        }

        private void VisualizeDetectedRectanglesForPerspectiveCorrectionIfNeeded(ImageData thresholdedImageData, Mat visualization, RotatedRect bestRect)
        {
            if (!isDebugModeEnabled)
                return;

            if (bestRect.Size.Width > 0 && bestRect.Size.Height > 0)
            {
                PointF[] rectPoints = bestRect.GetVertices();
                for (int i = 0; i < 4; i++)
                {
                    CvInvoke.Line(visualization, Point.Round(rectPoints[i]), Point.Round(rectPoints[(i + 1) % 4]), new MCvScalar(255, 255, 0), 2);
                }
                SaveImage(visualization, thresholdedImageData.Name, "DetectedRectanglesForPerspectiveCorrection.png");
            }
        }
    }
}

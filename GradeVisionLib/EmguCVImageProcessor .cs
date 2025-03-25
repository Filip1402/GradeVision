using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace GradeVisionLib
{
    public class EmguCVImageProcessor : IImageProcessor
    {
        public Mat LoadImage(string imagePath)
        {
            return CvInvoke.Imread(imagePath);
        }

        public Mat CorrectRotation(Mat image)
        {
            Mat edges = new Mat();
            CvInvoke.Canny(image, edges, 50, 150);

            LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 100, 100, 5);
            if (lines.Length == 0) return image;

            List<double> validAngles = new List<double>();

            foreach (var line in lines)
            {
                double angle = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * (180.0 / Math.PI);
                validAngles.Add(angle);
            }

            if (validAngles.Count == 0) return image;
            double mostCommonAngle = validAngles.GroupBy(a => Math.Round(a, 1)).OrderByDescending(g => g.Count()).First().Key;

            // Ensure vertical orientation (close to 0° or 90°)
            if (mostCommonAngle > 45) mostCommonAngle -= 90;
            if (mostCommonAngle < -45) mostCommonAngle += 90;

            Mat rotationMatrix = new Mat();
            CvInvoke.GetRotationMatrix2D(new PointF(image.Width / 2, image.Height / 2), mostCommonAngle, 1.0, rotationMatrix);
            Mat rotatedImage = new Mat();
            CvInvoke.WarpAffine(image, rotatedImage, rotationMatrix, image.Size, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(255, 255, 255));



            Mat treshold = ApplyThresholding(rotatedImage);
            // Find contours
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(treshold, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            Rectangle nameRect = new Rectangle();
            Mat visualization = rotatedImage.Clone();
            foreach (var contour in contours.ToArrayOfArray())
            {

                Rectangle rect = CvInvoke.BoundingRectangle(contour);
                int area = rect.Width * rect.Height;
                if (area < 25000)
                {
                    continue;
                }
                double aspectRatio =  (Double)rect.Width / (Double)rect.Height;
                if ((aspectRatio) > 7 && (aspectRatio) < 9)
                {
                    nameRect = rect;
                }
                CvInvoke.Rectangle(visualization, rect, new MCvScalar(0, 255, 0), 2);
            }

            CvInvoke.Rectangle(visualization, nameRect, new MCvScalar(255, 255, 0), 5);
            SaveImage(visualization, "LargestRectangle.png");

            Mat rotationMatrix2 = new Mat();
            Mat rotatedImage2 = new Mat();

            bool isUpsideDown = nameRect.Top > image.Height / 2;
            var rotAngle = 0;
            if (isUpsideDown)
            {
                rotAngle = 180;              
            }
            CvInvoke.GetRotationMatrix2D(new PointF(rotatedImage.Width / 2, rotatedImage.Height / 2), rotAngle, 1.0, rotationMatrix2);
            CvInvoke.WarpAffine(rotatedImage, rotatedImage2, rotationMatrix2, rotatedImage.Size, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(255, 255, 255));

            return rotatedImage2;
        }

        public Mat ConvertToGrayscale(Mat inputMat)
        {
            Mat grayMat = new Mat();
            CvInvoke.CvtColor(inputMat, grayMat, ColorConversion.Bgr2Gray); // Force grayscale conversion
            return grayMat;
        }

        public Mat ApplyBlur(Mat image)
        {
            // Estimate noise level (using standard deviation of pixel intensities)
            double noiseLevel = EstimateNoiseLevel(image);

            // Apply optimal blur or sharpen based on noise level
            if (noiseLevel > 50)
            {
                image = ApplyGaussianBlur(image, "Blurred");
            }
            else if (noiseLevel < 10)
            {
                AddOperationText(image, "Needs sharpening");
            }
            else
            {
                image = ApplyBilateralFilter(image, "Bilateral");
            }

            return image;
        }

        public Mat ApplyThresholding(Mat inputMat)
        {
            Mat threshMat = new Mat();
            CvInvoke.AdaptiveThreshold(inputMat, threshMat, 255, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 5, 5);
            return threshMat;
        }

        public Mat ApplyContours(Mat inputMat)
        {
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                Mat hierarchy = new Mat();
                CvInvoke.FindContours(inputMat, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                // Check if contours are found
                if (contours.Size == 0)
                {
                    Console.WriteLine("No contours detected.");
                    return inputMat; // Return original image if no contours are found
                }

                // Create a white background for output image
                Mat outputMat = new Mat(inputMat.Size, DepthType.Cv8U, 3); // Create a white image
                outputMat.SetTo(new MCvScalar(255, 255, 255)); // Set all pixels to white

                // Draw contours on the white background
                for (int i = 0; i < contours.Size; i++)
                {
                    // Draw each contour with a specific color (e.g., green) and thickness
                    CvInvoke.DrawContours(outputMat, contours, i, new MCvScalar(0, 255, 0), 2); // Green color for contours
                }

                return outputMat;

            }
        }

        public Rectangle DetectAnswerRegion(Mat binaryImage, Mat baseImage)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(binaryImage, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            var largestAnswerRegion = Enumerable.Range(0, contours.Size)
                 .Select(i => CvInvoke.BoundingRectangle(contours[i]))
                 .Where(boundingBox => IsLikelyAnswerRegion(boundingBox))
                 .OrderByDescending(boundingBox => boundingBox.Width * boundingBox.Height)
                 .FirstOrDefault();

            return largestAnswerRegion;
        }

        private bool IsLikelyAnswerRegion(Rectangle boundingBox)
        {
            float aspectRatio = (float)boundingBox.Width / boundingBox.Height;
            bool isSquareLike = aspectRatio >= 1f && aspectRatio <= 1.3f;
            return isSquareLike;
        }

        public Mat ApplyCannyEdgeDetection(Mat image)
        {
            Mat edges = new Mat();
            CvInvoke.Canny(image, edges, 50, 150);
            return edges;
        }

        public string DetectXMarks(Mat edges, Mat baseImage)
        {
            List<LineSegment2D> lines = DetectLines(edges);
            List<Rectangle> rectangles = FindRectangles(baseImage); // Method to detect answer rectangles
            List<Rectangle> xMarks = FindXMarksInsideRectangles(lines, rectangles); // Check X Marks inside the rectangles

            Mat convertedImage = new Mat();
            CvInvoke.CvtColor(baseImage, convertedImage, ColorConversion.Gray2Bgr);
            Bitmap resultBitmap = convertedImage.ToBitmap();

            using (Graphics g = Graphics.FromImage(resultBitmap))
            {
                // Draw detected lines for debugging
                foreach (var line in lines)
                {
                    using (Pen pen = new Pen(Color.Blue, 2))
                    {
                        g.DrawLine(pen, new Point((int)line.P1.X, (int)line.P1.Y), new Point((int)line.P2.X, (int)line.P2.Y));
                    }
                }

                // Draw the rectangles (answer boxes)
                foreach (var rect in rectangles)
                {
                    using (Pen pen = new Pen(Color.Red, 2))
                    {
                        g.DrawRectangle(pen, rect);
                    }
                }
                /*
                // Draw the bounding boxes for the X marks
                foreach (var xMark in xMarks)
                {
                    using (Pen pen = new Pen(Color.Green, 2))
                    {
                        g.DrawRectangle(pen, xMark);
                    }
                }*/
            }

            string detectedXMarksPath = SaveImage(resultBitmap.ToMat(), "Step8_DetectedXMarks_Debug.png");
            return detectedXMarksPath;
        }

        public List<LineSegment2D> DetectLines(Mat edgeImage)
        {
            // Perform Hough Line Transform to detect lines with adjusted parameters
            var lines = CvInvoke.HoughLinesP(edgeImage, 1, Math.PI / 180, 50, 10, 5); // Adjusted thresholds for smaller lines
            return lines.ToList();
        }

        #region Method to find rectangles (e.g., answer areas) in the image
        private List<Rectangle> FindRectangles(Mat baseImage)
        {
            var rectangles = new List<Rectangle>();

            // Use contour detection or other method to detect the rectangles


            using (var contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(baseImage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < contours.Size; i++)
                {
                    var rect = CvInvoke.BoundingRectangle(contours[i]);
                    if (rect.Width > 250 && rect.Width < 400 && rect.Height > 25) // Size filter for answer boxes
                    {
                        rectangles.Add(rect);
                    }
                }
            }
            
            return rectangles;
        }

        private List<Rectangle> FindXMarksInsideRectangles(List<LineSegment2D> lines, List<Rectangle> rectangles)
        {
            var xMarks = new List<Rectangle>();

            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    // Check if both lines are diagonal and if they form an X
                    if (IsDiagonal(lines[i]) && IsDiagonal(lines[j]) && IsXAngle(lines[i], lines[j]))
                    {

                        var intersection = GetIntersection(lines[i], lines[j]);

                        if (intersection.HasValue)
                        {
                            Point intersectPoint = intersection.Value;

                            // Check if the intersection point is inside any of the detected rectangles
                            foreach (var rect in rectangles)
                            {
                                if (rect.Contains(intersectPoint))
                                {
                                    // If the intersection is inside the rectangle, mark it as an X
                                    Rectangle boundingBox = GetBoundingBox(lines[i], lines[j]);
                                    xMarks.Add(boundingBox);
                                }
                            }
                        }

                    }
                }
            }

            return xMarks;
        }

        private bool IsDiagonal(LineSegment2D line)
        {
            double deltaX = Math.Abs(line.P2.X - line.P1.X);
            double deltaY = Math.Abs(line.P2.Y - line.P1.Y);
            return deltaX > 1 && deltaY > 1 && Math.Abs(deltaX - deltaY) < 10; // Increased tolerance
        }

        private bool IsXAngle(LineSegment2D line1, LineSegment2D line2)
        {
            // Get direction vectors for both lines
            var dir1 = new PointF((float)(line1.P2.X - line1.P1.X), (float)(line1.P2.Y - line1.P1.Y));
            var dir2 = new PointF((float)(line2.P2.X - line2.P1.X), (float)(line2.P2.Y - line2.P1.Y));

            // Calculate the dot product of the direction vectors
            float dotProduct = dir1.X * dir2.X + dir1.Y * dir2.Y;

            // Calculate the magnitudes of the direction vectors
            float magnitude1 = (float)Math.Sqrt(dir1.X * dir1.X + dir1.Y * dir1.Y);
            float magnitude2 = (float)Math.Sqrt(dir2.X * dir2.X + dir2.Y * dir2.Y);

            // Calculate the cosine of the angle between the lines
            float cosineAngle = dotProduct / (magnitude1 * magnitude2);

            // Ensure the cosine value is within the valid range [-1, 1] due to floating-point precision issues
            cosineAngle = Math.Max(-1, Math.Min(1, cosineAngle));

            // Calculate the angle in radians and then convert to degrees
            double angleInRadians = Math.Acos(cosineAngle);
            double angleInDegrees = angleInRadians * (180.0 / Math.PI);

            // Check if the angle is close to 90 degrees (with tolerance)
            return Math.Abs(angleInDegrees - 90) < 15; // Increased tolerance for better matching
        }

        private Rectangle GetBoundingBox(LineSegment2D line1, LineSegment2D line2)
        {
            int minX = Math.Min(Math.Min(line1.P1.X, line1.P2.X), Math.Min(line2.P1.X, line2.P2.X));
            int minY = Math.Min(Math.Min(line1.P1.Y, line1.P2.Y), Math.Min(line2.P1.Y, line2.P2.Y));
            int maxX = Math.Max(Math.Max(line1.P1.X, line1.P2.X), Math.Max(line2.P1.X, line2.P2.X));
            int maxY = Math.Max(Math.Max(line1.P1.Y, line1.P2.Y), Math.Max(line2.P1.Y, line2.P2.Y));

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private Point? GetIntersection(LineSegment2D line1, LineSegment2D line2)
        {
            // Line 1: (x1, y1) -> (x2, y2)
            double x1 = line1.P1.X, y1 = line1.P1.Y, x2 = line1.P2.X, y2 = line1.P2.Y;
            // Line 2: (x3, y3) -> (x4, y4)
            double x3 = line2.P1.X, y3 = line2.P1.Y, x4 = line2.P2.X, y4 = line2.P2.Y;

            // Calculate the denominator of the intersection point formula
            double denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            if (denominator == 0) return null; // No intersection, lines are parallel

            // Calculate the intersection point using the determinant method
            double intersectX = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denominator;
            double intersectY = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denominator;

            return new Point((int)intersectX, (int)intersectY);
        }

        private string SaveImage(Mat image, string fileName)
        {
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProcessedImages");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            string filePath = Path.Combine(outputDir, fileName);
            CvInvoke.Imwrite(filePath, image);
            return filePath;
        }
        #endregion

        #region Blur and Noise Estimation
        private double EstimateNoiseLevel(Mat image)
        {

            MCvScalar mean = new MCvScalar();
            MCvScalar stddev = new MCvScalar();

            CvInvoke.MeanStdDev(image, ref mean, ref stddev);

            return stddev.V0;
        }

        private Mat ApplyGaussianBlur(Mat image, string operationName)
        {

            int kernelSize = 5;
            Mat blurredImage = new Mat();
            CvInvoke.GaussianBlur(image, blurredImage, new Size(kernelSize, kernelSize), 0);

            AddOperationText(blurredImage, operationName);

            return blurredImage;
        }

        private Mat ApplyBilateralFilter(Mat image, string operationName)
        {
            Mat filteredImage = new Mat();
            CvInvoke.BilateralFilter(image, filteredImage, 9, 75, 75, BorderType.Default);

            AddOperationText(filteredImage, operationName);

            return filteredImage;
        }

        private void AddOperationText(Mat image, string operationName)
        {
            string text = operationName;
            var font = new System.Drawing.Font("Arial", 40);
            CvInvoke.PutText(image, text, new Point(10, 30), FontFace.HersheySimplex, 1.0, new MCvScalar(0, 255, 0), 2); // Green text
        }

        #endregion

    }
}

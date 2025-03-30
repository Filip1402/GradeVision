using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using System.Drawing;

using GradeVisionLib.Interfaces;


namespace GradeVisionLib.Impl
{
    public partial class EmguCVImageProcessor : IImageProcessor
    {
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
                        g.DrawLine(pen, new Point(line.P1.X, line.P1.Y), new Point(line.P2.X, line.P2.Y));
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
            var dir1 = new PointF(line1.P2.X - line1.P1.X, line1.P2.Y - line1.P1.Y);
            var dir2 = new PointF(line2.P2.X - line2.P1.X, line2.P2.Y - line2.P1.Y);

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

        #endregion
    }
}

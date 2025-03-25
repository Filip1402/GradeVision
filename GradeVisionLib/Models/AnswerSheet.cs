using System;
using System.Drawing;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace GradeVisionLib.Models
{
    public class AnswerSheet
    {
        public void GeneratePdfAnswerSheet(Test test, string outputPath)
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 20);
            XFont font1 = new XFont("Arial", 24);
            XFont circleFont = new XFont("Arial", 20);

            double pageHeight = page.Height;

            // Draw Name & Grade Fields (Adjusted for Top-Left Origin)
            DrawFrame(gfx, 40, 60, 500, 60, "Name: ___________    Grade: ___________", font);

            // Draw Answer Area Frame
            // Draw Questions & Answers
            double y = 200; // Start from top after the header
            int questionNumber = 1;
            foreach (var question in test.Questions)
            {
                gfx.DrawString(questionNumber.ToString()+".", font, XBrushes.Black, new XPoint(120, y));

                // Draw answer options as larger circles with labels inside
                double answerX = 200;
                double circleRadius = 25; // Increased circle size

                var y_offseted = y - 7;

                foreach (var answer in question.Answers)
                {
                    DrawCircle(gfx, answerX, y_offseted, circleRadius);
                    gfx.DrawString(answer.Label, circleFont, XBrushes.Black, new XPoint(answerX - 7, y_offseted + 5)); // Center label inside circle
                    answerX += 80;
                }
                y += 70; // Increase spacing to accommodate larger circles
                questionNumber++;
            }
            gfx.DrawRectangle(XPens.Black, 100, 150, 400, y-185);

            DrawFrame(gfx, 40, 120, 500, 640, "Answer Area", font);


            document.Save(outputPath);
        }

        private void DrawFrame(XGraphics gfx, double x, double y, double width, double height, string text, XFont font)
        {
            gfx.DrawRectangle(XPens.Black, x, y, width, height);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(x + 10, y + 20));
        }

        private void DrawCircle(XGraphics gfx, double x, double y, double radius)
        {
            gfx.DrawEllipse(XPens.Black, x - radius, y - radius, radius * 2, radius * 2);
        }
    }
}
using System;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace GradeVisionLib.Models
{
    public class AnswerSheet
    {
        // Layout Constants
        private const double PagePadding = 40;
        private const double HeaderHeight = 60;
        private const double AnswerAreaY = PagePadding * 1.2 + HeaderHeight;
        private const double AnswerFrameBottomPadding = 40;
        private const double FramePadding = 10;
        private const double InnerMargin = 20; // Padding inside answer area for inner content

        public void GeneratePdfAnswerSheet(Test test, string outputPath)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double pageWidth = page.Width;
            double pageHeight = page.Height;

            double answerAreaHeight = pageHeight - AnswerAreaY - AnswerFrameBottomPadding;
            double answerAreaWidth = pageWidth - 2 * PagePadding;

            int numQuestions = test.Questions.Count;
            int maxAnswers = GetMaxAnswerCount(test);

            // Space available for answers inside the main frame (minus padding)
            double innerAvailableWidth = answerAreaWidth - 2 * InnerMargin - 80; // 80 for question number + spacing
            double innerAvailableHeight = answerAreaHeight - InnerMargin;

            // Calculate circle size
            double questionSpacing = innerAvailableHeight / numQuestions;
            double maxCircleHeight = questionSpacing / 1.2;
            double maxCircleWidth = innerAvailableWidth / maxAnswers;
            double circleDiameter = Math.Min(maxCircleHeight, maxCircleWidth);
            double circleRadius = Math.Max(10, Math.Min(circleDiameter / 2, 25));
            double fontSize = circleRadius * 1.3;

            // Fonts
            var circleFont = new XFont("Arial", fontSize);
            var frameFont = new XFont("Arial", 18);

            // Draw header frame
            DrawFrame(gfx, PagePadding, PagePadding, answerAreaWidth, HeaderHeight, "Name: ___________    Grade: ___________", frameFont);

            // Draw outer answer area frame
            DrawFrame(gfx, PagePadding, AnswerAreaY, answerAreaWidth, answerAreaHeight, "", frameFont);

            // Start drawing questions
            double innerStartX = PagePadding + InnerMargin;
            double innerStartY = AnswerAreaY + InnerMargin * 1.2;
            double currentY = innerStartY;
            int questionNumber = 1;

            double innerContentLeft = innerStartX;
            double answerSpacing = innerAvailableWidth / maxAnswers;

            // Track bottom of content to draw inner box
            double innerContentBottomY = currentY;

            foreach (var question in test.Questions)
            {
                DrawQuestionRow(gfx, question, questionNumber, currentY, circleRadius, innerContentLeft, answerSpacing, circleFont);
                currentY += questionSpacing;
                questionNumber++;
            }

            innerContentBottomY = currentY - (questionSpacing - circleRadius * 2);

            // Draw inner frame around answer circles
            double innerFrameX = innerContentLeft - circleRadius - 10;
            double innerFrameY = innerStartY - circleRadius - 10;
            double innerFrameWidth = (maxAnswers * answerSpacing) + 2 * circleRadius + 20;
            double innerFrameHeight = innerContentBottomY - innerStartY + 2 * circleRadius + 20;


            document.Save(outputPath);
        }

        private void DrawQuestionRow(XGraphics gfx, Question question, int questionNumber, double y, double circleRadius, double startX, double answerSpacing, XFont font)
        {
            string qLabel = $"{questionNumber}.";
            var qSize = gfx.MeasureString(qLabel, font);
            double qLabelX = startX;
            gfx.DrawString(qLabel, font, XBrushes.Black, new XPoint(qLabelX, y + qSize.Height / 3));

            double answerX = startX + 60;

            foreach (var answer in question.Answers)
            {
                DrawCircle(gfx, answerX, y, circleRadius);
                CenterTextInCircle(gfx, answer.Label, font, answerX, y);
                answerX += answerSpacing;
            }
        }

        private void DrawFrame(XGraphics gfx, double x, double y, double width, double height, string text, XFont font)
        {
            gfx.DrawRectangle(XPens.Black, x, y, width, height);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(x + FramePadding, y + 25));
        }

        private void DrawCircle(XGraphics gfx, double centerX, double centerY, double radius)
        {
            gfx.DrawEllipse(XPens.Black, centerX - radius, centerY - radius, radius * 2, radius * 2);
        }

        private void CenterTextInCircle(XGraphics gfx, string text, XFont font, double centerX, double centerY)
        {
            var size = gfx.MeasureString(text, font);
            double textX = centerX - size.Width / 2;
            double textY = centerY - size.Height / 2 + size.Height * 0.75;
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(textX, textY));
        }

        private int GetMaxAnswerCount(Test test)
        {
            int max = 0;
            foreach (var question in test.Questions)
            {
                if (question.Answers.Count > max)
                    max = question.Answers.Count;
            }
            return max;
        }
    }
}

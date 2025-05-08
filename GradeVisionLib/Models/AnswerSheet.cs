using System;
using Lombok.NET;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace GradeVisionLib.Models
{
    [RequiredArgsConstructor(MemberType = MemberType.Field, AccessTypes = AccessTypes.Private)]
    public partial class AnswerSheet
    {
        private const double PagePadding = 40;
        private const double HeaderHeight = 60;
        private const double InnerMargin = 20;
        private const double HeaderTextPadding = 10;
        private const double HeaderTextOffsetY = 35;
        private const double QuestionNumberOffset = 70;
        private const double MaxCircleRadius = 20;
        private const double CircleFontScale = 1.3;
        private const double QuestionSpacingDivider = 2.5;

        private const string FontName = "Arial";
        private const double HeaderFontSize = 18;

        private readonly int _numOfQuestions;
        private readonly int _numOfAnswersPerQuestion;
        private readonly string _outputPath;

        public void Generate()
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double answerAreaY = PagePadding + HeaderHeight + 10;
            double answerAreaHeight = page.Height - answerAreaY - PagePadding;
            double availableWidth = page.Width - 2 * PagePadding;
            double availableHeight = answerAreaHeight - 2 * InnerMargin;
            double questionSpacing = availableHeight / _numOfQuestions;
            double circleRadius = Math.Min(questionSpacing / QuestionSpacingDivider, MaxCircleRadius);
            double fontSize = circleRadius * CircleFontScale;
            var font = new XFont(FontName, fontSize);

            DrawHeader(gfx, availableWidth);
            DrawAnswerAreaFrame(gfx, answerAreaY, availableWidth, answerAreaHeight);

            double contentStartY = answerAreaY + InnerMargin;
            double rowWidth = availableWidth - 2 * InnerMargin;
            double startX = PagePadding + InnerMargin;

            DrawAllQuestionRows(gfx, contentStartY, questionSpacing, circleRadius, startX, rowWidth, font);

            document.Save(_outputPath);
        }

        private void DrawHeader(XGraphics gfx, double width)
        {
            gfx.DrawRectangle(XPens.Black, PagePadding, PagePadding, width, HeaderHeight);
            gfx.DrawString(
                "Name: ___________    Grade: ___________",
                new XFont(FontName, HeaderFontSize),
                XBrushes.Black,
                new XPoint(PagePadding + HeaderTextPadding, PagePadding + HeaderTextOffsetY)
            );
        }

        private void DrawAnswerAreaFrame(XGraphics gfx, double y, double width, double height)
        {
            gfx.DrawRectangle(XPens.Black, PagePadding, y, width, height);
        }

        private void DrawAllQuestionRows(XGraphics gfx, double contentStartY, double questionSpacing, double radius, double startX, double rowWidth, XFont font)
        {
            for (int i = 0; i < _numOfQuestions; i++)
            {
                string label = $"{i + 1}.";
                double centerY = contentStartY + i * questionSpacing + questionSpacing / 2;

                var labelSize = gfx.MeasureString(label, font);
                gfx.DrawString(label, font, XBrushes.Black, new XPoint(startX, centerY + labelSize.Height / 3));

                double answerStartX = startX + QuestionNumberOffset;
                double spacing = (rowWidth - QuestionNumberOffset) / _numOfAnswersPerQuestion;
                double x = answerStartX;

                for (int j = 0; j < _numOfAnswersPerQuestion; j++)
                {
                    char answerLabel = (char)('A' + j);
                    DrawCircle(gfx, x, centerY, radius);
                    CenterTextInCircle(gfx, answerLabel.ToString(), font, x, centerY);
                    x += spacing;
                }
            }
        }

        private void DrawCircle(XGraphics gfx, double cx, double cy, double r)
        {
            gfx.DrawEllipse(XPens.Black, cx - r, cy - r, r * 2, r * 2);
        }

        private void CenterTextInCircle(XGraphics gfx, string text, XFont font, double cx, double cy)
        {
            var size = gfx.MeasureString(text, font);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(cx - size.Width / 2, cy + size.Height / 3));
        }
    }
}

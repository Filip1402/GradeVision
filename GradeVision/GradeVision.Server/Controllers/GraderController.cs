using Emgu.CV;
using Emgu.CV.Structure;
using GradeVisionLib;
using GradeVisionLib.Impl;
using GradeVisionLib.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GradeVision.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraderController : ControllerBase
    {
        private AnswerSheetAnalyzer answerSheetAnalyzer = new AnswerSheetAnalyzer(new EmguCVImageProcessor());

        [HttpPost("uploadControlTest")]
        public async Task<IActionResult> UploadControlTest(IFormFile controlTestFile)
        {
            try
            {
                // Load uploaded image into memory
                using var stream = controlTestFile.OpenReadStream();

                var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(controlTestFile.FileName));

                using (var fs = System.IO.File.Create(tempFilePath))
                    await stream.CopyToAsync(fs);

                var (processedImage, controlAnswers) = await Task.Run(() =>
                    answerSheetAnalyzer.ProcessControlSheet(tempFilePath)); // returns (IImage, string, double)

                System.IO.File.Delete(tempFilePath);

                if (processedImage is EmguCvImage emguImage)
                {
                    using var bitmap = emguImage.ToMat().ToImage<Bgr, byte>().ToBitmap();
                    using var ms = new MemoryStream();
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    var response = new
                    {
                        Image = ms.ToArray(),
                        controlAnswers = controlAnswers
                    };

                    return Ok(response);
                }

                return StatusCode(500, new { message = "Image conversion failed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing control test.", error = ex.Message });
            }
        }

        public class GradeAnswerSheetRequest
        {
            public string FileBase64 { get; set; }
            public Dictionary<int, List<EmguCVCircle>> ControlAnswers { get; set; }
        }

        [HttpPost("gradeAnswerSheet")]
        public async Task<IActionResult> UploadAnswerSheet([FromBody] GradeAnswerSheetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FileBase64))
                return BadRequest("No file provided.");

            try
            {
                // Remove base64 prefix if present
                var base64Data = Regex.Replace(request.FileBase64, @"^data:image\/[a-zA-Z]+;base64,", "");
                var fileBytes = Convert.FromBase64String(base64Data);

                // Save image to temp file
                var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
                await System.IO.File.WriteAllBytesAsync(tempFilePath, fileBytes);

                // Process the sheet
                var controlAnswersBase = request.ControlAnswers.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Cast<DetectedCircleBase>().ToList()
                );

                var (processedImage, grade, score) = answerSheetAnalyzer.ProcessAnswerSheet(tempFilePath, Guid.NewGuid().ToString(), controlAnswersBase);

                // Clean up
                System.IO.File.Delete(tempFilePath);

                return Ok(new
                {
                    grade,
                    score,
                    processedImage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error grading sheet", error = ex.Message });
            }
        }
    }
}

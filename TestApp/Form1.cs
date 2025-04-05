using GradeVisionLib;
using GradeVisionLib.Impl;
using GradeVisionLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form1 : Form
    {
        private static string controlImage = "bezRTubA.jpg";
        private static string inputFolder = @"C:\Users\zutif\OneDrive - Fakultet Organizacije i Informatike Varaždin\FOI\Diplomski\";

        private List<Result> results = new List<Result>();  // Using Result class to store data
        private List<string> inputPics = Directory.GetFiles(inputFolder, "*.*")
                                          .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                         file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                         file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                                          .Select(Path.GetFileName).ToList();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load_1(object sender, EventArgs e)
        {
            // Initial image display
            pictureBox1.Image = await LoadImageWithoutLockAsync(inputFolder + inputPics[inputPics.Count - 1]);

            // Bind the 'results' list to DataGridView
            dataGridView1.DataSource = results;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var answerSheetAnalyzer = new AnswerSheetAnalyzer(new EmguCVImageProcessor());

            // Process the control sheet
            var (controlIamgePath, a, b) = answerSheetAnalyzer.ProcessControlSheet(inputFolder + "Control" + "\\" + controlImage);
            pictureBox3.Image = await LoadImageWithoutLockAsync(controlIamgePath);  // Show the control image right away

            // Use a loop to process each answer sheet asynchronously
            foreach (var imageName in inputPics)
            {
                // Process the answer sheet
                var (finalImagePath, grade, score) = await Task.Run(() =>
                    answerSheetAnalyzer.ProcessAnswerSheet(inputFolder + imageName, imageName));

                // Update pictureBox1 with the current image being processed
                pictureBox1.Image = await LoadImageWithoutLockAsync(inputFolder + imageName);  // Display relevant image in pictureBox1

                // Update the picture box with the processed image (pictureBox2)
                pictureBox2.Image = await LoadImageWithoutLockAsync(finalImagePath);

                // Outputting the grade and score to the debug console
                System.Diagnostics.Debug.WriteLine($"Grade: {grade}, Score: {score}");

                // Add the result to the list
                results.Add(new Result(imageName, grade, score));

                // Allow UI to update after processing each image
                await Task.Yield();  // Ensure the UI thread has a chance to update
            }

            // After the loop finishes, refresh the DataGridView to reflect the new data
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = results;
        }

        private async Task<Image> LoadImageWithoutLockAsync(string path)
        {
            // Open the file asynchronously
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
            {
                // Read the bytes asynchronously
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, (int)stream.Length);

                // Create the image from the stream asynchronously
                using (var memoryStream = new MemoryStream(buffer))
                {
                    return Image.FromStream(memoryStream);
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            var gradeDefinisons = new List<string> { "1", "2", "3", "4", "5" };
            var gradeThresholds = new List<double> { 50.00, 63.00, 75.00, 85.00 };

            var gradeScale = new GradeScale(gradeDefinisons, gradeThresholds);
            var test = new Test(gradeScale);

            test.Name = "Test 1";
            test.SubjectName = "Math";
            test.TeacherName = "John Doe";

            Answer a = new Answer("A");
            Answer b = new Answer("B");
            Answer c = new Answer("C");
            Answer d = new Answer("D");
            var allAnswers = new List<Answer> { a, b, c, d };
            var correctAnswer1 = new List<Answer> { a, c };
            test.AddQuestion(new MultipleChoiceQuestion(allAnswers, correctAnswer1, 2));
            test.AddQuestion(new MultipleChoiceQuestion(allAnswers, correctAnswer1, 2));
            test.AddQuestion(new MultipleChoiceQuestion(allAnswers, correctAnswer1, 2));
            test.AddQuestion(new MultipleChoiceQuestion(allAnswers, correctAnswer1, 2));
            test.AddQuestion(new MultipleChoiceQuestion(allAnswers, correctAnswer1, 2));
            test.AddQuestion(new MultipleChoiceQuestion(allAnswers, correctAnswer1, 2));
            test.AddQuestion(new MultipleChoiceQuestion(allAnswers, correctAnswer1, 2));
            test.AddQuestion(new MultipleChoiceQuestion(allAnswers, correctAnswer1, 2));

            test.GetAnswerSheet();
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToInt32(row.Cells[2].Value) == -100)
                    row.DefaultCellStyle.BackColor = Color.Red;
            }
        }
    }
}

public class Result
{
    public string Image { get; set; }
    public string Grade { get; set; }
    public double Score { get; set; }

    public Result(string image, string grade, double score)
    {
        Grade = grade;
        Score = score;
        Image = image;
    }
}

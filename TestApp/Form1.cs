using GradeVisionLib;
using GradeVisionLib.Impl;
using GradeVisionLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private void Form1_Load_1(object sender, EventArgs e)
        {
            // Initial image display
            pictureBox1.Image = Image.FromFile(inputFolder + inputPics[inputPics.Count - 1]);

            // Bind the 'results' list to DataGridView
            dataGridView1.DataSource = results;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var imageName in inputPics)
            {
                var answerSheetAnalyzer = new AnswerSheetAnalyzer(new EmguCVImageProcessor(), imageName);
                var (controlIamgePath, a, b) = answerSheetAnalyzer.ProcessControlSheet(inputFolder + controlImage);
                var (finalImagePath, grade, score) = answerSheetAnalyzer.ProcessAnswerSheet(inputFolder + imageName);
                pictureBox2.Image = LoadImageWithoutLock(finalImagePath);
                pictureBox3.Image = LoadImageWithoutLock(controlIamgePath);

                // Outputting the grade and score to the debug console
                System.Diagnostics.Debug.WriteLine($"Grade: {grade}, Score: {score}");

                // Adding the result to the list
                results.Add(new Result(imageName, grade, score));  // Store the result in the list
            }

            // Refresh the DataGridView to reflect the new data
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = results;
        }

        private Image LoadImageWithoutLock(string path)
        {
            using (var stream = new System.IO.MemoryStream(File.ReadAllBytes(path)))
            {
                return Image.FromStream(stream);
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
    public string Grade { get; set; }  // The grade of the test (e.g., "A", "B", etc.)
    public double Score { get; set; }  // The score the student achieved (e.g., 75.0, 80.0, etc.)

    // Constructor to initialize the result
    public Result(string image, string grade, double score)
    {

        Grade = grade;
        Score = score;
        Image = image;
    }
}


using GradeVisionLib;
using GradeVisionLib.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form1 : Form
    {
        private string inputFolder = "C:\\Users\\zutif\\OneDrive - Fakultet Organizacije i Informatike Varaždin\\FOI\\Diplomski\\";
        List<string> inputPics = new List<string>
        { "6eb22210-d82b-4906-b55d-abb907e26b5d.png",
            "2.jpg",
            "3.png",
            "angled_temp.jpg",
            "temp_screen.png",
            "temp_screen1.png",
            "slika.jpg",
            "slika_rot1.jpg",
            "slika_rot2.jpg",
            "slika_rot3.jpg",
            "slika_rot4.jpg"
        };


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            pictureBox1.Image = Image.FromFile(inputFolder + inputPics[inputPics.Count - 1]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var imageName in inputPics)
            {
                var answerSheetAnalyzer = new AnswerSheetAnalyzer(new EmguCVImageProcessor(), imageName);
                string finalImagePath = answerSheetAnalyzer.ProcessAnswerSheet(inputFolder + imageName);
                pictureBox2.Image = LoadImageWithoutLock(finalImagePath);
            }

            //MessageBox.Show("Processing complete. Check processed images folder.");
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
    }
}

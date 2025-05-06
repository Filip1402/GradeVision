using Emgu.CV.Structure;
using Emgu.CV;
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
        private AnswerSheetAnalyzer AnswerSheetAnalyzer = new AnswerSheetAnalyzer(new EmguCVImageProcessor());
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
            pictureBox1.Image = await LoadImageWithoutLockAsync(inputFolder + inputPics[inputPics.Count - 1]);
            dataGridView1.DataSource = results;
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            var (processedControlImage, controlAnswers) = AnswerSheetAnalyzer.ProcessControlSheet(inputFolder + "Control" + "\\" + controlImage);
            pictureBox3.Image = (processedControlImage as EmguCvImage)?.ToMat().ToImage<Bgr, byte>().ToBitmap();

            foreach (var imageName in inputPics)
            {
                var (processedImage, grade, score) = await Task.Run(() =>
                    AnswerSheetAnalyzer.ProcessAnswerSheet(inputFolder + imageName, imageName, controlAnswers));

                pictureBox1.Image = await LoadImageWithoutLockAsync(inputFolder + imageName);  // Display relevant image in pictureBox1

                pictureBox2.Image = (processedImage as EmguCvImage).ToMat().ToImage<Bgr, byte>().ToBitmap();

                System.Diagnostics.Debug.WriteLine($"Grade: {grade}, Score: {score}");

                results.Add(new Result(imageName, grade, score));

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = results;

                await Task.Yield();
            }


        }

        private async Task<Image> LoadImageWithoutLockAsync(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
            {
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, (int)stream.Length);

                using (var memoryStream = new MemoryStream(buffer))
                {
                    return Image.FromStream(memoryStream);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            var test = new Test((int)numericUpDown1.Value, (int)numericUpDown2.Value);
            test.GetAnswerSheet();
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[2].Value != null && int.TryParse(row.Cells[2].Value.ToString(), out int cellValue) && cellValue == -100)
                {
                    row.DefaultCellStyle.BackColor = Color.Red;
                }
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

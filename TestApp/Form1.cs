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
using System.Diagnostics;
using GradeVisionLib.Interfaces;
using System.ComponentModel;

namespace TestApp
{
    public partial class Form1 : Form
    {
        private static string outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProcessedImages");
        private AnswerSheetGrader AnswerSheetAnalyzer = new AnswerSheetGrader(new EmguCVImageProcessor(true), outputFolder);
        private String ControlTestPath { get; set; }
        private List<String> TestsToGradePaths { get; set; }
        private Dictionary<int, List<DetectedCircleBase>> ControlAnswers = new Dictionary<int, List<DetectedCircleBase>>();

        private GradeScale GradeScale = new GradeScale(new List<string> { "1", "2", "3", "4", "5" }, new List<double> { 50.00, 63.00, 75.00, 85.00 });
        private BindingList<GradeDefinition> GradeDefintions = new BindingList<GradeDefinition>();
        private BindingList<GradingResult> results = new BindingList<GradingResult>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            dataGridView1.DataSource = results;
            dataGridView2.DataSource = GradeDefintions;
            dataGridView2.Columns[0].ReadOnly = true;
            dataGridView2.Columns[1].ReadOnly = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var numOfQuestions = (int)numericUpDown1.Value;
            var numOfAnswersPerQuestions = (int)numericUpDown2.Value;
            new AnswerSheetGenerator(numOfQuestions, numOfAnswersPerQuestions, outputFolder).Generate();

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

        private void btnLoadControlTest_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private async void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ControlTestPath = openFileDialog1.FileName;

            await ProcessControlTestImage();
        }

        private async Task ProcessControlTestImage()
        {
            try
            {
                var image = await LoadImageWithoutLockAsync(ControlTestPath);
                picControlTest.Image = image;
                await Task.Yield();
                var imageData = EmguCvImage.FromImage(image, Path.GetFileName(ControlTestPath));
                (var processedControlImage, ControlAnswers) = await Task.Run(() =>
                    AnswerSheetAnalyzer.ProcessControlSheet(imageData));
                picControlTest.Image = (processedControlImage as EmguCvImage)?.ToMat().ToImage<Bgr, byte>().ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load image: {ex.Message}");
            }
        }

        private async void openMultiFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TestsToGradePaths = openMultiFileDialog.FileNames.ToList();
            try
            {
                picTestToBeGraded.Image = await LoadImageWithoutLockAsync(TestsToGradePaths.First());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load image: {ex.Message}");
            }
        }

        private void btnLoadTestsToGrade_Click(object sender, EventArgs e)
        {
            openMultiFileDialog.ShowDialog();
        }

        private async void btnGradeTests_Click(object sender, EventArgs e)
        {

            if (ControlAnswers.Count == 0 || TestsToGradePaths.Count == 0)
            {
                MessageBox.Show(
                    "Control test and tests for grading must be loaded before grading!!!",
                    "Error!!!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            foreach (var imagePath in TestsToGradePaths)
            {
                var image = await LoadImageWithoutLockAsync(imagePath);
                picTestToBeGraded.Image = image;
                var imageData = EmguCvImage.FromImage(image, Path.GetFileName(imagePath));
                try
                {
                    var (processedImage, grade, score) = await Task.Run(() =>
                    AnswerSheetAnalyzer.ProcessAnswerSheet(imageData, ControlAnswers, GradeScale));
                    picxGradedTests.Image = (processedImage as EmguCvImage).ToMat().ToImage<Bgr, byte>().ToBitmap();
                    System.Diagnostics.Debug.WriteLine($"Grade: {grade}, Score: {score}");
                    results.Add(new GradingResult(Path.GetFileName(imagePath), grade, score));
                    await Task.Yield();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void btnDefineGrade_Click(object sender, EventArgs e)
        {
            if (tboxGradeDefintion.Text == null || tboxGradeDefintion.Text.Trim().Length == 0)
                return;

            GradeDefintions.Add(new GradeDefinition(tboxGradeDefintion.Text, null));
            dataGridView2.Rows[0].ReadOnly = true;
            tboxGradeDefintion.Text = "";
        }

        private void btnApplyGradeScale_Click(object sender, EventArgs e)
        {
            try
            {
                var (grades, thresholds) = TryExtractGradeScale(GradeDefintions);
                GradeScale = new GradeScale(grades, thresholds);
                GradeDefintions.Clear();
            }
            catch (Exception ex) { }
        }

        private (List<string>, List<double>) TryExtractGradeScale(
        BindingList<GradeDefinition> gradeDefinitions)
        {
            if (gradeDefinitions == null || gradeDefinitions.Count < 2)
            {
                MessageBox.Show("You need at least two grade definitions.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new ArgumentException("You need at least two grade definitions.");
            }

            var grades = gradeDefinitions.Select(g => g.Grade).ToList();
            var missingThreshold = gradeDefinitions.Skip(1).Any(g => g.Treshold == null);
            if (missingThreshold)
            {
                MessageBox.Show("All grade definitions except the first must have a threshold.", "Missing Threshold", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new InvalidOperationException("All grade definitions except the first must have a threshold.");
            }
            var thresholds = gradeDefinitions.Skip(1).Select(g => g.Treshold.Value).ToList();
            return (grades, thresholds);
        }

        #region Helper functions
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
        #endregion

        private async void chbocDebugEnabled_CheckedChanged(object sender, EventArgs e)
        {
            AnswerSheetAnalyzer = new AnswerSheetGrader(new EmguCVImageProcessor(chbocDebugEnabled.Checked), outputFolder);
            if (ControlTestPath != null)
                await ProcessControlTestImage();
        }
    }
}

public class GradingResult
{
    public string ImageName { get; set; }
    public string Grade { get; set; }
    public double ScoredPercentage { get; set; }

    public GradingResult(string imageName, string grade, double scoredPercentage)
    {
        Grade = grade;
        ScoredPercentage = scoredPercentage;
        ImageName = imageName;
    }
}

public class GradeDefinition
{
    public string Grade { get; set; }
    public double? Treshold { get; set; }
    public GradeDefinition(string grade, int? treshold)
    {
        Grade = grade;
        Treshold = treshold;
    }
}

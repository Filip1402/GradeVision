namespace TestApp
{
    partial class Form1 : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnGradeTests = new Button();
            picControlTest = new PictureBox();
            picxGradedTests = new PictureBox();
            btnGeneratePdf = new Button();
            dataGridView1 = new DataGridView();
            picTestToBeGraded = new PictureBox();
            numericUpDown1 = new NumericUpDown();
            numericUpDown2 = new NumericUpDown();
            btnLoadControlTest = new Button();
            openFileDialog1 = new OpenFileDialog();
            openMultiFileDialog = new OpenFileDialog();
            label1 = new Label();
            btnLoadTestsToGrade = new Button();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            groupBox1 = new GroupBox();
            label8 = new Label();
            dataGridView2 = new DataGridView();
            btnApplyGradeScale = new Button();
            btnDefineGrade = new Button();
            tboxGradeDefintion = new TextBox();
            label7 = new Label();
            ((System.ComponentModel.ISupportInitialize)picControlTest).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picxGradedTests).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picTestToBeGraded).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // btnGradeTests
            // 
            btnGradeTests.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnGradeTests.Location = new Point(455, 634);
            btnGradeTests.Margin = new Padding(4, 3, 4, 3);
            btnGradeTests.Name = "btnGradeTests";
            btnGradeTests.Size = new Size(795, 82);
            btnGradeTests.TabIndex = 1;
            btnGradeTests.Text = "Grade all tests";
            btnGradeTests.UseVisualStyleBackColor = true;
            btnGradeTests.Click += btnGradeTests_Click;
            // 
            // picControlTest
            // 
            picControlTest.BackColor = SystemColors.ControlDark;
            picControlTest.Location = new Point(50, 225);
            picControlTest.Name = "picControlTest";
            picControlTest.Size = new Size(390, 403);
            picControlTest.SizeMode = PictureBoxSizeMode.Zoom;
            picControlTest.TabIndex = 2;
            picControlTest.TabStop = false;
            // 
            // picxGradedTests
            // 
            picxGradedTests.BackColor = SystemColors.ControlDark;
            picxGradedTests.Location = new Point(860, 225);
            picxGradedTests.Name = "picxGradedTests";
            picxGradedTests.Size = new Size(390, 403);
            picxGradedTests.SizeMode = PictureBoxSizeMode.Zoom;
            picxGradedTests.TabIndex = 3;
            picxGradedTests.TabStop = false;
            // 
            // btnGeneratePdf
            // 
            btnGeneratePdf.BackColor = SystemColors.ButtonHighlight;
            btnGeneratePdf.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnGeneratePdf.Location = new Point(1543, 634);
            btnGeneratePdf.Name = "btnGeneratePdf";
            btnGeneratePdf.Size = new Size(185, 78);
            btnGeneratePdf.TabIndex = 4;
            btnGeneratePdf.Text = "Generate test PDF template";
            btnGeneratePdf.UseVisualStyleBackColor = false;
            btnGeneratePdf.Click += button2_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(1262, 46);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.Size = new Size(469, 582);
            dataGridView1.TabIndex = 5;
            dataGridView1.RowPrePaint += dataGridView1_RowPrePaint;
            // 
            // picTestToBeGraded
            // 
            picTestToBeGraded.BackColor = SystemColors.ControlDark;
            picTestToBeGraded.Location = new Point(455, 225);
            picTestToBeGraded.Name = "picTestToBeGraded";
            picTestToBeGraded.Size = new Size(390, 403);
            picTestToBeGraded.SizeMode = PictureBoxSizeMode.Zoom;
            picTestToBeGraded.TabIndex = 6;
            picTestToBeGraded.TabStop = false;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(1450, 638);
            numericUpDown1.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(82, 23);
            numericUpDown1.TabIndex = 7;
            numericUpDown1.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(1450, 673);
            numericUpDown2.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            numericUpDown2.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(82, 23);
            numericUpDown2.TabIndex = 8;
            numericUpDown2.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // btnLoadControlTest
            // 
            btnLoadControlTest.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnLoadControlTest.Location = new Point(50, 634);
            btnLoadControlTest.Name = "btnLoadControlTest";
            btnLoadControlTest.Size = new Size(178, 82);
            btnLoadControlTest.TabIndex = 10;
            btnLoadControlTest.Text = "Load test with correct answers";
            btnLoadControlTest.UseVisualStyleBackColor = true;
            btnLoadControlTest.Click += btnLoadControlTest_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog";
            openFileDialog1.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp; *.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFileDialog1.Title = "Select an Image File";
            openFileDialog1.FileOk += openFileDialog1_FileOk;
            // 
            // openMultiFileDialog
            // 
            openMultiFileDialog.FileName = "openMultiFileDialog";
            openMultiFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp; *.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openMultiFileDialog.Multiselect = true;
            openMultiFileDialog.Title = "Select Image Files";
            openMultiFileDialog.FileOk += openMultiFileDialog_FileOk;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(50, 197);
            label1.Name = "label1";
            label1.Size = new Size(314, 25);
            label1.TabIndex = 11;
            label1.Text = "Control image with correct answers:";
            // 
            // btnLoadTestsToGrade
            // 
            btnLoadTestsToGrade.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnLoadTestsToGrade.Location = new Point(234, 634);
            btnLoadTestsToGrade.Name = "btnLoadTestsToGrade";
            btnLoadTestsToGrade.Size = new Size(207, 82);
            btnLoadTestsToGrade.TabIndex = 12;
            btnLoadTestsToGrade.Text = "Load tests to grade";
            btnLoadTestsToGrade.UseVisualStyleBackColor = true;
            btnLoadTestsToGrade.Click += btnLoadTestsToGrade_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(466, 197);
            label2.Name = "label2";
            label2.Size = new Size(124, 25);
            label2.TabIndex = 13;
            label2.Text = "Test to grade:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.Location = new Point(862, 197);
            label3.Name = "label3";
            label3.Size = new Size(113, 25);
            label3.TabIndex = 14;
            label3.Text = "Graded test:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.Location = new Point(1262, 18);
            label4.Name = "label4";
            label4.Size = new Size(144, 25);
            label4.TabIndex = 15;
            label4.Text = "Grading results:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(1262, 640);
            label5.Name = "label5";
            label5.Size = new Size(122, 15);
            label5.TabIndex = 16;
            label5.Text = "Number of questions:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(1262, 675);
            label6.Name = "label6";
            label6.Size = new Size(182, 15);
            label6.TabIndex = 17;
            label6.Text = "Number of answers per question:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(dataGridView2);
            groupBox1.Controls.Add(btnApplyGradeScale);
            groupBox1.Controls.Add(btnDefineGrade);
            groupBox1.Controls.Add(tboxGradeDefintion);
            groupBox1.Controls.Add(label7);
            groupBox1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupBox1.Location = new Point(50, 8);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1202, 186);
            groupBox1.TabIndex = 18;
            groupBox1.TabStop = false;
            groupBox1.Text = "Custom grading scale:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label8.Location = new Point(460, 32);
            label8.Name = "label8";
            label8.Size = new Size(181, 15);
            label8.TabIndex = 24;
            label8.Text = "Define tresholds before finishing:";
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.AllowUserToOrderColumns = true;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView2.Location = new Point(460, 50);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.Size = new Size(375, 118);
            dataGridView2.TabIndex = 19;
            // 
            // btnApplyGradeScale
            // 
            btnApplyGradeScale.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnApplyGradeScale.Location = new Point(859, 50);
            btnApplyGradeScale.Name = "btnApplyGradeScale";
            btnApplyGradeScale.Size = new Size(315, 118);
            btnApplyGradeScale.TabIndex = 23;
            btnApplyGradeScale.Text = "Apply grading scale";
            btnApplyGradeScale.UseVisualStyleBackColor = true;
            btnApplyGradeScale.Click += btnApplyGradeScale_Click;
            // 
            // btnDefineGrade
            // 
            btnDefineGrade.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnDefineGrade.Location = new Point(15, 76);
            btnDefineGrade.Name = "btnDefineGrade";
            btnDefineGrade.Size = new Size(410, 92);
            btnDefineGrade.TabIndex = 19;
            btnDefineGrade.Text = "Define new grade";
            btnDefineGrade.UseVisualStyleBackColor = true;
            btnDefineGrade.Click += btnDefineGrade_Click;
            // 
            // tboxGradeDefintion
            // 
            tboxGradeDefintion.Location = new Point(132, 28);
            tboxGradeDefintion.Name = "tboxGradeDefintion";
            tboxGradeDefintion.Size = new Size(293, 29);
            tboxGradeDefintion.TabIndex = 22;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label7.Location = new Point(15, 35);
            label7.Name = "label7";
            label7.Size = new Size(74, 15);
            label7.TabIndex = 19;
            label7.Text = "Grade name:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1740, 793);
            Controls.Add(groupBox1);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btnLoadTestsToGrade);
            Controls.Add(label1);
            Controls.Add(btnLoadControlTest);
            Controls.Add(numericUpDown2);
            Controls.Add(numericUpDown1);
            Controls.Add(picTestToBeGraded);
            Controls.Add(dataGridView1);
            Controls.Add(btnGeneratePdf);
            Controls.Add(picxGradedTests);
            Controls.Add(picControlTest);
            Controls.Add(btnGradeTests);
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load_1;
            ((System.ComponentModel.ISupportInitialize)picControlTest).EndInit();
            ((System.ComponentModel.ISupportInitialize)picxGradedTests).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)picTestToBeGraded).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnGradeTests;
        private PictureBox picControlTest;
        private PictureBox picxGradedTests;
        private Button btnGeneratePdf;
        private DataGridView dataGridView1;
        private PictureBox picTestToBeGraded;
        private NumericUpDown numericUpDown1;
        private NumericUpDown numericUpDown2;
        private Button btnLoadControlTest;
        private OpenFileDialog openFileDialog1;
        private OpenFileDialog openMultiFileDialog;
        private Label label1;
        private Button btnLoadTestsToGrade;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private GroupBox groupBox1;
        private Label label7;
        private TextBox tboxGradeDefintion;
        private Button btnApplyGradeScale;
        private Button btnDefineGrade;
        private DataGridView dataGridView2;
        private Label label8;
    }
}


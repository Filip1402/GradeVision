
using System.CodeDom.Compiler;
using System.Drawing;

namespace GradeVisionLib.Models
{

    public class Test
    {
        // Private setters for encapsulation
        public string Name { private get; set; }
        public string SubjectName { private get; set; }
        public string TeacherName { private get; set; }

        // Questions and GradeScale properties
        public HashSet<Question> Questions { get; private set; }
        public GradeScale GradeScale { get; private set; }

        // Constructor to initialize the GradeScale and create an empty list of questions
        public Test(GradeScale gradeScale)
        {
            GradeScale = gradeScale;
            Questions = new HashSet<Question>(); // Initialize an empty HashSet for questions
        }

        // Method to add a question to the test
        public void AddQuestion(Question question)
        {
            Questions.Add(question); // Add the question to the HashSet
        }

        // Optional getter methods
        public string GetTeacherName() => TeacherName;
        public string GetName() => Name;
        public string GetSubjectName() => SubjectName;

        // Method to get the AnswerSheet
        public void GetAnswerSheet()
        {   
            var outputFolder =  Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/ProcessedImages/answerSheet.pdf";
            new AnswerSheet().GeneratePdfAnswerSheet(this, outputFolder);
        }

        public void generatePdf()
        {
            // Code to generate a PDF of the test
        }
    }
}

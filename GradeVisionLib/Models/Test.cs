
using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace GradeVisionLib.Models
{

    public class Test
    {
        public HashSet<Question> Questions { get; private set; }

        public Test(int numOfquestions, int numOfAnswersPerQuestion)
        {
            Questions = new HashSet<Question>();
            for (int i = 0; i < numOfquestions; i++)
            {
                var question = new Question(numOfAnswersPerQuestion);
                Questions.Add(question);
            }   
        }
        public void GetAnswerSheet()
        {   
            var outputFolder =  Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/ProcessedImages/answerSheet.pdf";
            new AnswerSheet().GeneratePdfAnswerSheet(this, outputFolder);
        }

    }
}

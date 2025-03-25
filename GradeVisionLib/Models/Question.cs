using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeVisionLib.Models
{
    public abstract class Question
    {
        public  List<Answer> Answers {get; }

        private List<Answer> CorrectAnswers;

        public Question(List<Answer> answers, List<Answer> correctAnswers, int points)
        {
            Answers = answers;
            CorrectAnswers = correctAnswers;
        }

        

        //TODO: add answer comparison and scoring logic
    }
}

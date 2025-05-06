using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeVisionLib.Models
{
    public  class Question
    {
        public List<Answer> Answers {get; }

        public Question(List<Answer> answers)
        {
            Answers = answers;
        }

        public Question(int numOfAnswers)
        {
            Answers = new List<Answer>();
            for (int i = 0; i < numOfAnswers; i++)
            {
                var label = (char)('A' + i);
                var answer = new Answer(label.ToString());
                Answers.Add(answer);
            }
        }
    }
}

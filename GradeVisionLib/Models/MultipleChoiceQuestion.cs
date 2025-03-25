using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeVisionLib.Models
{
    public class MultipleChoiceQuestion : Question
    {
        public MultipleChoiceQuestion(List<Answer> answers, List<Answer> correctAnswers, int points) : base(answers, correctAnswers, points)
        {
        }
    }
}

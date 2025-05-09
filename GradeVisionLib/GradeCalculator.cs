using Emgu.CV.Structure;
using GradeVisionLib.Interfaces;
using GradeVisionLib.Models;
using Lombok.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeVisionLib
{
    [RequiredArgsConstructor(MemberType = MemberType.Field, AccessTypes = AccessTypes.Private)]
    public partial class GradeCalculator
    {
        private readonly GradeScale GradeScale;
        private readonly Dictionary<int, List<DetectedCircleBase>> ControlTest;
        private readonly Dictionary<int, List<DetectedCircleBase>> StudentTest;
        private const double INVALID_TEST_SCORE = -100;

        public (string, double) GetGrade()
        {
            var score = CalculateTestScore();
            return (GradeScale.GetGrade(score), score);
        }
        private double CalculateTestScore()
        {
            if (TestsWithMismatchingStructure())
            {
                return INVALID_TEST_SCORE;
            }

            var totalScore = 0.0;
            for (var i = 0; i < ControlTest.Count; i++)
            {
                var controlTestQuestion = ControlTest.ElementAt(i).Value;
                var studentTestQuestion = StudentTest.ElementAt(i).Value;
                if (controlTestQuestion.Count != studentTestQuestion.Count)
                    throw new ArgumentException("There is a mismatch between number of answers between student and control test.");

                var numOfCorrectAnswersForQuestionControl = controlTestQuestion.Count((answer) => answer.IsMarked);
                var numOfCorrectAnswersForQuestionStudent = studentTestQuestion.Count((answer) => answer.IsMarked);

                if (numOfCorrectAnswersForQuestionStudent != numOfCorrectAnswersForQuestionControl)
                {
                    return INVALID_TEST_SCORE;
                }

                var numOfCorrectStudentAnswers = 0;
                for (var j = 0; j < controlTestQuestion.Count; j++)
                {
                    var controlAnswer = controlTestQuestion[j];
                    var studentAnswer = studentTestQuestion[j];
                    if (controlAnswer.IsMarked && studentAnswer.IsMarked)
                    {
                        numOfCorrectStudentAnswers++;
                    }
                }
                totalScore += (double)numOfCorrectStudentAnswers / numOfCorrectAnswersForQuestionControl;

            }
            return totalScore / ControlTest.Count * 100;

        }
        private bool TestsWithMismatchingStructure()
        {
            return ControlTest.Count == 0 || StudentTest.Count == 0 || ControlTest.Count != StudentTest.Count;
        }
    }
}

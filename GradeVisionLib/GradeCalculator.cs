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
        private readonly Dictionary<int, List<DetectedCircleBase>> ControlTestQuestions;
        private readonly Dictionary<int, List<DetectedCircleBase>> StudentTestQuestions;
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
            for (var i = 0; i < ControlTestQuestions.Count; i++)
            {
                var controlTestQuestion = ControlTestQuestions.ElementAt(i).Value;
                var studentTestQuestion = StudentTestQuestions.ElementAt(i).Value;
                if (controlTestQuestion.Count != studentTestQuestion.Count)
                    throw new ArgumentException("There is a mismatch between number of answers between student and control test.");

                var controlContainsUnansweredQuestions = ControlTestQuestions
                    .Any(question => question.Value.Count == 0 || question.Value.All(answer => !answer.IsMarked));

                if (controlContainsUnansweredQuestions)
                    throw new ArgumentException("Control test doesn't have all question answered.");


                var controlMarked = controlTestQuestion.Select(answer => answer.IsMarked).ToList();
                var studentMarked = studentTestQuestion.Select(answer => answer.IsMarked).ToList();

                if (controlMarked.SequenceEqual(studentMarked))
                {
                    totalScore += 1.0;
                }
                
            }
            return totalScore / ControlTestQuestions.Count * 100;

        }

        private bool TestsWithMismatchingStructure()
        {
            return ControlTestQuestions.Count == 0 || StudentTestQuestions.Count == 0 || ControlTestQuestions.Count != StudentTestQuestions.Count;
        }
    }
}

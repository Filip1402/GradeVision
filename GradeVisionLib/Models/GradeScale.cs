using System;
using System.Collections.Generic;
using System.Linq;

namespace GradeVisionLib.Models
{
    public class GradeScale
    {
        private readonly double INVALID_TEST_TRESHOLD = 0;
        public List<string> GradeDefinitions { get; set; }
        public List<double> Thresholds { get; set; }

        public GradeScale(List<string> gradeDefinitions, List<double> thresholds)
        {
            if (gradeDefinitions.Count - 1 != thresholds.Count)
            {
                throw new ArgumentException("The number of grade definitions must be one more than the number of thresholds.");
            }
            gradeDefinitions.Insert(0, "NOT GRADED");
            GradeDefinitions =   gradeDefinitions;
            thresholds.Insert(0, INVALID_TEST_TRESHOLD);
            Thresholds = thresholds;
        }

        public string GetGrade(double score)
        {
            string grade = GradeDefinitions.First();
            for (int i = 0; i < Thresholds.Count; i++)
            {
                if (score >= Thresholds[i])
                {
                    grade = GradeDefinitions[i + 1];
                }
                else
                    break;
            }

            return grade;
        }

    }
}

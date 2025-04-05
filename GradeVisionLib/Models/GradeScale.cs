using System;
using System.Collections.Generic;
using System.Linq;

namespace GradeVisionLib.Models
{
    public class GradeThreshold
    {
        public string Grade { get; set; }
        public double Threshold { get; set; }

        public GradeThreshold(string grade, double threshold)
        {
            Grade = grade;
            Threshold = threshold;
        }
    }

    public class GradeScale
    {
        public List<string> GradeDefinitions { get; set; }
        public List<double> Thresholds { get; set; }

        public GradeScale(List<string> gradeDefinitions, List<double> thresholds)
        {
            if (gradeDefinitions.Count - 1 != thresholds.Count)
            {
                throw new ArgumentException("The number of grade definitions must be one more than the number of thresholds.");
            }

            GradeDefinitions = gradeDefinitions;
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

        // Method to display the grade definitions and thresholds
        public void DisplayGradeThresholds()
        {
            for (int i = 0; i < Thresholds.Count; i++)
            {
                Console.WriteLine($"{GradeDefinitions[i]}: {Thresholds[i]}%");
            }
            Console.WriteLine($"{GradeDefinitions.Last()}: Below {Thresholds.Last()}%");
        }
    }
}

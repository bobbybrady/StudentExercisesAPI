using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesAPI.Models
{
    public class Student
    {
        public string SlackHandle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CohortId { get; set; }
        public int Id { get; set; }
        public Cohort Cohort { get; set; }
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}

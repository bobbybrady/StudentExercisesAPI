using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExercisesAPI.Models;

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CohortController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: api/Student
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "Select s.slackHandle as StudentSlack, s.id as StudentID, s.cohortId as StudentCohortId, s.firstName as StudentFirstName, s.lastName as StudentLastName, i.FirstName as InstructorFirstName, i.LastName as InstructorLastName, i.slackHandle as InstructorSlack, i.id as InstructorId, i.cohortId as InstructorCohortId, i.specialty, c.Name as CohortName, c.id as CohortId from Student s left join Cohort c on c.id = s.cohortId left join Instructor i on c.id = i.CohortId";
                    SqlDataReader reader = cmd.ExecuteReader();
                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();

                    while (reader.Read())
                    {
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.ContainsKey(cohortId))
                        {
                            Cohort cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                            };
                            cohorts.Add(cohortId, cohort);
                        }
                        Cohort fromDictionary = cohorts[cohortId];

                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            Student aStudent = new Student()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort(),

                                Exercises = new List<Exercise>()
                            };
                            List<Student> whereList = fromDictionary.students.Where(s => s.Id == reader.GetInt32(reader.GetOrdinal("StudentId"))).ToList();
                            if (whereList.Count == 0)
                            {
                                fromDictionary.students.Add(aStudent);
                            }
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            Instructor aInstructor = new Instructor()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Specialty = reader.GetString(reader.GetOrdinal("specialty")),
                                Cohort = new Cohort(),
                            };
                            List<Instructor> whereList = fromDictionary.instructors.Where(i => i.Id == reader.GetInt32(reader.GetOrdinal("InstructorId"))).ToList();
                            if (whereList.Count == 0)
                            {
                                fromDictionary.instructors.Add(aInstructor);
                            }
                        }
                    }
                    reader.Close();
                    return Ok(cohorts.Values);
                }


            }
        }

    }
}


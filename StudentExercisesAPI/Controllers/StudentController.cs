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
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
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
        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            id, firstName, lastName,cohortId, slackHandle
                        FROM Student
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read())
                    {
                        student = new Student
                        {
                            FirstName = reader.GetString(reader.GetOrdinal("firstName")),
                            LastName = reader.GetString(reader.GetOrdinal("lastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("cohortId")),
                            Id = id
                        };
                    }
                    reader.Close();

                    return Ok(student);
                }
            }
        }
        // GET: api/Student
        [HttpGet]
        public async Task<IActionResult> Get(string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "exercise")
                    {
                        cmd.CommandText = "Select s.slackHandle, s.id, s.cohortId, s.firstName, s.lastName, c.Name, e.Name as ExerciseName, e.Language as ExerciseLanguage, e.Id as ExerciseId from Student s left join Cohort c on c.id = s.cohortId left join StudentExercise se on se.StudentId = s.Id left join Exercise e on se.ExerciseId = e.Id and se.StudentId = s.Id";
                    }
                    else
                    {
                        cmd.CommandText = "Select s.slackHandle, s.id, s.cohortId, s.firstName, s.lastName, c.Name from Student s left join Cohort c on c.id = s.cohortId";
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    Dictionary<int, Student> students = new Dictionary<int, Student>();

                    while (reader.Read())
                    {
                        int studentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        if (!students.ContainsKey(studentId))
                        {
                            Student student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("firstName")),
                                LastName = reader.GetString(reader.GetOrdinal("lastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("slackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    students = new List<Student>(),
                                    instructors = new List<Instructor>()
                                },
                            };
                            students.Add(studentId, student);
                        }
                        Student fromDictionary = students[studentId];
                        if (include == "exercise" && !reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                        {
                            Exercise aExercise = new Exercise()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage")),
                            };
                            List<Exercise> whereList = fromDictionary.Exercises.Where(e => e.Id == reader.GetInt32(reader.GetOrdinal("ExerciseId"))).ToList();
                            if (whereList.Count == 0)
                            {
                                fromDictionary.Exercises.Add(aExercise);
                            }
                        }
                    }
                    reader.Close();

                    return Ok(students.Values);
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post(Student student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Student (firstName, lastName, slackHandle, cohortId) Values (@firstName, @lastName, @slackHandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));
                    cmd.ExecuteNonQuery();
                    return Ok(student);
                }
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int Id,[FromBody] Student student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "Update Student set firstName = @firstName, lastName = @lastName, slackHandle = @slackHandle, cohortId = @cohortId where id = @id";
                    cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@id", Id));
                    cmd.ExecuteNonQuery();
                    return Ok(student);
                }
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"Delete from StudentExercise where studentId = @id 
                    Delete from Student where id = @id";
                    
                    cmd.Parameters.Add(new SqlParameter("@id", Id));
                    cmd.ExecuteNonQuery();
                    return Ok("Deleted Student");
                }
            }
        }
    }
}

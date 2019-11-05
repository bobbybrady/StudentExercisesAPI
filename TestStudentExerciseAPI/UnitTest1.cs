using Newtonsoft.Json;
using StudentExercisesAPI.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestKennelAPI;
using Xunit;

namespace TestStudentExerciseAPI
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test_Get_All_Students()
        {

            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/student");


                string responseBody = await response.Content.ReadAsStringAsync();
                var studentList = JsonConvert.DeserializeObject<List<Student>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(studentList.Count > 0);
            }
        }
        [Fact]
        public async Task Test_Modify_Student()
        {
            // New last name to change to and test
            int newCohortId = 2;

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                */
                Student modifiedHarry = new Student
                {
                    FirstName = "Harry",
                    LastName = "Harrison",
                    CohortId = newCohortId,
                    SlackHandle = "slack1"
                };
                var modifiedHarryAsJSON = JsonConvert.SerializeObject(modifiedHarry);

                var response = await client.PutAsync(
                    "/api/student/1",
                    new StringContent(modifiedHarryAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);


                /*
                    GET section
                    Verify that the PUT operation was successful
                */
                var getHarry = await client.GetAsync("/api/student/1");
                getHarry.EnsureSuccessStatusCode();

                string getHarryBody = await getHarry.Content.ReadAsStringAsync();
                Student newHarry = JsonConvert.DeserializeObject<Student>(getHarryBody);

                Assert.Equal(HttpStatusCode.OK, getHarry.StatusCode);
                Assert.Equal(newCohortId, newHarry.CohortId);
            }
        }
        [Fact]
        public async Task Test_Delete_Student()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.DeleteAsync("/api/student/3");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);


                var getDeletedStudent = await client.GetAsync("/api/student/3");
                getDeletedStudent.EnsureSuccessStatusCode();

                

                Assert.Equal(HttpStatusCode.NoContent, getDeletedStudent.StatusCode);
            }
        }
    }
}

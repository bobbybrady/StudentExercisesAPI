﻿Select s.slackHandle as StudentSlack, s.id as StudentID, s.cohortId as StudentCohortId, s.firstName as StudentFirstName, s.lastName as StudentLastName, i.FirstName as InstructorFirstName, i.LastName as InstructorLastName, i.slackHandle as InstructorSlack, i.cohortId as InstructorCohortId, i.specialty, c.Name as CohortName from Student s inner join Cohort c on c.id = s.cohortId left join Instructor i on c.id = i.CohortId
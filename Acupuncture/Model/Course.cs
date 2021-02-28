using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Acupuncture.Model
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public IList<StudentCourse> StudentCourses { get; set; }
    }
}

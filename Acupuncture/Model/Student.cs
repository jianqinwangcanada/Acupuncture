using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Acupuncture.Model
{
    public class Student
    {   [Key]
        public int StudentId { get; set; }
      public string Name { get; set; }
        public IList<StudentCourse> StudentCourses { get; set; }
    }
}

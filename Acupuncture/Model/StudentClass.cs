using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Acupuncture.Model
{
    public class StudentCourse
    {
        
        public string enrollment { get; set; }
        public int StudentId { get; set; }
        
        public int CourseId { get; set; }
        public virtual Student student { get; set; }
        public virtual Course course { get; set; }

    }
}

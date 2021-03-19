using System;
using System.ComponentModel.DataAnnotations;

namespace Acupuncture.Model
{
    public class PermissionType
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject.Models
{
    public class ClassType
    {
        public long ClassTypeID { get; set; }

        [Required]
        [MaxLength(255)]
        public string ClassName { get; set; }

        [Required]
        public string ClassDescription { get; set; }

        [Required]
        public int MaxCapacity { get; set; }

        //[ForeignKey("FK_ClassType_User_UserID")]
        [Required]
        public long InstructorID { get; set; }
        public virtual Instructor Instructor { get; set; }

        //[ForeignKey("FK_ClassType_Location_LocationID")]
        [Required]
        public long LocationID { get; set; }
        public virtual Location Location { get; set; }

        public virtual ICollection<ClassSchedule> ClassSchedules { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
    }
}

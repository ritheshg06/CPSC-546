using GroupProject.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject.Models
{
    public class Location
    {
        public long LocationID { get; set; }

        [Required]
        [MaxLength(100)]
        public string LocationName { get; set; }

        [Required]
        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        [Required]
        [MaxLength(255)]
        public string City { get; set; }

        [Required]
        [MaxLength(255)]
        public string State { get; set; }

        [Required]
        public int Pincode { get; set; }

        [Required]
        [MaxLength(255)]
        public string Country { get; set; }

        [Required]
        public string TimeZone { get; set; }


        public virtual ICollection<ClassType> ClassTypes { get; set; }

        public virtual ICollection<RoomLayout> RoomLayouts { get; set; }

        public virtual ICollection<Instructor> Instructors { get; set; }
    }
}

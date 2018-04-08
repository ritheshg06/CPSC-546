using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject.Models
{
    public class Class
    {
        public long ClassID { get; set; }

        [Required]
        public long ClassScheduleID { get; set; }
        public ClassSchedule ClassSchedule { get; set; }

        [Required]
        public long ClassTypeID { get; set; }
        public ClassType ClassType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public bool IsCancelled { get; set; }

        [Required]
        public long? InstructorID { get; set; }
        public virtual Instructor SubstituteInstructor { get; set; }

        [Required]
        public bool AllowWaitlist { get; set; }

        [Required]
        public int CancelOffset { get; set; }

        public bool DisabledForView { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}

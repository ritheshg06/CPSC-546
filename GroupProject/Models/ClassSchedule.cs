using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject.Models
{
    public class ClassSchedule
    {
        public ClassSchedule()
        {
            Days = "";
        }
        public long ClassScheduleID { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string ScheduleName { get; set; }

        [Required]
        public long ClassTypeID { get; set; }
        public virtual ClassType ClassType { get; set; }

        [Required]
        public long? RoomLayoutID { get; set; }
        public virtual RoomLayout RoomLayout { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public string Days { get; set; }

        public string[] SelectedDays
        {
            get
            {
                return Days.Split(',');
            }
            set
            {
                Days = string.Join(", ", value);
            }
        }

        public virtual ICollection<Class> Classes { get; set; }
    }
}

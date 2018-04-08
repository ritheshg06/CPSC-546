using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject.Models
{
    public class RoomLayout
    {
        public RoomLayout()
        {
            MaxSeats = 100;
        }
        public long RoomLayoutID { get; set; }

        [Required]
        [MaxLength(255)]
        public string RoomName { get; set; }

        [Required]
        public string SeatMatrix { get; set; }

        [Required]
        public int Rows { get; set; }
        [Required]
        public int Columns { get; set; }
        [Required]
        public int MaxSeats { get; set; }
        [Required]
        public int ConfiguredSeats { get; set; }

        //[ForeignKey("FK_RoomLayout_Location_LocationID")]
        [Required]
        public long LocationID { get; set; }
        public virtual Location Location { get; set; }

        public virtual ICollection<ClassSchedule> ClassSchedules { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}

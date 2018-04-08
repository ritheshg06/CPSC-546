using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject.Models
{
    public class Reservation
    {
        public long ReservationID { get; set; }

        [Required]
        public long ClassID { get; set; }
        public virtual Class Class { get; set; }

        [Required]
        public long? RoomLayoutID { get; set; }
        public virtual RoomLayout RoomLayout { get; set; }

        [Required]
        public long? UserID { get; set; }
        public virtual User User { get; set; }

        public int SeatNumber { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }


        //	1 - Enrolled
        //	2 - Cancelled
        //	3 - Maintenance
        //	4 - LateCancelled
        //  9 - WaitListed
        //  10 - Class Cancelled
        [Required]
        public int ReservationStatus { get; set; }

        public int SignedIn { get; set; }

        public DateTime? Timestamp { get; set; }

        public DateTime? UpdateTimestamp { get; set; }
    }
}

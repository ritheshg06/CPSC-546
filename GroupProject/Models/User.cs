using GroupProject.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject.Models
{
    public class User
    {
        public long UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(255)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Password { get; set; }


        public DateTime DateOfBirth { get; set; }
        

        public int _Gender { get; set; }

        public string Gender
        {
            get
            {
                if (_Gender == 1)
                    return "Male";
                else if (_Gender == 2)
                    return "Female";
                return "Others";
            }
        }


        public string AddressLine1 { get; set; }

  
        public string AddressLine2 { get; set; }

 
        [MaxLength(255)]
        public string City { get; set; }


        [MaxLength(255)]
        public string State { get; set; }

      
        public int Pincode { get; set; }

    
        [MaxLength(255)]
        public string Country { get; set; }

        public string WorkPhone { get; set; }

        public string Phone { get; set; }

       
        public int _UserRole { get; set; }

        
        public bool ActivatedFlag { get; set; }

        public bool DeletedFlag { get; set; }

        public string UserRole
        {
            get
            {
                if (_UserRole == UserRoles.Administrator)
                    return "Administrator";
                if (_UserRole == UserRoles.Instructor)
                    return "Instructor";
                return "";
            }
        }

        public double? Height { get; set; }

        public double? Weight { get; set; }

        [NotMapped]
        public string Designation { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject.Models
{
    public class Instructor:User
    {
        [Required]
        public double HourlyRate { get; set; }
        [Required]
        public string Bio { get; set; }
        [Required]
        public string Description { get; set; }
        public string Quote { get; set; }
        public int SortOrder { get; set; }
        public string FacebookURL { get; set; }
        public string InstagramURL { get; set; }
        public string TwitterURL { get; set; }
        public string SpotifyURL { get; set; }
        public string SoundCloudURL { get; set; }
        [Required]
        public bool DisplayInstructorFlag { get; set; }

        [Required]
        public long? LocationID { get; set; }
        public virtual Location Location { get; set; }

        public virtual ICollection<ClassType> ClassTypes { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
    }
}

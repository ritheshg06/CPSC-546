using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject.Models
{
    public class ClassPackage
    {
        public long ClassPackageID { get; set; }

        [Required]
        [MaxLength(255)]
        public string ClassPackageName { get; set; }

        public string ClassPackageDescription { get; set; }

        [Required]
        public double Price { get; set; }

        public int ExpirationDuration { get; set; }

        public int SortOrder { get; set; }

        [Required]
        public int ClassCount { get; set; }

        public string Badge { get; set; }

        public bool IsActiveFlag { get; set; }

        public bool IsFeaturedFlag { get; set; }

        public string ClassTypes { get; set; }
    }
}

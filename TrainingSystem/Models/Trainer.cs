//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TrainingSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public partial class Trainer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Trainer()
        {
            this.AssignCourse = new HashSet<AssignCourse>();
            this.Request = new HashSet<Request>();
        }
    
        public int trainerID { get; set; }

        [Required]
        [DisplayName("Username")]
        public string trainer_username { get; set; }

        [Required]
        [DisplayName("Password")]
        public string trainer_password { get; set; }

        [Required]
        [DisplayName("Fullname")]
        public string trainer_fullname { get; set; }

        [Required]
        [DisplayName("Address")]
        public string trainer_address { get; set; }

        [Required]
        [DisplayName("Workingplace")]
        public string trainer_workingplace { get; set; }

        [Required]
        [DisplayName("Phone")]
        public string trainer_phone { get; set; }

        [Required]
        [DisplayName("Email")]
        public string trainer_email { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AssignCourse> AssignCourse { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Request> Request { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrainingSystem.Models
{
    public class TraineeViewModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TraineeViewModel()
        {
            this.Enroll = new HashSet<Enroll>();
        }

        public int traineeID { get; set; }

        [DisplayName("Username")]
        public string trainee_username { get; set; }

        [DisplayName("Password")]
        public string trainee_password { get; set; }

        [DisplayName("Fullname")]
        public string trainee_fullname { get; set; }

        [DisplayName("Age")]
        public Nullable<int> trainee_age { get; set; }

        [DisplayName("Date of birth")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> trainee_birthday { get; set; }

        [DisplayName("Education")]
        public string trainee_education { get; set; }

        [DisplayName("Email")]
        public string trainee_email { get; set; }

        [DisplayName("Phone")]
        public string trainee_phone { get; set; }

        [DisplayName("Experience")]
        public string trainee_experience_details { get; set; }

        [DisplayName("Department")]
        public string trainee_department { get; set; }

        [DisplayName("Address")]
        public string trainee_address { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Enroll> Enroll { get; set; }
    }
}
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

    public partial class Events
    {
        public int eventID { get; set; }



        [DisplayName("Room")]
        public string room { get; set; }


        [DisplayName("Description")]
        public string event_descriptions { get; set; }

        [Required]
        [DisplayName("Start")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:ddd MM/dd/yyyy HH:mm}")]
        public System.DateTime event_start { get; set; }

        [Required]
        [DisplayName("End")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:HH:mm}")]
        public System.DateTime event_end { get; set; }
        public Nullable<int> courseID { get; set; }
    
        public virtual Courses Courses { get; set; }
    }
}

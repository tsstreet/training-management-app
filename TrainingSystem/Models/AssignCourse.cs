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

    public partial class AssignCourse
    {
        public int asscourseID { get; set; }


        [DisplayName("Description")]
        public string asscourse_descriptions { get; set; }
        public int courseID { get; set; }
        public int trainerID { get; set; }
    
        public virtual Courses Courses { get; set; }
        public virtual Trainer Trainer { get; set; }
    }
}

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

    public partial class Topic
    {
        public int topicID { get; set; }

        [Required]
        [DisplayName("Topic Name")]
        public string topic_name { get; set; }


        [DisplayName("Description")]
        public string topic_descriptions { get; set; }
        public int courseID { get; set; }

        [Required]
        [DisplayName("Course Time")]
        public string course_time { get; set; }

  
        [DisplayName("Status")]
        public string topic_status { get; set; }
    
        public virtual Courses Courses { get; set; }
    }
}

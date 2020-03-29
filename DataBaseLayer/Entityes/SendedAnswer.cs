using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataBaseLayer.Entityes
{
    public class SendedAnswer : BaseEntity
    {
        [Required]
        public int AnswerId { get; set; }
        [Required]
        public int MessageId { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual Answer Answer { get; set; }

    }
}

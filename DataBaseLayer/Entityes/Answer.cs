using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataBaseLayer.Entityes
{
    public class Answer : BaseEntity
    {
        public int QuestionId { get; set; }
        [Required]
        [MaxLength(512)]
        public string Text { get; set; }
        [Required]
        public virtual Question Question { get; set; }

        public virtual ICollection<SendedAnswer> SendedAnswer { get; set; }
    }
}

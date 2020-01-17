using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseLayer.Entityes
{
    public class Question : BaseEntity
    {
        public Question()
        {
            Answers = new List<Answer>();
        }
        [Required]
        [MaxLength(128)]
        public string Text { get; set; }
        [Required]
        public int Quantity { get; set; }

        [Required]
        [ForeignKey("Creator")]
        public int CreatorId { get; set; }
        [ForeignKey("Updater")]
        public int? UpdatedId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }
        public virtual TelegramUser Creator { get; set; }
        public virtual TelegramUser Updater { get; set; }
    }
}

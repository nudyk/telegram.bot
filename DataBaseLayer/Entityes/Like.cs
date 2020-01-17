using System;
using System.ComponentModel.DataAnnotations;

namespace DataBaseLayer.Entityes
{
    public class Like : BaseEntity
    {
        [Required] public long ChatId { get; set; }
        [Required] public int MessageId { get; set; }
        [Required] public int TelegramUserId { get; set; }
        [Required] public bool IsLike { get; set; }
        [Required] public DateTime CreatedDate { get; set; }

        public virtual TelegramUser TelegramUser { get; set; }
    }
}
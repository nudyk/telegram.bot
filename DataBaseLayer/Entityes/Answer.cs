using System.ComponentModel.DataAnnotations;

namespace DataBaseLayer.Entityes
{
    public class Answer : BaseEntity
    {
        public int QustionId { get; set; }
        [Required]
        [MaxLength(512)]
        public string Text { get; set; }
        [Required]
        public virtual Question Question { get; set; }
    }
}

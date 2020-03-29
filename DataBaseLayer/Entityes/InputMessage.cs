using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseLayer.Entityes
{
    public class InputMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int MessageId { get; set; }
        [Required]
        public string Text { get; set; }
    }
}

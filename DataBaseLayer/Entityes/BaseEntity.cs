using System.ComponentModel.DataAnnotations;

namespace DataBaseLayer.Entityes
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}

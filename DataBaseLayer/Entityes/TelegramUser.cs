using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseLayer.Entityes
{
    public class TelegramUser : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public new int Id { get; set; }
        // Summary:
        //     True, if this user is a bot
        [Required] public bool IsBot { get; set; }

        //
        // Summary:
        //     User's or bot's first name
        [Required] [MaxLength(128)] public string FirstName { get; set; }

        //
        // Summary:
        //     Optional. User's or bot's last name
        [MaxLength(128)] public string LastName { get; set; }

        //
        // Summary:
        //     Optional. User's or bot's username
        [MaxLength(128)] public string Username { get; set; }

        //
        // Summary:
        //     Optional. IETF language tag of the user's language
        [MaxLength(128)] public string LanguageCode { get; set; }
        [Required]
        public bool IsCanAddAnswers { get; set; }
    }
}
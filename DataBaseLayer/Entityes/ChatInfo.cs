using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseLayer.Entityes
{
    public class ChatInfo: BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
         //
        // Summary:
        //     Unique identifier for this chat, not exceeding 1e13 by absolute value
        public new long Id { get; set; }
        //
        // Summary:
        //     Type of chat
        [Required]
        public int Type { get; set; }
        //
        // Summary:
        //     Optional. Title, for channels and group chats
        public string Title { get; set; }
        //
        // Summary:
        //     Optional. Username, for private chats and channels if available
        public string Username { get; set; }
        //
        // Summary:
        //     Optional. First name of the other party in a private chat
        public string FirstName { get; set; }
        //
        // Summary:
        //     Optional. Last name of the other party in a private chat
        public string LastName { get; set; }
        //
        // Summary:
        //     Optional. Description, for supergroups and channel chats. Returned only in getChat.
        public string Description { get; set; }
        //
        // Summary:
        //     Optional. Chat invite link, for supergroups and channel chats. Returned only
        //     in getChat.
        public string InviteLink { get; set; }
        //
        // Summary:
        //     Optional. For supergroups, the minimum allowed delay between consecutive messages
        //     sent by each unpriviledged user. Returned only in getChat.
        public int? SlowModeDelay { get; set; }
        //
        // Summary:
        //     Optional. For supergroups, name of group sticker set. Returned only in getChat.
        public string StickerSetName { get; set; }
        //
        // Summary:
        //     Optional. True, if the bot can change the group sticker set. Returned only in
        //     getChat.
        public bool? CanSetStickerSet { get; set; }

        /// <summary>
        /// Is not send messages to this chat
        /// </summary>
        public bool IsSelentMode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

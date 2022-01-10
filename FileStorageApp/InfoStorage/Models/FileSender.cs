﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageApp.Data.InfoStorage.Models
{
    [Table("FileSenders")]
    public class FileSender : IModel
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public long TelegramId { get; set; }

        [Required]
        [MaxLength(255)]
        public string TelegramUserName { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; }

        public virtual ICollection<File> Files { get; set; } = new List<File>();

        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}
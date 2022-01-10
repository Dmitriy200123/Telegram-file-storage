﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageApp.Data.InfoStorage.Models
{
    [Table("Users")]
    public class User : IModel
    {
        [Key]
        public Guid Id { get; set; }

        public long? TelegramId { get; set; }

        public int GitLabId { get; set; }

        public string Name { get; set; }

        public string Avatar { get; set; }

        public string RefreshToken { get; set; }

        [ForeignKey("UserId")]
        public virtual ICollection<Right> Rights { get; set; }
    }
}
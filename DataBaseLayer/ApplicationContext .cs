using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DataBaseLayer.Entityes;

namespace DataBaseLayer
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<SendedAnswer> SendedAnswers { get; set; }
        public DbSet<InputMessage> InputMessages { get; set; }
        public DbSet<ChatInfo> ChatInfos { get; set; }

        /* 
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        */  
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TelegramUser>()
                .Property(b => b.IsCanAddAnswers)
                .HasDefaultValue(false);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("<connection string>");

        }
    }
}

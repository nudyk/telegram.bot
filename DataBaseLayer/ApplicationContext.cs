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
            //optionsBuilder.UseNpgsql("Host=raspberrypi.local;Port=5432;Database=telegrambot;Username=postgres;Password=ifjoktyt");
            //optionsBuilder.UseNpgsql("Host=192.168.88.199;Port=5432;Database=telegrambot;Username=postgres;Password=ifjoktyt");
            optionsBuilder.UseNpgsql("Host=192.168.88.128;Port=5432;Database=telegrambot;Username=nudyk;Password=ifjoktyt");
        }
    }
}

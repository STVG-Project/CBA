using CBA.Models;
using Microsoft.EntityFrameworkCore;

namespace CBA.Models
{
    public class DataContext : DbContext
    {
        public static Random random = new Random();
        public DbSet<SqlUser>? users { get; set; }
        public DbSet<SqlRole>? roles { get; set; }
        public DbSet<SqlPerson>? persons { get; set; }
        public DbSet<SqlDevice>? devices { get; set; }
        public DbSet<SqlGroup>? groups { get; set; }
        public DbSet<SqlFace>? faces { get; set; }
        public DbSet<SqlFile>? files { get; set; }
        public DbSet<SqlLogPerson>? logs { get; set; }


        public static string randomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string configSql = "Host=office.stvg.vn:59061;Database=db_stvg_cba;Username=postgres;Password=stvg";

        //public static string configSql = "Host=192.168.1.241:60001;Database=db_stvg_cba;Username=postgres;Password=stvg";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql(configSql);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SqlGroup>().HasMany<SqlPerson>(s => s.persons).WithOne(s => s.group);
            modelBuilder.Entity<SqlPerson>().HasMany<SqlFace>(s => s.faces).WithOne(s => s.person);
        }

    }
}

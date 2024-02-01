using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoApp.Model
{
    public class dbContext : DbContext
    {
        public dbContext() : base("KinoDB")
        {
        }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Film> Films { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Film>().Property(p => p.ID_Film).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Film>()
            .HasMany(c => c.Genres)
            .WithMany(a => a.Films)
            .Map(m =>
            {
                m.ToTable("GenreFilm");
                m.MapLeftKey("FilmId");
                m.MapRightKey("GenreId");
            });
        }

    }
}

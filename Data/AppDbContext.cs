
using Microsoft.EntityFrameworkCore;
using AI_Art_Gallery.Models;

namespace AI_Art_Gallery.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanındaki "Artworks" tablosu bu satırla temsil edilir.
        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<AppLog> AppLogs { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
using ExercicioAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace ExercicioAPI.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      /*
      //defining relation between Book and BookUser
      modelBuilder.Entity<BookUser>()
        .HasOne(b => b.Book)
        .WithMany(bu => bu.BooksUsers)
        .HasForeignKey(bi => bi.BookId);
      */
      //defining relation between User and BookUser
      modelBuilder.Entity<BookUser>()
        .HasOne(b => b.User)
        .WithMany(bu => bu.BooksUsers)
        .HasForeignKey(bi => bi.UserId);
    }
    public DbSet<Book> Books { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<BookUser> BooksUsers { get; set; }

  }
}
using Lab4Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Lab4Web.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<BorrowedBook> BorrowedBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BorrowedBook>()
                .HasKey(bb => new { bb.ReaderId, bb.BookId });

            modelBuilder.Entity<BorrowedBook>()
                .HasOne(bb => bb.Reader)
                .WithMany(r => r.BorrowedBooks)
                .HasForeignKey(bb => bb.ReaderId);

            modelBuilder.Entity<BorrowedBook>()
                .HasOne(bb => bb.Book)
                .WithMany()
                .HasForeignKey(bb => bb.BookId);
        }
    }
}

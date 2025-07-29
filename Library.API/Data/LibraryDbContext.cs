using Microsoft.EntityFrameworkCore;
namespace Library.API.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();

        public DbSet<AuthorHistory> AuthorsHistory => Set<AuthorHistory>();
        public DbSet<BookHistory> BooksHistory => Set<BookHistory>();
        public DbSet<BookAuthorHistory> BookAuthorsHistory => Set<BookAuthorHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Author>(entity =>
 {
     entity.HasKey(a => a.AuthorId);

     entity.Property(a => a.FirstName)
         .IsRequired()
         .HasMaxLength(60)
         .IsUnicode(true);

     entity.Property(a => a.LastName)
         .IsRequired()
         .HasMaxLength(60)
         .IsUnicode(true);

     entity.Property(a => a.PenName)
         .IsRequired()
         .HasMaxLength(60)
         .IsUnicode(true);

     entity.HasIndex(a => a.FirstName).HasDatabaseName("IDX_Authors_FirstName");
     entity.HasIndex(a => a.LastName).HasDatabaseName("IDX_Authors_LastName");
     entity.HasIndex(a => a.PenName).HasDatabaseName("IDX_Authors_PenName");

     entity.ToTable("Authors");
 });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(b => b.BookId);

                entity.Property(b => b.Title)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(true);

                entity.Property(b => b.Publisher)
                    .HasMaxLength(255)
                    .IsUnicode(true);

                entity.Property(b => b.Price)
                    .HasColumnType("decimal(10,2)");

                entity.HasIndex(b => b.Title).HasDatabaseName("IDX_Books_Title");

                entity.ToTable("Books");
            });

            modelBuilder.Entity<BookAuthor>(entity =>
            {
                entity.HasKey(ba => new { ba.BookId, ba.AuthorId });

                entity.HasOne(ba => ba.Book)
                    .WithMany(b => b.BookAuthors)
                    .HasForeignKey(ba => ba.BookId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ba => ba.Author)
                    .WithMany(a => a.BookAuthors)
                    .HasForeignKey(ba => ba.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable("BookAuthors");
            });

            modelBuilder.Entity<AuthorHistory>(entity =>
            {
                entity.HasKey(e => e.AuthorId);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(60)
                    .IsUnicode(true);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(60)
                    .IsUnicode(true);

                entity.Property(e => e.PenName)
                    .IsRequired()
                    .HasMaxLength(60)
                    .IsUnicode(true);

                entity.Property(e => e.UpdatedDate)
                    .IsRequired(false);
            });

            modelBuilder.Entity<BookHistory>(entity =>
            {
                entity.HasKey(e => e.BookId);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(true);

                entity.Property(e => e.Publisher)
                    .HasMaxLength(255)
                    .IsUnicode(true);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.UpdatedDate)
                    .IsRequired(false);
            });

            modelBuilder.Entity<BookAuthorHistory>(entity =>
            {
                entity.HasKey(e => new { e.BookId, e.AuthorId });

                entity.Property(e => e.UpdatedDate)
                    .IsRequired(false);
            });

        }
    }
}
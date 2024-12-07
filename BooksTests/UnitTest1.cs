using Lab4Web.Controllers;
using Lab4Web.Data;
using Lab4Web.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BooksTests
{
    public class UnitTest1
    {
        private DbContextOptions<LibraryContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
        private LibraryContext CreateTestContext()
        {
            var options = GetInMemoryOptions();
            var context = new LibraryContext(options);
            context.Database.EnsureCreated();
            return context;
        }


        /// <summary>
        /// Добавление книги, когда всё ок.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddBook_WhenBookIsValid()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);
            var controller = new BooksController(context);
            var newBook = new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                Article = "123ABC",
                YearPublication = 2023,
                Count = 5
            };

            var result = await controller.AddBook(newBook);

            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(BooksController.GetBookById), actionResult.ActionName);

            var createdBook = Assert.IsType<Book>(actionResult.Value);
            Assert.Equal(newBook.Title, createdBook.Title);

            var bookInDb = await context.Books.FirstOrDefaultAsync();
            Assert.NotNull(bookInDb);
            Assert.Equal(newBook.Title, bookInDb.Title);
        }
        /// <summary>
        /// Добавление книги, когда не всё ок.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddBook_WhenBookIsInvalid()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);
            var controller = new BooksController(context);
            var invalidBook = new Book
            {
                Title = "",
                Author = "Test Author",
                Article = "123ABC",
                YearPublication = 2023,
                Count = 5
            };

            var result = await controller.AddBook(invalidBook);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Данные некорректно заполнены.", badRequestResult.Value);
        }


        /// <summary>
        /// Редактирование книги, когда всё ок.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EditBook_WhenBookIsValid()
        {
            var options = GetInMemoryOptions();

            using (var context = new LibraryContext(options))
            {
                context.Books.Add(new Book
                {
                    Id = 1,
                    Title = "bomba",
                    Author = "Старый автор",
                    Article = "boba321",
                    YearPublication = 2000,
                    Count = 3
                });
                await context.SaveChangesAsync();
            }

            using (var context = new LibraryContext(options))
            {
                var controller = new BooksController(context);
                var updateBook = new Book
                {
                    Id = 1,
                    Title = "biba",
                    Author = "roma",
                    Article = "new123",
                    YearPublication = 2001,
                    Count = 5
                };

                var result = await controller.EditBook(1, updateBook);

                Assert.IsType<NoContentResult>(result);

                var book = await context.Books.FindAsync(1);
                Assert.Equal("biba", book.Title);
                Assert.Equal("roma", book.Author);
                Assert.Equal("new123", book.Article);
                Assert.Equal(2001, book.YearPublication);
                Assert.Equal(5, book.Count);
            }
        }
        /// <summary>
        /// Редактирование книги, когда не всё ок.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task EditBook_WhenIsNotValid()
        {
            var options = GetInMemoryOptions();

            using (var context = new LibraryContext(options))
            {
                context.Books.Add(new Book
                {
                    Id = 1,
                    Title = "bomba",
                    Author = "Старый автор",
                    Article = "boba321",
                    YearPublication = 2000,
                    Count = 3
                });
                await context.SaveChangesAsync();
            }
            using (var context = new LibraryContext(options))
            {
                var controller = new BooksController(context);
                var updateBook = new Book
                {
                    Id = 1,
                    Title = "biba",
                    Author = "roma",
                    Article = "new123",
                    YearPublication = 2001,
                    Count = 5
                };

                var result = await controller.EditBook(2, updateBook);
                var book = await context.Books.FindAsync(1);
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("id книги не совпадает", badRequestResult.Value);

            }
        }


        /// <summary>
        /// Удаление книги, когда всё ок!
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteBook_WhenIsValid()
        {
            var options = GetInMemoryOptions();
            using (var context = new LibraryContext(options))
            {
                var controller = new BooksController(context);
                context.Books.Add(new Book
                {
                    Title = "bobma",
                    Author = "Test Author",
                    Article = "123ABC",
                    YearPublication = 2023,
                    Count = 5
                });
                await context.SaveChangesAsync();
            }

            using (var context = new LibraryContext(options))
            {
                var controller = new BooksController(context);

                var result = await controller.DeleteBook(1);

                Assert.IsType<NoContentResult>(result);
                Assert.Null(await context.Readers.FindAsync(1));
            }
        }
        /// <summary>
        /// Удаление книги, если не всё ок.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteBook_WhenIsNotValid()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);

            var controller = new BooksController(context);
            var result = await controller.DeleteBook(1);
            Assert.IsType<NotFoundResult>(result);
        }


        /// <summary>
        /// Получение данных о книге, если всё ок.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDataBookById_WhenIsValid()
        {
            var options = GetInMemoryOptions();
            using var context = new LibraryContext(options);
            var controller = new BooksController(context);

            context.Books.Add(new Book
            {
                Title = "bobma",
                Author = "Test Author",
                Article = "123ABC",
                YearPublication = 2023,
                Count = 5
            });
            await context.SaveChangesAsync();

            var result = await controller.GetBookById(1);
            var actionResult = Assert.IsType<ActionResult<Book>>(result);
            var book = Assert.IsType<Book>(actionResult.Value);
            Assert.Equal("bobma", book.Title);
            Assert.Equal("Test Author", book.Author);
            Assert.Equal("123ABC", book.Article);
            Assert.Equal(2023, book.YearPublication);
            Assert.Equal(5, book.Count);
        }
        /// <summary>
        /// Получение данных о книге если не всё ок.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDataBookById_WhenIsNotValid()
        {
            var options = GetInMemoryOptions();
            using var context = new LibraryContext(options);
            var controller = new BooksController(context);
            var result = await controller.GetBookById(1);
            Assert.IsType<ActionResult<Book>>(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetBorrowedBooks_IsValid()
        {
            var options = GetInMemoryOptions();
            using var context = new LibraryContext(options);

            context.Books.Add(new Book
            {
                Id = 1,
                Title = "Book 1",
                Author = "Author 1",
                Article = "ABC123",
                YearPublication = 2023,
                Count = 1
            });

            context.BorrowedBooks.Add(new BorrowedBook
            {
                ReaderId = 1,
                BookId = 1,
                BorrowedDate = DateTime.Now,
                ReturnDate = null
            });

            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            var result = await controller.GetBorrowedBooks();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Book>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var books = Assert.IsType<List<Book>>(okResult.Value);

            Assert.Single(books);

            var book = books[0];
            Assert.Equal("Book 1", book.Title);
            Assert.Equal("Author 1", book.Author);
        }
        [Fact]
        public async Task GetBorrowedBooks_IsNotValid()
        {
            var options = GetInMemoryOptions();
            using var context = new LibraryContext(options);
            var controller = new BooksController(context);

            var result = await controller.GetBorrowedBooks();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Book>>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal("No borrowed books found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAvailableBooks_IsValid()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);

            context.Books.Add(new Book
            {
                Id = 1,
                Title = "Available Book 1",
                Author = "Author 1",
                Article = "ABC123",
                YearPublication = 2023,
                Count = 2 
            });

            context.Books.Add(new Book
            {
                Id = 2,
                Title = "Borrowed Book 1",
                Author = "Author 2",
                Article = "XYZ789",
                YearPublication = 2022,
                Count = 1 
            });

            context.BorrowedBooks.Add(new BorrowedBook
            {
                ReaderId = 1,
                BookId = 2,
                BorrowedDate = DateTime.Now,
                ReturnDate = null
            });

            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            var result = await controller.GetAvailableBooks();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Book>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var books = Assert.IsType<List<Book>>(okResult.Value);

            Assert.Single(books);

            var book = books[0];
            Assert.Equal("Available Book 1", book.Title);
            Assert.Equal("Author 1", book.Author);
        }
        [Fact]
        public async Task GetAvailableBooks_IsNotValid()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);

            context.Books.Add(new Book
            {
                Id = 1,
                Title = "Borrowed Book",
                Author = "Author 1",
                Article = "ABC123",
                YearPublication = 2023,
                Count = 1
            });

            context.BorrowedBooks.Add(new BorrowedBook
            {
                ReaderId = 1,
                BookId = 1,
                BorrowedDate = DateTime.Now,
                ReturnDate = null 
            });

            context.Books.Add(new Book
            {
                Id = 2,
                Title = "Out of Stock Book",
                Author = "Author 2",
                Article = "XYZ789",
                YearPublication = 2022,
                Count = 0 
            });

            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            var result = await controller.GetAvailableBooks();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Book>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var books = Assert.IsType<List<Book>>(okResult.Value);

            Assert.Empty(books);
        }
        [Fact]
        public async Task SearchBooksByTitle_IsValid()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);

            context.Books.Add(new Book
            {
                Id = 1,
                Title = "C# Programming",
                Author = "Author 1",
                Article = "ABC123",
                YearPublication = 2023,
                Count = 5
            });
            context.Books.Add(new Book
            {
                Id = 2,
                Title = "Java Programming",
                Author = "Author 2",
                Article = "XYZ789",
                YearPublication = 2022,
                Count = 3
            });
            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            var result = await controller.SearchBooksByTitle("Programming");

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Book>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var books = Assert.IsType<List<Book>>(okResult.Value);

            Assert.Equal(2, books.Count); 
            Assert.Contains(books, b => b.Title == "C# Programming");
            Assert.Contains(books, b => b.Title == "Java Programming");
        }
        [Fact]
        public async Task SearchBooksByTitle_IsNotValid()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);
            var controller = new BooksController(context);
            
            var result = await controller.SearchBooksByTitle("");

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Book>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Название книги не может быть пустым.", badRequestResult.Value);
        }
    }
}
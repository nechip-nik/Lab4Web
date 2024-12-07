using Lab4Web.Controllers;
using Lab4Web.Data;
using Lab4Web.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BooksTests
{
    public class UnitTest1
    {
        private DbContextOptions<LibraryContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: "TestLibrary")
                .Options;
        }
        private LibraryContext CreateTestContext()
        {
            var options = GetInMemoryOptions();
            var context = new LibraryContext(options);
            context.Database.EnsureDeleted();
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
        public async Task GetListBooksBorrowed_WhenIsValid()
        {
            var options = GetInMemoryOptions();
            using var context = new LibraryContext(options);
            var controller = new BooksController(context);

            // Arrange
            context.Books.Add(new Book
            {
                Id = 1,
                Title = "Book 1",
                Author = "Author 1",
                Article = "ABC123",
                YearPublication = 2023,
                Count = 1
            });

            context.Readers.Add(new Reader
            {
                Id = 1,
                Name = "John",
                LastName = "Doe",
                MiddleName = "A",
                DayOfBirthday = new DateTime(1990, 1, 1)
            });

            context.BorrowedBooks.Add(new BorrowedBook
            {
                ReaderId = 1,
                BookId = 1,
                BorrowedDate = DateTime.Now,
                ReturnDate = null
            });

            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetBorrowedBooks();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<object>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var borrowedBooks = Assert.IsType<List<object>>(okResult.Value);

            Assert.Single(borrowedBooks); // Проверяем, что в списке одна запись

            // Проверяем свойства первой записи
            var borrowedBook = borrowedBooks[0];
            Assert.Contains("Book", borrowedBook.ToString());
            Assert.Contains("Reader", borrowedBook.ToString());
            Assert.Contains("BorrowedBook", borrowedBook.ToString());
        }



    }
}
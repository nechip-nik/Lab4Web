using Lab4Web.Controllers;
using Lab4Web.Data;
using Lab4Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace TestProject1
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
        /// Проверка на удаление читателя. Если читатель существует.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteReaderById()
        {
            var options = GetInMemoryOptions();

            using (var context = new LibraryContext(options))
            {
                context.Readers.Add(new Reader
                {
                    Name = "Иван",
                    LastName = "Иванов",
                    MiddleName = "Иванович",
                    DayOfBirthday = new DateTime(2003, 11, 19)
                });
                await context.SaveChangesAsync();
            }

            using (var context = new LibraryContext(options))
            {
                var controller = new ReadersController(context);

                var result = await controller.DeleteById(1);

                Assert.IsType<NoContentResult>(result);
                Assert.Null(await context.Readers.FindAsync(1)); 
            }
        }

        /// <summary>
        /// Проверка на удаление читателя. Если читателя не существует.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteReaderById_ifReaderNull()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);
            
            var controller = new ReadersController(context);
            var result = await controller.DeleteById(11);
            Assert.IsType<NotFoundResult>(result);
           
        }

        [Fact]
        public async Task AddReader_IsValid()
        {
            var options = GetInMemoryOptions();
            using var context = new LibraryContext(options);

            var controller = new ReadersController(context);

            var newReader = new Reader
            {
                Name = "Роман",
                LastName = "Корнеев",
                MiddleName = "Александрович",
                DayOfBirthday = new DateTime(2003, 11, 19)
            };

            var result = await controller.AddReader(newReader);

            var actionResult = Assert.IsType<ActionResult<Reader>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var addedReader = Assert.IsType<Reader>(createdAtActionResult.Value);

            Assert.Equal("Роман", addedReader.Name);
            Assert.Equal("Корнеев", addedReader.LastName);
            Assert.Equal("Александрович", addedReader.MiddleName);
            Assert.Equal(new DateTime(2003, 11, 19), addedReader.DayOfBirthday);

            var savedReader = await context.Readers.FirstOrDefaultAsync(r => r.Id == addedReader.Id);
            Assert.NotNull(savedReader);
            Assert.Equal("Роман", savedReader.Name);
            Assert.Equal("Корнеев", savedReader.LastName);
            Assert.Equal("Александрович", savedReader.MiddleName);
            Assert.Equal(new DateTime(2003, 11, 19), savedReader.DayOfBirthday);
        }
        [Fact]
        public async Task AddReader_ReturnsBadRequest_WhenNameIsEmpty()
        {
            var options = GetInMemoryOptions();
            using var context = new LibraryContext(options);

            var controller = new ReadersController(context);

            var newReader = new Reader
            {
                Name = "", 
                LastName = "Корнеев",
                MiddleName = "Александрович",
                DayOfBirthday = new DateTime(2003, 11, 19)
            };

            var result = await controller.AddReader(newReader);

            var actionResult = Assert.IsType<ActionResult<Reader>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            Assert.Equal("Данные некорректны", badRequestResult.Value);

            var savedReader = await context.Readers.FirstOrDefaultAsync(r => r.LastName == "Корнеев");
            Assert.Null(savedReader);
        }
        [Fact]
        public async Task EditById_ValidId_ReturnsNoContent()
        {
            using var context = CreateTestContext();
            var reader = new Reader
            {
                Id = 1,
                Name = "John",
                LastName = "Doe", 
                MiddleName = "Smith"
            };
            context.Readers.Add(reader);
            context.SaveChanges();

            var updatedReader = new Reader
            {
                Id = 1,
                Name = "John Updated",
                LastName = "Doe", 
                MiddleName = "Smith" 
            };

            var controller = new ReadersController(context);

            var result = await controller.EditById(1, updatedReader);

            Assert.IsType<NoContentResult>(result.Result);
        }


        [Fact]
        public async Task EditById_InvalidId_ReturnsBadRequest()
        {
            using var context = CreateTestContext();
            var reader = new Reader
            {
                Id = 1,
                Name = "John",
                LastName = "Doe",
                MiddleName = "Smith" 
            };
            context.Readers.Add(reader);
            context.SaveChanges();

            var controller = new ReadersController(context);

            var result = await controller.EditById(2, reader);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("id не совпадают", badRequestResult.Value);
        }

        [Fact]
        public async Task BorrowBook_ShouldReturnOk_WhenBookIsSuccessfullyBorrowed()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);
            context.Readers.Add(new Reader { Name = "Роман", LastName = "Корнеев", MiddleName = "Александрович", DayOfBirthday = new DateTime(1995, 5, 15) });
            context.Books.Add(new Book { Title = "C# для начинающих", Count = 1, Article = "ABC123", Author = "billiboba", YearPublication = 1999 });
            await context.SaveChangesAsync();

            var controller = new ReadersController(context);

            var result = await controller.BorrowBook(1, 1);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("книга выдана читателю", actionResult.Value);
        }
        [Fact]
        public async Task BorrowBook_ValidData_ReturnsOk()
        {
            using var context = CreateTestContext();

            var reader = new Reader
            {
                Id = 1,
                Name = "John",
                LastName = "Doe", 
                MiddleName = "Smith"
            };

            var book = new Book
            {
                Id = 1,
                Title = "C# Programming",
                Article = "123-ABC", 
                Author = "Author Name",
                Count = 1
            };

            context.Readers.Add(reader);
            context.Books.Add(book);
            context.SaveChanges();

            var controller = new ReadersController(context);

            var result = await controller.BorrowBook(1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("книга выдана читателю", okResult.Value);
        }

        [Fact]
        public async Task BorrowBook_NoAvailableBooks_ReturnsBadRequest()
        {
            using var context = CreateTestContext();

            var reader = new Reader
            {
                Id = 1,
                Name = "John",
                LastName = "Doe", 
                MiddleName = "Smith"
            };

            var book = new Book
            {
                Id = 1,
                Title = "C# Programming",
                Article = "123-ABC", 
                Author = "Author Name", 
                Count = 0 
            };

            context.Readers.Add(reader);
            context.Books.Add(book);
            context.SaveChanges();

            var controller = new ReadersController(context);

            var result = await controller.BorrowBook(1, 1);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Нет доступных книг", badRequestResult.Value);
        }
        [Fact]
        public async Task ReturnBook_ValidData_ReturnsOk()
        {
            // Arrange
            using var context = CreateTestContext();

            var reader = new Reader
            {
                Id = 1,
                Name = "John",
                LastName = "Doe", // Обязательное поле
                MiddleName = "Smith" // Обязательное поле
            };

            var book = new Book
            {
                Id = 1,
                Title = "C# Programming",
                Article = "123-ABC", // Обязательное поле
                Author = "Author Name", // Обязательное поле
                Count = 0 // Количество доступных книг
            };

            var borrowedBook = new BorrowedBook
            {
                ReaderId = 1,
                BookId = 1,
                BorrowedDate = DateTime.Now,
                ReturnDate = null
            };

            context.Readers.Add(reader);
            context.Books.Add(book);
            context.BorrowedBooks.Add(borrowedBook);
            context.SaveChanges();

            var controller = new ReadersController(context);

            // Act
            var result = await controller.ReturnBook(1, 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Книга добавлена обратно в библиотеку", okResult.Value);
        }


        [Fact]
        public async Task ReturnBook_NoBorrowedBook_ReturnsNotFound()
        {
            // Arrange
            using var context = CreateTestContext();

            var reader = new Reader
            {
                Id = 1,
                Name = "John",
                LastName = "Doe", // Обязательное поле
                MiddleName = "Smith" // Обязательное поле
            };

            var book = new Book
            {
                Id = 1,
                Title = "C# Programming",
                Article = "123-ABC", // Обязательное поле
                Author = "Author Name", // Обязательное поле
                Count = 1
            };

            context.Readers.Add(reader);
            context.Books.Add(book);
            context.SaveChanges();

            var controller = new ReadersController(context);

            // Act
            var result = await controller.ReturnBook(1, 1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Книга не была выдана или уже возвращена", notFoundResult.Value);
        }





    }
}
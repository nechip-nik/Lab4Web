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
        /// �������� �� �������� ��������. ���� �������� ����������.
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
                    Name = "����",
                    LastName = "������",
                    MiddleName = "��������",
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
        /// �������� �� �������� ��������. ���� �������� �� ����������.
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
                Name = "�����",
                LastName = "�������",
                MiddleName = "�������������",
                DayOfBirthday = new DateTime(2003, 11, 19)
            };

            var result = await controller.AddReader(newReader);

            var actionResult = Assert.IsType<ActionResult<Reader>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var addedReader = Assert.IsType<Reader>(createdAtActionResult.Value);

            Assert.Equal("�����", addedReader.Name);
            Assert.Equal("�������", addedReader.LastName);
            Assert.Equal("�������������", addedReader.MiddleName);
            Assert.Equal(new DateTime(2003, 11, 19), addedReader.DayOfBirthday);

            var savedReader = await context.Readers.FirstOrDefaultAsync(r => r.Id == addedReader.Id);
            Assert.NotNull(savedReader);
            Assert.Equal("�����", savedReader.Name);
            Assert.Equal("�������", savedReader.LastName);
            Assert.Equal("�������������", savedReader.MiddleName);
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
                LastName = "�������",
                MiddleName = "�������������",
                DayOfBirthday = new DateTime(2003, 11, 19)
            };

            var result = await controller.AddReader(newReader);

            var actionResult = Assert.IsType<ActionResult<Reader>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            Assert.Equal("������ �����������", badRequestResult.Value);

            var savedReader = await context.Readers.FirstOrDefaultAsync(r => r.LastName == "�������");
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
            Assert.Equal("id �� ���������", badRequestResult.Value);
        }

        [Fact]
        public async Task BorrowBook_ShouldReturnOk_WhenBookIsSuccessfullyBorrowed()
        {
            var options = GetInMemoryOptions();

            using var context = new LibraryContext(options);
            context.Readers.Add(new Reader { Name = "�����", LastName = "�������", MiddleName = "�������������", DayOfBirthday = new DateTime(1995, 5, 15) });
            context.Books.Add(new Book { Title = "C# ��� ����������", Count = 1, Article = "ABC123", Author = "billiboba", YearPublication = 1999 });
            await context.SaveChangesAsync();

            var controller = new ReadersController(context);

            var result = await controller.BorrowBook(1, 1);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("����� ������ ��������", actionResult.Value);
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
            Assert.Equal("����� ������ ��������", okResult.Value);
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
            Assert.Equal("��� ��������� ����", badRequestResult.Value);
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
                LastName = "Doe", // ������������ ����
                MiddleName = "Smith" // ������������ ����
            };

            var book = new Book
            {
                Id = 1,
                Title = "C# Programming",
                Article = "123-ABC", // ������������ ����
                Author = "Author Name", // ������������ ����
                Count = 0 // ���������� ��������� ����
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
            Assert.Equal("����� ��������� ������� � ����������", okResult.Value);
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
                LastName = "Doe", // ������������ ����
                MiddleName = "Smith" // ������������ ����
            };

            var book = new Book
            {
                Id = 1,
                Title = "C# Programming",
                Article = "123-ABC", // ������������ ����
                Author = "Author Name", // ������������ ����
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
            Assert.Equal("����� �� ���� ������ ��� ��� ����������", notFoundResult.Value);
        }





    }
}
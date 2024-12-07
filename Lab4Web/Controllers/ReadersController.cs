using Lab4Web.Data;
using Lab4Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab4Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadersController : ControllerBase
    {
        private LibraryContext _libraryContext;

        public ReadersController(LibraryContext libraryContext)
        {
            _libraryContext = libraryContext;
        }
        [HttpPost]
        public async Task<ActionResult<Reader>> AddReader(Reader reader)
        {
            if (reader == null || string.IsNullOrWhiteSpace(reader.LastName) || string.IsNullOrWhiteSpace(reader.Name) || string.IsNullOrWhiteSpace(reader.MiddleName)
                || reader.DayOfBirthday == default)
            {
                return BadRequest("Данные некорректны");
            }
            _libraryContext.Readers.Add(reader);
            await _libraryContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReaderById), new { id = reader.Id }, reader);
        }

        [HttpGet("id")]
        public async Task<ActionResult<Reader>> GetReaderById(int id)
        {
            var reader = await _libraryContext.Readers.FindAsync(id);
            if (reader == null)
            {
                return NotFound();
            }
            return reader;
        }

        [HttpPut("id")]
        public async Task<ActionResult<Reader>> EditById(int id, Reader updateReader)
        {
            if (id != updateReader.Id)
            {
                return BadRequest("id не совпадают");
            }
            var reader = _libraryContext.Readers.Find(id);
            if (reader == null)
            {
                return NotFound();
            }
            await _libraryContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<ActionResult> DeleteById(int id)
        {
            var reader = await _libraryContext.Readers.FindAsync(id);
            if (reader == null)
            {
                return NotFound();
            }
            _libraryContext.Readers.Remove(reader);
            await _libraryContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{readerId}/borrow/{bookId}")]
        public async Task<IActionResult> BorrowBook(int readerId, int bookId)
        {
            var reader = await _libraryContext.Readers.FindAsync(readerId);
            if (reader == null)
            {
                return NotFound("Не найден читатель");
            }
            var book = await _libraryContext.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound("Не найдена книга");
            }
            if (book.Count <= 0)
            {
                return BadRequest("Нет доступных книг");
            }
            var borrowedBook = new BorrowedBook
            {
                ReaderId = readerId,
                BookId = bookId,
                BorrowedDate = DateTime.Now,
                ReturnDate = null
            };
            _libraryContext.BorrowedBooks.Add(borrowedBook);
            await _libraryContext.SaveChangesAsync();
            return Ok("книга выдана читателю");
        }

        [HttpPost("{readerId}/return/{bookId}")]
        public async Task<IActionResult> ReturnBook(int readerId, int bookId)
        {
            var borrowedBook = await _libraryContext.BorrowedBooks.FirstOrDefaultAsync(bb => bb.ReaderId == readerId && bb.BookId == bookId && bb.ReturnDate == null);
            if (borrowedBook == null)
            {
                return NotFound("Книга не была выдана или уже возвращена");
            }
            var book = await _libraryContext.Books.FindAsync(bookId);

            if(book == null)
            {
                return NotFound("Книга не найдена");
            }
            borrowedBook.ReturnDate = DateTime.Now;
            book.Count++;
            await _libraryContext.SaveChangesAsync();
            return Ok("Книга добавлена обратно в библиотеку");
        }
    }
}

using Lab4Web.Data;
using Lab4Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab4Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private LibraryContext _libraryContext;

        public BooksController(LibraryContext libraryContext)
        {
            _libraryContext = libraryContext;
        }

        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            if (book == null || string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author) || string.IsNullOrWhiteSpace(book.Article)
                || book.YearPublication <= 0 || book.Count <= 0)
            {
                return BadRequest("Данные некорректно заполнены.");
            }
            _libraryContext.Books.Add(book);
            await _libraryContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [HttpPut("id")]
        public async Task<IActionResult> EditBook(int id, Book updateBook)
        {
            if (id != updateBook.Id)
            {
                return BadRequest("id книги не совпадает");
            }
            var book = await _libraryContext.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            book.Title = updateBook.Title;
            book.Author = updateBook.Author;
            book.Article = updateBook.Article;
            book.YearPublication = updateBook.YearPublication;
            book.Count = updateBook.Count;

            await _libraryContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var book = await _libraryContext.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            _libraryContext.Books.Remove(book);
            await _libraryContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await _libraryContext.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpGet("borrowed")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBorrowedBooks()
        {
            var borrowedBooks = await _libraryContext.BorrowedBooks
                .Where(bb => bb.ReturnDate == null)
                .Select(bb => bb.Book)
                .ToListAsync();
            if (!borrowedBooks.Any())
            {
                return NotFound("No borrowed books found.");
            }
            return Ok(borrowedBooks);
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Book>>> GetAvailableBooks()
        {
            var availableBooks = await _libraryContext.Books
                .Where(b => b.Count > 0 && !_libraryContext.BorrowedBooks.Any(bb => bb.BookId == b.Id && bb.ReturnDate == null))
                .ToListAsync();
            return Ok(availableBooks);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooksByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Название книги не может быть пустым.");
            }

            var books = await _libraryContext.Books
                .Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            return books.Any() ? Ok(books) : NotFound("Книги не найдены.");
        }

        [HttpGet("published-before")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksPublishedBeforeYear(int year)
        {
            if (year <= 0)
            {
                return BadRequest("Год должен быть положительным числом.");
            }

            var books = await _libraryContext.Books
                .Where(b => b.YearPublication < year)
                .ToListAsync();

            return books.Any() ? Ok(books) : NotFound("Книги не найдены.");
        }
    }
}
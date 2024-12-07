namespace Lab4Web.Models
{
    public class BorrowedBook
    {
        public int ReaderId { get; set; }
        public Reader Reader { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnDate { get; set; } 
    }
}

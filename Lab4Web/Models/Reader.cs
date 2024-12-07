namespace Lab4Web.Models
{
    public class Reader
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string MiddleName { get; set; }
        public DateTime DayOfBirthday { get; set; }
        public ICollection<BorrowedBook> BorrowedBooks { get; set; }

    }
}

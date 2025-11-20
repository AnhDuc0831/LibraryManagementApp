using System;

namespace LibraryManageApp.ViewModels
{
    public class BookResult
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int? PublishYear { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }

        // New fields
        public string CoverImagePath { get; set; }
        public bool IsAvailable { get; set; }
        public int AvailableCount { get; set; }
    }

    public class BorrowHistoryItem
    {
        public DateTime BorrowDate { get; set; }
        public string ReturnDate { get; set; }
        public string Status { get; set; }
        public string BookTitle { get; set; }
        public int InstanceId { get; set; }
        public string LibrarianName { get; set; }
    }
}
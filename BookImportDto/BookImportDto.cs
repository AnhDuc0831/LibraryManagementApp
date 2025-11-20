namespace DTO
{
    public class BookImportDto
    {
        public string Title { get; set; }
        public string AuthorName { get; set; } // Quan trọng: Tên tác giả (chữ)
        public string AuthorCountry { get; set; }
        public string Category { get; set; }
        public int PublishYear { get; set; }
        public int Price { get; set; }
        public string Description { get; set; } // Có thể lưu vào cột ghi chú nếu muốn
    }
}

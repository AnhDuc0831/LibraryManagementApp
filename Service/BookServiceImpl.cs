using BussinessObject;
using DTO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface BookService
    {
        Task<List<Book>> GetBooksAsync();
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(int id);
        Task<List<BookInstance>> GetByBookIdAsync(int bookId);
        Task SaveInstancesAsync(int bookId, List<BookInstance> list);
        void ImportBooksToDatabase(List<BookImportDto> booksFromAI);
    }

    public class BookServiceImpl : BookService
    {
        private static string ReadConnectionString()

        {

            var cfg = new ConfigurationBuilder()

                .SetBasePath(AppContext.BaseDirectory)

                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)

                .Build();



            return cfg.GetConnectionString("DBLibraryManagement");

        }
        private readonly BookRepository _repo;

        public BookServiceImpl()
        {
            _repo = new BookRepositoryImpl();
        }

        public Task<List<Book>> GetBooksAsync() => _repo.GetAllAsync();

        public Task AddBookAsync(Book book) => _repo.AddAsync(book);

        public Task UpdateBookAsync(Book book) => _repo.UpdateAsync(book);

        public Task DeleteBookAsync(int id) => _repo.DeleteAsync(id);

        public Task<List<BookInstance>> GetByBookIdAsync(int bookId)
        => _repo.GetByBookIdAsync(bookId);

        public Task SaveInstancesAsync(int bookId, List<BookInstance> list)
            => _repo.SaveInstancesAsync(bookId, list);

        // Hàm này nhận list sách từ AI và lưu vào DB an toàn
        public void ImportBooksToDatabase(List<BookImportDto> booksFromAI)
        {
            using (var conn = new SqlConnection(ReadConnectionString()))
            {
                conn.Open();

                foreach (var book in booksFromAI)
                {
                    // BƯỚC 1: XỬ LÝ TÁC GIẢ (AUTHOR)
                    int authorId = 0;

                    // Kiểm tra tác giả đã tồn tại chưa?
                    string checkAuthorSql = "SELECT AuthorID FROM Author WHERE FullName = @Name";
                    using (var cmdCheck = new SqlCommand(checkAuthorSql, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@Name", book.AuthorName);
                        var result = cmdCheck.ExecuteScalar();

                        if (result != null)
                        {
                            // Đã có -> Lấy ID
                            authorId = (int)result;
                        }
                        else
                        {
                            // Chưa có -> Insert mới và lấy ID vừa tạo
                            string insertAuthorSql = @"
                        INSERT INTO Author (FullName, Country) 
                        OUTPUT INSERTED.AuthorID 
                        VALUES (@Name, @Country)";

                            using (var cmdInsertAuthor = new SqlCommand(insertAuthorSql, conn))
                            {
                                cmdInsertAuthor.Parameters.AddWithValue("@Name", book.AuthorName);
                                cmdInsertAuthor.Parameters.AddWithValue("@Country", book.AuthorCountry ?? "Unknown");
                                authorId = (int)cmdInsertAuthor.ExecuteScalar();
                            }
                        }
                    }

                    // BƯỚC 2: INSERT SÁCH (BOOK) VỚI AUTHOR_ID VỪA CÓ
                    string insertBookSql = @"
                INSERT INTO Book (Title, AuthorID, PublishYear, Quantity, Price, Category)
                VALUES (@Title, @AuthorID, @Year, 0, @Price, @Category)";
                    // Mặc định nhập về 10 cuốn

                    using (var cmdBook = new SqlCommand(insertBookSql, conn))
                    {
                        cmdBook.Parameters.AddWithValue("@Title", book.Title);
                        cmdBook.Parameters.AddWithValue("@AuthorID", authorId); // Dùng ID xịn
                        cmdBook.Parameters.AddWithValue("@Year", book.PublishYear);
                        cmdBook.Parameters.AddWithValue("@Price", book.Price);
                        cmdBook.Parameters.AddWithValue("@Category", book.Category);

                        cmdBook.ExecuteNonQuery();
                    }

                    // BƯỚC 3: (Tùy chọn) Tạo BookInstance cho từng cuốn nếu logic của bạn cần...
                }
            }
        }
    }

}

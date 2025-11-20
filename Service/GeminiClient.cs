using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service
{
    public class GeminiClient
    {
        private readonly HttpClient httpClient;
        // LƯU Ý: Đây là API Key bạn đã cung cấp. 
        private readonly string apiKey = "AIzaSyDf7_Nt-KmhZVzSFDQ1w_jTjWf2lfGg1e8";

        private readonly string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent";

        public GeminiClient()
        {
            httpClient = new HttpClient();
        }
        public enum UserRole
        {
            Librarian, // Thủ thư
            Student    // Học sinh
        }

        public async Task<string> GetSqlQueryAsync(string naturalLanguageQuery)
        {
            // Schema cơ sở dữ liệu
            string schema = @"
                Author(AuthorID, FullName, Country);
                Book(BookID, Title, AuthorID, PublishYear, Quantity, Price, Category);
                BookInstance(InstanceID, BookID, Condition);
                Student(StudentID, Code, FullName, Class, Password, Email);
                Librarian(LibrarianID, Code, Name, Password);
                Borrow(BorrowID, StudentID, BorrowDate, ReturnDate, LibrarianID, Status);
                BorrowDetail(BorrowDetailID, BorrowID, InstanceID);
                BorrowFine(FineID, BorrowDetailID, Amount, Reason);
            ";

            // CẬP NHẬT PROMPT: Thêm yêu cầu số 3 về tiền tố N cho Unicode
            string fullPrompt = $@"
Bạn là một chuyên gia SQL Server. Nhiệm vụ của bạn là chuyển đổi câu hỏi ngôn ngữ tự nhiên thành câu lệnh SQL SELECT dành cho hệ thống Quản lý Thư viện.

Database: DBLibraryManagement

Danh sách bảng & cột chính (Schema):
{schema}

QUY TẮC & RÀNG BUỘC BỔ SUNG (RẤT QUAN TRỌNG):
1. Bảng Author:
   - FullName, Country: NVARCHAR, có thể chứa Unicode.

2. Bảng Book:
   - Liên kết AuthorID → Author.AuthorID.
   - Category là NVARCHAR, chứa các giá trị hợp lệ:
     + N'Văn học'
     + N'Khoa học'
     + N'Lịch sử'
     + N'Triết học'
     + N'Nghệ thuật'
     + N'Tôn giáo'

3. Bảng BookInstance:
   - BookID → Book.BookID.
   - Condition là NVARCHAR(20).

4. Bảng Student:
   - FullName, Email: NVARCHAR.
   - Code là duy nhất.

5. Bảng Librarian:
   - Name: NVARCHAR.
   - Code là VARCHAR.

6. Bảng Borrow:
   - StudentID → Student.StudentID
   - LibrarianID → Librarian.LibrarianID
   - BorrowDate, ReturnDate: kiểu DATE
   - Status có giá trị:
     + 'Late'
     + 'Returned'
     + 'Borrowing'
   - ReturnDate có thể NULL (NULL nghĩa là sách chưa trả).

7. Bảng BorrowDetail:
   - BorrowID → Borrow.BorrowID
   - InstanceID → BookInstance.InstanceID

8. Bảng BorrowFine:
   - BorrowDetailID → BorrowDetail.BorrowDetailID
   - Amount: INT
   - Reason: NVARCHAR

YÊU CẦU NGHIÊM NGẶT:
1. CHỈ trả về câu lệnh SQL, không giải thích, không dùng markdown.
2. CHỈ được dùng SELECT.
3. Mọi chuỗi (string literal) BẮT BUỘC có tiền tố N.
   Ví dụ: FullName = N'Nguyễn Văn A'
4. Khi JOIN phải đúng khóa ngoại thực tế:
   - Book.AuthorID → Author.AuthorID
   - BookInstance.BookID → Book.BookID
   - Borrow.StudentID → Student.StudentID
   - Borrow.LibrarianID → Librarian.LibrarianID
   - BorrowDetail.BorrowID → Borrow.BorrowID
   - BorrowDetail.InstanceID → BookInstance.InstanceID
   - BorrowFine.BorrowDetailID → BorrowDetail.BorrowDetailID
5. Khi lọc ngày, sử dụng format chuẩn SQL 'YYYY-MM-DD'.

Câu hỏi: {naturalLanguageQuery}
SQL:";



            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = fullPrompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gửi request kèm key trên URL
            string urlWithKey = $"{apiEndpoint}?key={apiKey}";

            var response = await httpClient.PostAsync(urlWithKey, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(responseJson);

            try
            {
                var sqlResult = document.RootElement
                                    .GetProperty("candidates")[0]
                                    .GetProperty("content")
                                    .GetProperty("parts")[0]
                                    .GetProperty("text")
                                    .GetString();

                // Làm sạch chuỗi
                sqlResult = sqlResult.Replace("```sql", "").Replace("```", "").Trim();

                return sqlResult;
            }
            catch (Exception)
            {
                throw new Exception("Không thể đọc SQL từ phản hồi của Gemini. Có thể do Safety Filter.");
            }
        }
    }
}
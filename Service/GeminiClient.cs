using DTO;
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
        //private readonly string apiKey = "AIzaSyDf7_Nt-KmhZVzSFDQ1w_jTjWf2lfGg1e8";
        private readonly string apiKey = "AIzaSyAuJmne4GGbbreuPNkxuRUWa9kOqCc92vg";
        

        private readonly string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent";

        public GeminiClient(int role)
        {
            httpClient = new HttpClient();
        }
        public enum UserRole
        {
            Librarian, // Thủ thư
            Student    // Học sinh
        }

        public async Task<string> GetSqlQueryAsync(string naturalLanguageQuery, int role, string currentUserId = null)
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

            string securityInstruction = "";

            if (role == 1)
            {
                securityInstruction = @"
        ROLE: LIBRARIAN (ADMIN).
        - Có quyền truy cập TOÀN BỘ dữ liệu.
        - Có thể tìm kiếm thông tin của bất kỳ học sinh nào.
        ";
            }
            else if (role == 2)
            {
                // Rất quan trọng: Ép buộc AI phải thêm WHERE StudentID = ...
                if (string.IsNullOrEmpty(currentUserId)) throw new Exception("Student Role requires a generic UserID");

                securityInstruction = $@"
        ROLE: STUDENT (RESTRICTED).
        ID CỦA USER HIỆN TẠI: '{currentUserId}'
        
        QUY TẮC BẢO MẬT NGHIÊM NGẶT CHO STUDENT:
        1. Bảng được phép xem chung: Author, Book, BookInstance (để tra cứu sách).
        2. Bảng dữ liệu cá nhân: Borrow, BorrowDetail, BorrowFine, Student.
        3. BẮT BUỘC: Mọi truy vấn liên quan đến bảng dữ liệu cá nhân (Borrow, Student...) PHẢI có điều kiện: WHERE Student.StudentID = '{currentUserId}' hoặc Borrow.StudentID = '{currentUserId}'.
        4. CẤM: Không được phép SELECT dữ liệu của Student khác.
        5. CẤM: Không được phép truy cập bảng Librarian.
        6. Nếu người dùng hỏi về 'sách đã mượn', 'tiền phạt', 'thông tin cá nhân', hãy tự động thêm bộ lọc ID của họ.
        ";
            }

            // CẬP NHẬT PROMPT: Thêm yêu cầu số 3 về tiền tố N cho Unicode
            string fullPrompt = $@"
Bạn là một chuyên gia SQL Server. Nhiệm vụ của bạn là chuyển đổi câu hỏi ngôn ngữ tự nhiên thành câu lệnh SQL SELECT dành cho hệ thống Quản lý Thư viện.

Database: DBLibraryManagement

Danh sách bảng & cột chính (Schema):
{schema}

{securityInstruction}

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



        public async Task<List<BookImportDto>> ResearchBooksAsync(string topic)
        {
            // Prompt kỹ thuật bắt buộc trả về JSON thuần
            string prompt = $@"
    Bạn là trợ lý nhập liệu thư viện. Hãy tìm kiếm/tra cứu thông tin về: {topic}.
    
    YÊU CẦU OUTPUT:
    1. Trả về danh sách kết quả dưới dạng JSON Array.
    2. KHÔNG viết thêm lời dẫn, KHÔNG dùng markdown (```json). Chỉ trả về raw text JSON.
    3. Cấu trúc JSON cho mỗi object:
       - Title (string): Tên sách (Tiếng Việt nếu có thể)
       - AuthorName (string): Tên tác giả đầy đủ
       - AuthorCountry (string): Quốc tịch tác giả (đoán)
       - Category (string): Chỉ chọn 1 trong các loại: 'Văn học', 'Khoa học', 'Lịch sử', 'Triết học', 'Nghệ thuật', 'Tôn giáo'.
       - PublishYear (int): Năm xuất bản
       - Price (int): Ước lượng giá sách (đơn vị VNĐ, ví dụ 100000)

    Ví dụ output mong muốn:
    [
      {{ ""Title"": ""Dế Mèn Phiêu Lưu Ký"", ""AuthorName"": ""Tô Hoài"", ""Category"": ""Văn học"", ... }}
    ]
    ";

            // Gọi hàm gửi request (tái sử dụng logic cũ)
            // Lưu ý: Bạn cần chỉnh lại hàm SendRequestToGemini để trả về string raw
            string jsonResult = await SendRequestToGeminiRaw(prompt);

            try
            {
                // Xử lý sạch chuỗi json (đề phòng AI vẫn thêm markdown)
                jsonResult = jsonResult.Replace("```json", "").Replace("```", "").Trim();

                // Parse JSON thành List Object C#
                var books = JsonSerializer.Deserialize<List<BookImportDto>>(jsonResult);
                return books;
            }
            catch
            {
                return new List<BookImportDto>(); // Trả về rỗng nếu lỗi
            }
        }
        private async Task<string> SendRequestToGeminiRaw(string prompt)
        {
            // 1. Cấu trúc Body theo chuẩn của Google Gemini API
            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        },
                // (Tùy chọn) Bạn có thể thêm config để giảm độ sáng tạo nếu cần chính xác cao
                generationConfig = new
                {
                    temperature = 0.2, // Thấp để AI trả lời ổn định, ít bịa
                    maxOutputTokens = 2000
                }
            };

            // 2. Serialize body thành chuỗi JSON
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // 3. Gửi Request (POST)
            string urlWithKey = $"{apiEndpoint}?key={apiKey}";
            var response = await httpClient.PostAsync(urlWithKey, httpContent);

            // 4. Kiểm tra lỗi HTTP (Ví dụ: 400, 401, 500)
            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi gọi API Gemini ({response.StatusCode}): {errorDetails}");
            }

            // 5. Đọc phản hồi
            var responseString = await response.Content.ReadAsStringAsync();

            // 6. Parse JSON để lấy đúng phần text trả lời
            using (var document = JsonDocument.Parse(responseString))
            {
                try
                {
                    // Cấu trúc JSON trả về: candidates[0] -> content -> parts[0] -> text
                    if (document.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var text = candidates[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        return text; // Trả về nội dung thô
                    }
                    else
                    {
                        // Trường hợp AI từ chối trả lời (Safety Filter) hoặc không có candidate nào
                        // Thường xảy ra nếu prompt vi phạm chính sách
                        return "ERROR: AI không trả về kết quả (Có thể bị chặn bởi Safety Filter).";
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi phân tích JSON từ Gemini: {ex.Message}");
                }
            }
        }
    }

}
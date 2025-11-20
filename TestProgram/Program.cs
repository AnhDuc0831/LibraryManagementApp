//using DataAccsess;
//using Service;
//using System.Data;

//namespace TestProgram
//{
//    public class Program
//    {
//        // Khởi tạo các Service
//        private static readonly GeminiClient geminiClient = new GeminiClient();
//        private static readonly DatabaseService dbService = new DatabaseService();

//        static async Task Main(string[] args)
//        {
//            Console.OutputEncoding = System.Text.Encoding.UTF8;
//            Console.WriteLine("=================================================");
//            Console.WriteLine("       DEMO: NL to SQL Query Execution");
//            Console.WriteLine("=================================================");

//            while (true)
//            {
//                Console.Write("\n[NL Query] Nhập câu truy vấn (hoặc 'exit' để thoát): ");
//                string naturalLanguageQuery = Console.ReadLine();

//                if (naturalLanguageQuery?.Trim().ToLower() == "exit")
//                {
//                    break;
//                }
//                if (string.IsNullOrWhiteSpace(naturalLanguageQuery))
//                {
//                    continue;
//                }

//                try
//                {
//                    Console.WriteLine("-------------------------------------------------");
//                    Console.WriteLine("1. Đang gọi Gemini API để chuyển đổi...");

//                    // 1. Gọi Gemini API
//                    //string sqlQuery = await geminiClient.GetSqlQueryAsync(naturalLanguageQuery);

//                    Console.WriteLine($"[SQL Query]: {sqlQuery.Trim()}");
//                    Console.WriteLine("-------------------------------------------------");

//                    Console.WriteLine("2. Đang thực thi truy vấn SQL trong cơ sở dữ liệu...");

//                    // 2. Thực thi SQL
//                    DataTable results = await dbService.ExecuteQueryAsync(sqlQuery);

//                    Console.WriteLine("3. Kết quả:");
//                    DisplayDataTable(results);
//                }
//                catch (System.Net.Http.HttpRequestException httpEx)
//                {
//                    Console.ForegroundColor = ConsoleColor.Red;
//                    Console.WriteLine($"LỖI API: Vui lòng kiểm tra API Key hoặc Endpoint.\nChi tiết: {httpEx.Message}");
//                    Console.ResetColor();
//                }
//                catch (Microsoft.Data.SqlClient.SqlException sqlEx)
//                {
//                    Console.ForegroundColor = ConsoleColor.Red;
//                    Console.WriteLine($"LỖI DB: Lỗi thực thi SQL hoặc kết nối cơ sở dữ liệu.\nChi tiết: {sqlEx.Message}");
//                    Console.ResetColor();
//                }
//                catch (System.Exception ex)
//                {
//                    Console.ForegroundColor = ConsoleColor.Red;
//                    Console.WriteLine($"LỖI HỆ THỐNG: {ex.Message}");
//                    Console.ResetColor();
//                }
//                finally
//                {
//                    Console.WriteLine("=================================================");
//                }
//            }
//        }

//        // Hàm hỗ trợ hiển thị DataTable trong Console
//        private static void DisplayDataTable(DataTable table)
//        {
//            if (table == null || table.Rows.Count == 0)
//            {
//                Console.ForegroundColor = ConsoleColor.Yellow;
//                Console.WriteLine("=> [KHÔNG CÓ DỮ LIỆU] Không tìm thấy kết quả nào.");
//                Console.ResetColor();
//                return;
//            }

//            Console.WriteLine($"=> Tìm thấy {table.Rows.Count} hàng:");

//            // In tiêu đề cột
//            Console.ForegroundColor = ConsoleColor.Cyan;
//            string header = "";
//            foreach (DataColumn col in table.Columns)
//            {
//                header += $"{col.ColumnName,-25}";
//            }
//            Console.WriteLine(header);
//            Console.WriteLine(new string('-', header.Length));
//            Console.ResetColor();

//            // In dữ liệu
//            foreach (DataRow row in table.Rows)
//            {
//                string rowData = "";
//                foreach (DataColumn col in table.Columns)
//                {
//                    rowData += $"{row[col],-25}";
//                }
//                Console.WriteLine(rowData);
//            }
//        }
//    }
//}

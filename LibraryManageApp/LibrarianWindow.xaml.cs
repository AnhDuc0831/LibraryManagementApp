using BussinessObject;
using Microsoft.VisualBasic;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LibraryManageApp
{
    /// <summary>
    /// Interaction logic for LibrarianWindow.xaml
    /// </summary>
    public partial class LibrarianWindow : Window
    {
        private readonly BookServiceImpl bookServiceImpl=new BookServiceImpl();
        private readonly GeminiClient geminiClient;

        public LibrarianWindow(Librarian librarian)
        {
            this.geminiClient = new GeminiClient(1);
            InitializeComponent();
        }

        private void OpenBookManagement_Click(object sender, RoutedEventArgs e)
        {
            var win = new BookManagementWindow();
            win.Show();
        }

        private void OpenBorrowManagement_Click(object sender, RoutedEventArgs e)
        {
            var win = new BorrowManagementWindow();
            win.Show();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        private void OpenChatWindow_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChatWindow(1, null);
            win.Show();
        }

        private async void btnSmartImport_Click(object sender, RoutedEventArgs e)
        {
            // 1. Hiển thị hộp thoại yêu cầu nhập chủ đề
            string topic = Interaction.InputBox(
                "Nhập chủ đề sách bạn muốn tìm kiếm (VD: Sách kinh tế 2024):",
                "Smart Import - AI Research",
                "Sách bán chạy nhất năm nay");

            // Nếu người dùng bấm Cancel hoặc để trống thì thoát
            if (string.IsNullOrEmpty(topic)) return;

            // 2. Hiển thị thông báo đang xử lý (Vì đợi AI hơi lâu)
            // Bạn có thể thay bằng ProgressBar nếu muốn xịn hơn
            this.Cursor = Cursors.Wait;
            var btn = sender as Button;
            btn.Content = "Đang tìm kiếm...";
            btn.IsEnabled = false;

            try
            {
                // 3. Gọi hàm AI (Giả sử bạn đã khởi tạo geminiClient ở hàm khởi tạo Window)
                // Lưu ý: Đảm bảo bạn đã tạo instance: GeminiClient geminiClient = new GeminiClient(1);
                var books = await geminiClient.ResearchBooksAsync(topic);

                if (books.Count == 0)
                {
                    MessageBox.Show("AI không tìm thấy sách nào phù hợp.", "Kết quả", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // 4. Tạo thông báo xác nhận
                    string msg = $"AI tìm thấy {books.Count} cuốn sách:\n\n";
                    foreach (var b in books.Take(5)) // Liệt kê tối đa 5 cuốn để popup đỡ dài
                    {
                        msg += $"- {b.Title} ({b.AuthorName}) - {b.Price:N0} đ\n";
                    }
                    msg += "\nBạn có muốn nhập toàn bộ vào CSDL không?";

                    var result = MessageBox.Show(msg, "Xác nhận Import", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // 5. Gọi hàm lưu vào DB (Hàm ImportBooksToDatabase bạn đã viết ở Service/DAO)
                        // var bookDAO = new BookDAO(); // Ví dụ
                        // bookDAO.ImportBooksToDatabase(books);

                        // Nếu bạn viết hàm Import ngay trong Window này thì gọi trực tiếp:
                        this.bookServiceImpl.ImportBooksToDatabase(books);

                        MessageBox.Show($"Đã thêm thành công {books.Count} đầu sách!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 6. Trả lại trạng thái nút bấm
                this.Cursor = Cursors.Arrow;
                btn.Content = "Smart Import";
                btn.IsEnabled = true;
            }
        }
    }
}

using DTO;
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
    /// Interaction logic for SmartImportWindow.xaml
    /// </summary>
    public partial class SmartImportWindow : Window
    {
        private GeminiClient _geminiClient;
        private List<BookImportDto> _currentBookList;
        private BookService _bookService;


        public List<BookImportDto> SelectedBooks { get; private set; }
        public SmartImportWindow()
        {
            InitializeComponent();
            _geminiClient = new GeminiClient(1);
            _currentBookList = new List<BookImportDto>();
            _bookService = new BookServiceImpl();
        }
        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string topic = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(topic)) return;

            // UI updates
            AddMessageToChat("User", topic, Brushes.White, HorizontalAlignment.Right);
            txtInput.Clear();
            this.Cursor = Cursors.Wait;
            btnSaveToDB.IsEnabled = false; // Khóa nút lưu khi đang tìm
            lblStatus.Text = "AI đang tìm kiếm...";

            try
            {
                // Gọi AI (Hàm ResearchBooksAsync trả về List<BookImportDto>)
                // Bạn cần đảm bảo DTO của Service map được sang ViewModel của UI
                var booksDto = await _geminiClient.ResearchBooksAsync(topic);

                if (booksDto.Count == 0)
                {
                    AddMessageToChat("Bot", "Xin lỗi, tôi không tìm thấy cuốn sách nào phù hợp.", Brushes.MistyRose, HorizontalAlignment.Left);
                    lblStatus.Text = "";
                }
                else
                {
                    // Convert từ DTO sang ViewModel (để hiển thị trên Grid)
                    _currentBookList = booksDto.Select(b => new BookImportDto
                    {
                        IsSelected = true, // Mặc định tích chọn hết
                        Title = b.Title,
                        AuthorName = b.AuthorName,
                        Category = b.Category,
                        PublishYear = b.PublishYear,
                        Price = b.Price
                    }).ToList();

                    // Hiển thị lên Grid
                    dgResults.ItemsSource = _currentBookList;

                    AddMessageToChat("Bot", $"Đã tìm thấy {_currentBookList.Count} cuốn sách. Hãy kiểm tra danh sách bên phải, bỏ tích những cuốn không muốn thêm, rồi bấm nút 'Lưu'.", Brushes.LightGreen, HorizontalAlignment.Left);

                    // Mở khóa nút lưu
                    btnSaveToDB.IsEnabled = true;
                    lblStatus.Text = "";
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat("Bot", $"Lỗi: {ex.Message}", Brushes.Red, HorizontalAlignment.Left);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnSaveToDB_Click(object sender, RoutedEventArgs e)
        {
            // Lọc ra những cuốn sách có IsSelected = true
            var selectedBooks = _currentBookList.Where(b => b.IsSelected).ToList();

            if (selectedBooks.Count == 0)
            {
                MessageBox.Show("Bạn chưa chọn cuốn sách nào!", "Thông báo");
                return;
            }

            if (MessageBox.Show($"Bạn có chắc muốn thêm {selectedBooks.Count} cuốn sách này vào CSDL?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    // Convert ngược lại từ ViewModel sang DTO để gửi xuống tầng Service
                    var booksToImport = selectedBooks.Select(b => new BookImportDto
                    {
                        Title = b.Title,
                        AuthorName = b.AuthorName,
                        Category = b.Category,
                        PublishYear = b.PublishYear,
                        Price = b.Price
                        // Map thêm các trường khác nếu cần
                    }).ToList();

                    // Gọi hàm Import Database (bạn đã viết trước đó)
                    // Giả sử hàm này nằm ở class BookService hoặc gọi trực tiếp từ đây nếu logic đơn giản
                    // new BookService().ImportBooksToDatabase(booksToImport);
                    _bookService.ImportBooksToDatabase(booksToImport);
                    // Demo thông báo
                    AddMessageToChat("Bot", $"✅ Đã thêm thành công {selectedBooks.Count} sách vào hệ thống!", Brushes.LightCyan, HorizontalAlignment.Left);

                    // Clear Grid sau khi lưu xong để tránh lưu trùng
                    dgResults.ItemsSource = null;
                    btnSaveToDB.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi Database: " + ex.Message);
                }
            }
        }

        private void txtInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btnSend_Click(sender, e);
            }
        }

        private void AddMessageToChat(string senderName, string message, Brush background, HorizontalAlignment alignment)
        {
            Border bubble = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = background,
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                HorizontalAlignment = alignment,
                MaxWidth = 300,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };

            TextBlock textBlock = new TextBlock
            {
                Text = senderName == "User" ? message : $"{senderName}: {message}",
                TextWrapping = TextWrapping.Wrap
            };

            bubble.Child = textBlock;
            pnlChatHistory.Children.Add(bubble);
            scrollViewerChat.ScrollToBottom();
        }
    }
}

using Service; // Đảm bảo namespace này trỏ đúng tới nơi chứa DatabaseService và GeminiClient
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LibraryManageApp
{
    public partial class ChatWindow : Window
    {
        // Khởi tạo Services
        private readonly GeminiClient geminiClient;
        private readonly DatabaseService dbService = new DatabaseService();
        private readonly int role;
        private readonly string currentUserId;

        public ChatWindow(int role, string currentUserId)
        {
            InitializeComponent();
            txtInput.Focus();
            geminiClient = new GeminiClient(role);
            this.role = role;
            this.currentUserId = currentUserId;
        }

        // Xử lý khi nhấn phím Enter
        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        // Xử lý khi nhấn nút Gửi
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        // Logic chính
        private async void SendMessage()
        {
            string query = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            // 1. Hiển thị tin nhắn của User
            AddMessageToChat("User", query, Brushes.White, HorizontalAlignment.Right);
            txtInput.Clear();

            // UI Loading trạng thái
            var loadingMsg = AddMessageToChat("Bot", "Đang suy nghĩ...", Brushes.LightYellow, HorizontalAlignment.Left);

            try
            {
                // 2. Gọi Gemini lấy SQL
                string sqlQuery = await geminiClient.GetSqlQueryAsync(query, role, currentUserId);

                // Cập nhật tin nhắn loading thành SQL (để debug/thông báo)
                // Nếu muốn ẩn SQL, bạn có thể thay bằng text khác
                //((TextBlock)((Border)loadingMsg).Child).Text = $"Đã tạo SQL: {sqlQuery}";

                // 3. Thực thi SQL
                DataTable data = await dbService.ExecuteQueryAsync(sqlQuery);

                // 4. Hiển thị kết quả lên DataGrid
                if (data != null && data.Rows.Count > 0)
                {
                    dgResults.ItemsSource = data.DefaultView;
                    AddMessageToChat("Bot", $"Hoàn tất! Tìm thấy {data.Rows.Count} kết quả.", Brushes.LightGreen, HorizontalAlignment.Left);
                }
                else
                {
                    dgResults.ItemsSource = null;
                    AddMessageToChat("Bot", "Không tìm thấy dữ liệu nào phù hợp.", Brushes.LightGray, HorizontalAlignment.Left);
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat("Bot", $"Lỗi: {ex.Message}", Brushes.LightPink, HorizontalAlignment.Left);
            }
            finally
            {
                // Cuộn xuống cuối
                scrollViewerChat.ScrollToBottom();
            }
        }

        // Hàm hỗ trợ vẽ bong bóng chat
        private UIElement AddMessageToChat(string senderName, string message, Brush background, HorizontalAlignment alignment)
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

            return bubble;
        }
    }
}
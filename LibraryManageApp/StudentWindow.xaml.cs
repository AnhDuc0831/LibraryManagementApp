using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using DataAccsess;
using LibraryManageApp.ViewModels;
using BussinessObject;

namespace LibraryManageApp
{
    public partial class StudentWindow : Window
    {
        private readonly DBLibraryManagementContext _db = new DBLibraryManagementContext();

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private int _totalItems = 0;
        private readonly int currentUserId;

        public StudentWindow(Student student)
        {
            InitializeComponent();
            Loaded += StudentWindow_Loaded;
            this.currentUserId = student.StudentId;
        }

        private void StudentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // đảm bảo chạy sau khi visual tree hoàn tất
            Dispatcher.BeginInvoke(() => _ = LoadFilterOptionsAsync());
        }

        private async Task LoadFilterOptionsAsync()
        {
            // fallback nếu field do XAML sinh không tồn tại
            var authorCombo = cboFilterAuthor ?? FindName("cboFilterAuthor") as ComboBox;
            var categoryCombo = cboFilterCategory ?? FindName("cboFilterCategory") as ComboBox;
            var pageSizeCombo = cboPageSize ?? FindName("cboPageSize") as ComboBox;

            if (authorCombo == null || categoryCombo == null || pageSizeCombo == null)
            {
                MessageBox.Show(
                    "Không tìm thấy control UI (cboFilterAuthor / cboFilterCategory / cboPageSize).\n" +
                    "Hãy Save XAML và thực hiện Clean → Rebuild. Kiểm tra `x:Name` trong StudentWindow.xaml.",
                    "UI initialization", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var authors = await _db.Authors
                                       .OrderBy(a => a.FullName)
                                       .Select(a => a.FullName)
                                       .Distinct()
                                       .ToListAsync();

                authorCombo.Items.Clear();
                authorCombo.Items.Add(string.Empty);
                foreach (var a in authors) authorCombo.Items.Add(a);

                var categories = await _db.Books
                                          .Where(b => b.Category != null)
                                          .Select(b => b.Category)
                                          .Distinct()
                                          .OrderBy(c => c)
                                          .ToListAsync();

                categoryCombo.Items.Clear();
                categoryCombo.Items.Add(string.Empty);
                foreach (var c in categories) categoryCombo.Items.Add(c);

                if (pageSizeCombo.SelectedItem == null)
                    pageSizeCombo.SelectedIndex = 1; // default 10
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading filters", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            await LoadBooksAsync(_currentPage);
        }

        private async void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                await LoadBooksAsync(_currentPage - 1);
            }
        }

        private async void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                await LoadBooksAsync(_currentPage + 1);
            }
        }

        private async void CboPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPageSize?.SelectedItem is ComboBoxItem item)
            {
                if (int.TryParse(item.Content.ToString(), out var newSize))
                {
                    _pageSize = newSize;
                    _currentPage = 1;
                    await LoadBooksAsync(_currentPage);
                }
            }
        }

        private async void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            if (txtSearch != null) txtSearch.Text = string.Empty;
            if (cboFilterAuthor != null) cboFilterAuthor.SelectedIndex = 0;
            if (cboFilterCategory != null) cboFilterCategory.SelectedIndex = 0;
            if (txtYearFrom != null) txtYearFrom.Text = string.Empty;
            if (txtYearTo != null) txtYearTo.Text = string.Empty;
            if (chkAvailableOnly != null) chkAvailableOnly.IsChecked = false;
            _currentPage = 1;
            await LoadBooksAsync(_currentPage);
        }

        private async Task LoadBooksAsync(int page)
        {
            try
            {
                if (btnSearch != null) btnSearch.IsEnabled = false;

                var queryText = txtSearch?.Text?.Trim() ?? string.Empty;
                var criteria = (cboCriteria?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Title";
                var filterAuthor = cboFilterAuthor?.SelectedItem?.ToString();
                var filterCategory = cboFilterCategory?.SelectedItem?.ToString();

                int.TryParse(txtYearFrom?.Text ?? string.Empty, out var yearFrom);
                int.TryParse(txtYearTo?.Text ?? string.Empty, out var yearTo);
                var availableOnly = chkAvailableOnly?.IsChecked == true;

                var query = _db.Books
                               .Include(b => b.Author)
                               .Include(b => b.BookInstances)
                                   .ThenInclude(i => i.BorrowDetails)
                                       .ThenInclude(d => d.Borrow)
                               .AsNoTracking()
                               .AsQueryable();

                if (!string.IsNullOrEmpty(queryText))
                {
                    var text = queryText.Trim();
                    switch (criteria)
                    {
                        case "Author":
                            query = query.Where(b => b.Author.FullName.Contains(text));
                            break;
                        case "Category":
                            query = query.Where(b => b.Category != null && b.Category.Contains(text));
                            break;
                        default:
                            query = query.Where(b => b.Title.Contains(text));
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(filterAuthor))
                    query = query.Where(b => b.Author.FullName == filterAuthor);

                if (!string.IsNullOrEmpty(filterCategory))
                    query = query.Where(b => b.Category == filterCategory);

                if (yearFrom > 0)
                    query = query.Where(b => b.PublishYear.HasValue && b.PublishYear >= yearFrom);

                if (yearTo > 0)
                    query = query.Where(b => b.PublishYear.HasValue && b.PublishYear <= yearTo);

                var books = await query.ToListAsync();

                var results = books.Select(b =>
                {
                    var totalInstances = (b.BookInstances?.Count).GetValueOrDefault();
                    var borrowedCount = b.BookInstances?
                        .SelectMany(i => i.BorrowDetails)
                        .Count(d => d.Borrow != null && d.Borrow.ReturnDate == null) ?? 0;

                    if (totalInstances == 0)
                        totalInstances = b.Quantity;

                    var available = Math.Max(0, totalInstances - borrowedCount);

                    return new BookResult
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        Author = b.Author?.FullName ?? string.Empty,
                        PublishYear = b.PublishYear,
                        Quantity = b.Quantity,
                        Category = b.Category,
                        AvailableCount = available,
                        IsAvailable = available > 0,
                        CoverImagePath = GetCoverImagePath(b.BookId)
                    };
                }).ToList();

                if (availableOnly)
                    results = results.Where(r => r.IsAvailable).ToList();

                _totalItems = results.Count;
                if (_totalItems == 0)
                {
                    if (dgBooks != null) dgBooks.ItemsSource = null;
                    MessageBox.Show("Sách không có sẵn", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (lblPageInfo != null) lblPageInfo.Text = "Trang 0/0";
                    if (btnPrevPage != null) btnPrevPage.IsEnabled = false;
                    if (btnNextPage != null) btnNextPage.IsEnabled = false;
                    return;
                }

                _totalPages = Math.Max(1, (int)Math.Ceiling(_totalItems / (double)_pageSize));
                if (page < 1) page = 1;
                if (page > _totalPages) page = _totalPages;

                var pageItems = results
                                .OrderBy(r => r.Title)
                                .Skip((page - 1) * _pageSize)
                                .Take(_pageSize)
                                .ToList();

                if (dgBooks != null) dgBooks.ItemsSource = pageItems;

                _currentPage = page;
                if (lblPageInfo != null) lblPageInfo.Text = $"Trang {_currentPage}/{_totalPages} ({_totalItems} kết quả)";

                if (btnPrevPage != null) btnPrevPage.IsEnabled = _currentPage > 1;
                if (btnNextPage != null) btnNextPage.IsEnabled = _currentPage < _totalPages;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (btnSearch != null) btnSearch.IsEnabled = true;
            }
        }

        private string GetCoverImagePath(int bookId)
        {
            try
            {
                var baseDir = AppContext.BaseDirectory;
                var coversDir = Path.Combine(baseDir, "Covers");
                if (!Directory.Exists(coversDir)) return null;

                var candidates = new[]
                {
                    $"{bookId}.jpg",
                    $"{bookId}.jpeg",
                    $"{bookId}.png",
                    $"{bookId}.bmp",
                    $"book_{bookId}.jpg",
                    $"book_{bookId}.png",
                    $"book_{bookId}.jpeg"
                };

                foreach (var candidate in candidates)
                {
                    var path = Path.Combine(coversDir, candidate);
                    if (File.Exists(path)) return path;
                }
            }
            catch
            {
                // ignore IO errors and return null
            }

            return null;
        }

        private void BtnLoadHistory_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            var code = txtStudentCode?.Text?.Trim();
            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Nhập Student Code.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            lblStudentName.Text = string.Empty;
            dgHistory.ItemsSource = null;

            var student = await _db.Students
                             .Include(s => s.Borrows)
                                .ThenInclude(br => br.BorrowDetails)
                                    .ThenInclude(d => d.Instance)
                                        .ThenInclude(i => i.Book)
                             .Include(s => s.Borrows)
                                .ThenInclude(br => br.Librarian)
                             .AsNoTracking()
                             .FirstOrDefaultAsync(s => s.Code == code);

            if (student == null)
            {
                MessageBox.Show("Không tìm thấy student với mã đã nhập.", "Not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lblStudentName.Text = student.FullName;

            var history = student.Borrows
                .OrderByDescending(b => b.BorrowDate)
                .SelectMany(b => b.BorrowDetails.Select(d => new BorrowHistoryItem
                {
                    BorrowDate = b.BorrowDate.ToDateTime(TimeOnly.MinValue),
                    ReturnDate = b.ReturnDate.HasValue ? b.ReturnDate.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd") : string.Empty,
                    Status = b.Status,
                    BookTitle = d.Instance.Book.Title,
                    InstanceId = d.Instance.InstanceId,
                    LibrarianName = b.Librarian != null ? b.Librarian.Name : string.Empty
                }))
                .ToList();

            dgHistory.ItemsSource = history;
        }

        private void BtnChatAI_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChatWindow(2, this.currentUserId.ToString());
            win.Show();
        }
    }
}       
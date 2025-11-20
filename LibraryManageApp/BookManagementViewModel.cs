using BussinessObject;
using Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManageApp
{
    public class BookManagementViewModel : INotifyPropertyChanged
    {
        private readonly BookService _bookService;
        private readonly AuthorService _authorService;  

        public ObservableCollection<Book> Books { get; } = new();

        public ObservableCollection<Author> Authors { get; } = new();

        public List<string> Categories { get; } = new()
        {
            "Văn học",
            "Khoa học",
            "Lịch sử",
            "Triết học",
            "Nghệ thuật",
            "Tôn giáo"
        };

        private Book? _selectedBook;
        public Book? SelectedBook
        {
            get => _selectedBook;
            set { _selectedBook = value; OnPropertyChanged(nameof(SelectedBook)); }
        }

        public BookManagementViewModel()
        {
            _bookService = new BookServiceImpl();
            _authorService = new AuthorServiceImpl();

            _ = LoadBooks();
            _ = LoadAuthors();
        }

        public async Task LoadBooks()
        {
            Books.Clear();
            foreach (var b in await _bookService.GetBooksAsync())
                Books.Add(b);
        }

        public async Task LoadAuthors()
        {
            Authors.Clear();
            foreach (var a in await _authorService.GetAuthorsAsync())
                Authors.Add(a);
        }

        public async Task AddBook(Book book)
        {
            if (book == null) return;
            await _bookService.AddBookAsync(book);
            await LoadBooks();
        }

        public async Task UpdateBook()
        {
            if (SelectedBook == null) return;
            await _bookService.UpdateBookAsync(SelectedBook);
            await LoadBooks();
        }

        public async Task DeleteBook()
        {
            if (SelectedBook == null) return;
            await _bookService.DeleteBookAsync(SelectedBook.BookId);
            await LoadBooks();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

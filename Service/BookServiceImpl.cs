using BussinessObject;
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
    }

    public class BookServiceImpl : BookService
    {
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
    }

}

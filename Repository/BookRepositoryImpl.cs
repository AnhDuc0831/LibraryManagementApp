using BussinessObject;
using DataAccsess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface BookRepository
    {
        Task<List<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(int id);
        Task<List<BookInstance>> GetByBookIdAsync(int bookId);
        Task SaveInstancesAsync(int bookId, List<BookInstance> list);
    }

    public class BookRepositoryImpl : BookRepository
    {
        private readonly DBLibraryManagementContext db;
        public BookRepositoryImpl() 
        {
            db = new DBLibraryManagementContext();
        }

        public Task<List<BookInstance>> GetByBookIdAsync(int bookId)
        {
            return db.BookInstances.Where(x => x.BookId == bookId).ToListAsync();
        }

        public async Task SaveInstancesAsync(int bookId, List<BookInstance> editedList)
        {
            var existing = await db.BookInstances
        .Where(x => x.BookId == bookId)
        .ToListAsync();

            // Update + add
            foreach (var inst in editedList)
            {
                if (inst.InstanceId == 0)
                {
                    // New instance
                    inst.BookId = bookId;
                    db.BookInstances.Add(inst);
                }
                else
                {
                    // Update existing
                    var target = existing.First(e => e.InstanceId == inst.InstanceId);
                    target.Condition = inst.Condition;
                }
            }

            // Delete removed
            var editedIds = editedList
                .Where(i => i.InstanceId != 0)
                .Select(i => i.InstanceId)
                .ToHashSet();

            var toDelete = existing
                .Where(e => !editedIds.Contains(e.InstanceId))
                .ToList();

            db.BookInstances.RemoveRange(toDelete);

            await db.SaveChangesAsync();
        }

        public async Task<List<Book>> GetAllAsync()
        {
            return await db.Books.Include(b => b.Author).ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await db.Books.FindAsync(id);
        }

        public async Task AddAsync(Book book)
        {
            using var db = new DBLibraryManagementContext();

            db.Books.Add(book);
            await db.SaveChangesAsync(); 

            var instances = new List<BookInstance>();

            for (int i = 0; i < book.Quantity; i++)
            {
                instances.Add(new BookInstance
                {
                    BookId = book.BookId,
                    Condition = "New"
                });
            }

            db.BookInstances.AddRange(instances);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            db.Books.Update(book);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await db.Books.FindAsync(id);
            if (book == null) return;

            db.Books.Remove(book);
            await db.SaveChangesAsync();
        }
    }
}

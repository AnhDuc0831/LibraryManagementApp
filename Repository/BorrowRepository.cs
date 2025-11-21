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

    public class BorrowRepository
    {
        private readonly DBLibraryManagementContext db;
        public BorrowRepository()
        {
            db = new DBLibraryManagementContext();
        }

        public async Task<List<Borrow>> GetByStudentIdAsync(string studentCode)
        {

            return await db.Borrows
                .Include(b => b.Student)
                .Include(b => b.BorrowDetails)
                    .ThenInclude(d => d.Instance)
                .Include(b => b.BorrowDetails)
                    .ThenInclude(d => d.BorrowFines)
                .Where(x => x.Student.Code == studentCode)
                .ToListAsync();
        }

        public async Task UpdateAsync(Borrow borrow)
        {
            db.Borrows.Update(borrow);
            await db.SaveChangesAsync();
        }

        public async Task AddFineAsync(int borrowDetailId, int amount, string reason)
        {
            db.BorrowFines.Add(new BorrowFine
            {
                BorrowDetailId = borrowDetailId,
                Amount = amount,
                Reason = reason
            });

            await db.SaveChangesAsync();
        }
    }
}

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

        public async Task<List<Borrow>> GetByStudentIdAsync(int studentId)
        {

            return await db.Borrows
                .Where(x => x.StudentId == studentId)
                .Include(x => x.BorrowDetails)
                    .ThenInclude(d => d.Instance)
                .Include(x => x.BorrowDetails)
                    .ThenInclude(d => d.BorrowFines)
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

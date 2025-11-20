using BussinessObject;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class BorrowService
    {
        private readonly BorrowRepository _borrowRepo;

        public BorrowService()
        {
            _borrowRepo = new BorrowRepository();
        }

        public Task<List<Borrow>> GetStudentHistoryAsync(int studentId)
            => _borrowRepo.GetByStudentIdAsync(studentId);

        public Task UpdateBorrowAsync(Borrow borrow)
            => _borrowRepo.UpdateAsync(borrow);

        public Task AddFineAsync(int detailId, int amount, string reason)
        => _borrowRepo.AddFineAsync(detailId, amount, reason);
    }
}

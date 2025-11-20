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
    public class BorrowManagementViewModel : INotifyPropertyChanged
    {
        private readonly BorrowService _borrowService;

        public ObservableCollection<Borrow> Borrows { get; } = new();
        public ObservableCollection<BorrowDetail> Details { get; } = new();
        public ObservableCollection<BorrowFine> Fines { get; } = new();

        private Borrow _selectedBorrow;
        public Borrow SelectedBorrow
        {
            get => _selectedBorrow;
            set
            {
                _selectedBorrow = value;
                LoadBorrowInfo();
                OnPropertyChanged(nameof(SelectedBorrow));
            }
        }

        private BorrowDetail _selectedDetail;
        public BorrowDetail SelectedDetail
        {
            get => _selectedDetail;
            set
            {
                _selectedDetail = value;
                LoadFineInfo();
                OnPropertyChanged(nameof(SelectedDetail));
            }
        }

        public BorrowManagementViewModel()
        {
            _borrowService = new BorrowService();
        }

        public async Task Search(int studentId)
        {
            Borrows.Clear();
            foreach (var b in await _borrowService.GetStudentHistoryAsync(studentId))
                Borrows.Add(b);
        }

        private void LoadBorrowInfo()
        {
            Details.Clear();
            Fines.Clear();

            if (SelectedBorrow == null) return;

            foreach (var d in SelectedBorrow.BorrowDetails)
                Details.Add(d);
        }

        private void LoadFineInfo()
        {
            Fines.Clear();
            if (SelectedDetail == null) return;

            foreach (var f in SelectedDetail.BorrowFines)
                Fines.Add(f);
        }

        public async Task AddFine(int amount, string reason)
        {
            if (SelectedDetail == null) return;

            await _borrowService.AddFineAsync(SelectedDetail.BorrowDetailId, amount, reason);
        }

        public async Task SetReturned(string status)
        {
            SelectedBorrow.Status = status;
            SelectedBorrow.ReturnDate = DateOnly.FromDateTime(DateTime.Now);

            await _borrowService.UpdateBorrowAsync(SelectedBorrow);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

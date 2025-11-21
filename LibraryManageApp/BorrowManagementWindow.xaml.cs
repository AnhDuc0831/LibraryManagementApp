using Repository;
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
    /// Interaction logic for BorrowManagementWindow.xaml
    /// </summary>
    public partial class BorrowManagementWindow : Window
    {
        private readonly BorrowManagementViewModel _vm;

        public BorrowManagementWindow()
        {
            InitializeComponent();

            var borrowRepo = new BorrowRepository();

            _vm = new BorrowManagementViewModel();

            DataContext = _vm;
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtStudentId.Text.Trim()))
            {
                await _vm.Search(txtStudentId.Text.Trim());
            }
        }

        private async void AddFine_Click(object sender, RoutedEventArgs e)
        {
            var input = new AddFineWindow(); // simple popup containing amount + reason
            if (input.ShowDialog() == true)
            {
                await _vm.AddFine(input.Amount, input.Reason);
                await _vm.Search(_vm.SelectedBorrow.Student.Code);
            }
        }

        private async void MarkReturned_Click(object sender, RoutedEventArgs e)
        {
            await _vm.SetReturned("Returned");
            await _vm.Search(_vm.SelectedBorrow.Student.Code);
        }

        private async void MarkLate_Click(object sender, RoutedEventArgs e)
        {
            await _vm.SetReturned("Late");
            await _vm.Search(_vm.SelectedBorrow.Student.Code);
        }
    }

}

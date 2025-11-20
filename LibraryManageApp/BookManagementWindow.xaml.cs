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
    /// Interaction logic for BookManagementWindow.xaml
    /// </summary>
    public partial class BookManagementWindow : Window
    {
        private readonly BookManagementViewModel _vm;

        public BookManagementWindow()
        {
            InitializeComponent();

            _vm = new BookManagementViewModel();

            DataContext = _vm;
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var popup = new AddBookWindow(_vm.Authors.ToList())
            {
                Owner = this
            };

            if (popup.ShowDialog() == true)
            {
                await _vm.AddBook(popup.CreatedBook);
            }
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            await _vm.UpdateBook();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await _vm.DeleteBook();
        }

        private async void EditInstances_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedBook == null)
                return;

            var popup = new BookInstanceWindow(_vm.SelectedBook.BookId, new BookServiceImpl())
            {
                Owner = this
            };

            if (popup.ShowDialog() == true)
            {
                int newQty = popup.UpdatedInstances.Count;

                _vm.SelectedBook.Quantity = newQty;

                await _vm.UpdateBook();
            }
        }
    }
}

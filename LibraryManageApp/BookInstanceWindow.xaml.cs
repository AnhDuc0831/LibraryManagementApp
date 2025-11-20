using BussinessObject;
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
    /// Interaction logic for BookInstanceWindow.xaml
    /// </summary>
    public partial class BookInstanceWindow : Window
    {
        private readonly int _bookId;
        private readonly BookService _service;

        private List<BookInstance> _instances;

        public List<BookInstance> UpdatedInstances => _instances;

        public BookInstanceWindow(int bookId, BookService service)
        {
            InitializeComponent();
            _bookId = bookId;
            _service = service;

            LoadInstances();
        }

        private async void LoadInstances()
        {
            _instances = await _service.GetByBookIdAsync(_bookId);
            dgInstances.ItemsSource = null;
            dgInstances.ItemsSource = _instances;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            _instances.Add(new BookInstance
            {
                BookId = _bookId,
                Condition = "New"
            });

            dgInstances.Items.Refresh();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (dgInstances.SelectedItem is BookInstance instance)
            {
                _instances.Remove(instance);
                dgInstances.Items.Refresh();
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            await _service.SaveInstancesAsync(_bookId, _instances);
            DialogResult = true;
            Close();
        }
    }
}

using BussinessObject;
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
    /// Interaction logic for AddBookWindow.xaml
    /// </summary>
    public partial class AddBookWindow : Window
    {
        public Book CreatedBook { get; private set; }

        public AddBookWindow(List<Author> authors)
        {
            InitializeComponent();

            cbAuthor.ItemsSource = authors;

            cbCategory.ItemsSource = new List<string>
            {
                "Văn học", "Khoa học", "Lịch sử", "Triết học", "Nghệ thuật", "Tôn giáo"
            };
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (cbAuthor.SelectedValue == null)
            {
                MessageBox.Show("Please select an author.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Title cannot be empty.");
                return;
            }

            CreatedBook = new Book
            {
                Title = txtTitle.Text,
                AuthorId = (int)cbAuthor.SelectedValue,
                PublishYear = int.TryParse(txtYear.Text, out int y) ? y : null,
                Quantity = int.TryParse(txtQty.Text, out int q) ? q : 0,
                Price = decimal.TryParse(txtPrice.Text, out decimal p) ? p : null,
                Category = cbCategory.SelectedItem?.ToString()
            };

            DialogResult = true;
            Close();
        }
    }
}

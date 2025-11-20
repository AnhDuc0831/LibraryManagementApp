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
    /// Interaction logic for AddFineWindow.xaml
    /// </summary>
    public partial class AddFineWindow : Window
    {
        public int Amount { get; private set; }
        public string Reason { get; private set; }

        public AddFineWindow()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtAmount.Text, out int amount))
            {
                MessageBox.Show("Amount must be a number.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Reason cannot be empty.");
                return;
            }

            Amount = amount;
            Reason = txtReason.Text.Trim();

            DialogResult = true;
            Close();
        }
    }
}

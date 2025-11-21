using BussinessObject;
using Microsoft.VisualBasic;
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
    /// Interaction logic for LibrarianWindow.xaml
    /// </summary>
    public partial class LibrarianWindow : Window
    {
        private readonly BookServiceImpl bookServiceImpl=new BookServiceImpl();
        private readonly GeminiClient geminiClient;

        public LibrarianWindow(Librarian librarian)
        {
            this.geminiClient = new GeminiClient(1);
            InitializeComponent();
        }

        private void OpenBookManagement_Click(object sender, RoutedEventArgs e)
        {
            var win = new BookManagementWindow();
            win.Show();
        }

        private void OpenBorrowManagement_Click(object sender, RoutedEventArgs e)
        {
            var win = new BorrowManagementWindow();
            win.Show();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        private void OpenChatWindow_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChatWindow(1, null);
            win.Show();
        }

        private async void btnSmartImport_Click(object sender, RoutedEventArgs e)
        {
            SmartImportWindow importWin = new SmartImportWindow();
            importWin.Show(); // Hoặc ShowDialog()
        }
    }
}

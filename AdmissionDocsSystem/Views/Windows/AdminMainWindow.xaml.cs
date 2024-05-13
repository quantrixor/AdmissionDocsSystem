using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.Views.Pages.AdminPages;
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

namespace AdmissionDocsSystem.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для AdminMainWindow.xaml
    /// </summary>
    public partial class AdminMainWindow : Window
    {
        private Users _currentUser;
        public AdminMainWindow(Users currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdminFrame.Navigate(new ApplicantsListPage());
        }
    }
}

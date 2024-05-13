using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.Views.Pages;
using System.Windows;

namespace AdmissionDocsSystem.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ApplicantsMainWindow.xaml
    /// </summary>
    public partial class ApplicantsMainWindow : Window
    {
        private Users _currentUser;

        public ApplicantsMainWindow(Users currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainUserFrame.Navigate(new ProfileApplicantsPage(_currentUser));
        }
    }
}

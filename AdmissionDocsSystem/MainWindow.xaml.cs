using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.Views.Pages;
using System.Windows;

namespace AdmissionDocsSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RegistrationPage());
        }
    }
}

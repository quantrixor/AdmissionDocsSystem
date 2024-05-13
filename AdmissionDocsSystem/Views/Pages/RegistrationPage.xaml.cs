using AdmissionDocsSystem.Views.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdmissionDocsSystem.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }
        private void CloseMainWindow()
        {
            // Этот код предполагает, что страница содержится внутри Frame, который находится в MainWindow
            var mainWindow = Window.GetWindow(this);

            // Проверяем, не равен ли mainWindow null, и является ли он экземпляром MainWindow
            if (mainWindow != null && mainWindow is MainWindow)
            {
                mainWindow.Close();
            }
        }


        private void CancelRegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            SignInWindow signInWindow = new SignInWindow();
            signInWindow.Show();
            CloseMainWindow();

        }
    }
}

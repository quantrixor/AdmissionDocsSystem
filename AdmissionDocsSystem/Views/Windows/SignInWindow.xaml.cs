using AdmissionDocsSystem.Model;
using System;
using System.Linq;
using System.Windows;

namespace AdmissionDocsSystem.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        public SignInWindow()
        {
            InitializeComponent();
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = UserPasswordBox.Password;

            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пустые значения недопустимы.", "Ошибка авторизации", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = AuthenticateUser(email, password);
                if (user != null)
                {
                    if (user.Roles.RoleName == "Administrator")
                    {
                        AdminMainWindow adminWindow = new AdminMainWindow(user);
                        adminWindow.Show();
                    }
                    else if (user.Roles.RoleName == "Applicant")
                    {
                        ApplicantsMainWindow applicantsMainWindow = new ApplicantsMainWindow(user);
                        applicantsMainWindow.Show();
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный email или пароль. Попробуйте снова.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public Users AuthenticateUser(string email, string password)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var user = db.Users.Include("Roles")
                                   .FirstOrDefault(u => u.Email == email && u.Password == password);
                return user;
            }
        }

    }
}

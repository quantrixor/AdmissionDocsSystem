using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.ViewModel;
using AdmissionDocsSystem.Views.Windows;
using System;
using System.Windows;
using System.Windows.Controls;

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

        private void SubmitDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(DataProcessingConsentCheckBox.IsChecked == false)
                {
                    MessageBox.Show("Вам необходимо подтвердить своё согласие на обработку персональных данных.",
                        "Внимание, системное уведомление!", MessageBoxButton.OK,MessageBoxImage.Warning);
                    return;
                }
                using (var db = new AdmissionDocsSystemEntities())
                {
                    // Создание нового пользователя
                    var user = new Users
                    {
                        Email = EmailTextBox.Text,
                        Password = PasswordGenerator.GeneratePassword(),
                        IsActive = false,
                        RoleID = 2, // По умолчанию это абитуриент
                        RegistrationDate = DateTime.Now
                    };

                    db.Users.Add(user);
                    db.SaveChanges(); // Сохраняем пользователя, чтобы получить его ID для связи с абитуриентом

                    // Создание нового абитуриента
                    var applicant = new Applicants
                    {
                        UserID = user.UserID, // Связываем с только что созданным пользователем
                        FirstName = FirstNameTextBox.Text,
                        LastName = LastNameTextBox.Text,
                        MiddleName = MiddleNameTextBox.Text,
                        DateOfBirth = BirthDatePicker.SelectedDate.Value,
                        Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                        EducationalLevelID = (int)(EducationLevelComboBox.SelectedValue),
                        ProgramTypeID = (int)(FieldOfStudyComboBox.SelectedValue),
                        RegistrationAddress = RegistrationAddressTextBox.Text,
                        ResidentialAddress = ResidentialAddressTextBox.Text,
                        DataConsentGiven = DataProcessingConsentCheckBox.IsChecked.Value,
                        ApplicationStatusID = 1, // По умолчанию в ожидании
                        IsConfirmed = false // По умолчанию, регистрация не подтверждена
                    };

                    db.Applicants.Add(applicant);
                    db.SaveChanges(); // Сохраняем абитуриента

                    MessageBox.Show("Регистрация прошла успешно. Ваша заявка отправлена на проверку.", "Ваши данные отправлены.",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData.LoadEducationalLevels(EducationLevelComboBox);
            LoadData.LoadProgramTypes(FieldOfStudyComboBox);
            LoadData.LoadEducationForm(EducationFormComboBox);
        }
    }
}

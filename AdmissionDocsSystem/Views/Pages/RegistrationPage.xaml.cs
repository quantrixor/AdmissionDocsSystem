using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.ViewModel;
using AdmissionDocsSystem.Views.Windows;
using System;
using System.Net.Sockets;
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
                if (DataProcessingConsentCheckBox.IsChecked == false)
                {
                    MessageBox.Show("Вам необходимо подтвердить своё согласие на обработку персональных данных.",
                        "Внимание, системное уведомление!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!EmailValidator.IsValidEmail(EmailTextBox.Text))
                {
                    MessageBox.Show("Вы ввели недействительный электронный адрес. Пожалуйста, убедитесь в правильности введенных вами данных!",
                        "Неккоретный адрес e-mail", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(EmailTextBox.Text) || string.IsNullOrEmpty(FirstNameTextBox.Text) ||
                    string.IsNullOrEmpty(LastNameTextBox.Text) || string.IsNullOrEmpty(MiddleNameTextBox.Text) ||
                    string.IsNullOrEmpty(PhoneNumberTextBox.Text) ||
                    string.IsNullOrEmpty(RegistrationAddressTextBox.Text) || string.IsNullOrEmpty(ResidentialAddressTextBox.Text) ||
                    string.IsNullOrEmpty(GenderComboBox.Text) || string.IsNullOrEmpty(EducationFormComboBox.Text) ||
                    string.IsNullOrEmpty(FieldOfStudyComboBox.Text) || string.IsNullOrEmpty(EducationLevelComboBox.Text))
                {
                    MessageBox.Show("Заполните все поля! Пустые значения недопустимы.",
                        "Некорректные данные", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new AdmissionDocsSystemEntities())
                {
                    // Создание нового пользователя
                    var user = new Users
                    {
                        Email = EmailTextBox.Text,
                        Password = null,
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

                    MessageBox.Show("Регистрация прошла успешно. Ваша заявка отправлена на проверку. По окончанию проверки, вы получите письмо на указанную почту.", "Ваши данные отправлены.",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            // Очищаем текстовые поля
            FirstNameTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            MiddleNameTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            PhoneNumberTextBox.Text = string.Empty;
            RegistrationAddressTextBox.Text = string.Empty;
            ResidentialAddressTextBox.Text = string.Empty;

            // Сбрасываем DatePicker
            BirthDatePicker.SelectedDate = null;

            // Сброс состояния ComboBox-ов (предполагается, что они имеют пустой элемент для 'не выбрано')
            GenderComboBox.SelectedItem = null;
            EducationLevelComboBox.SelectedItem = null;
            FieldOfStudyComboBox.SelectedItem = null;
            EducationFormComboBox.SelectedItem = null;

            // Сброс CheckBox
            DataProcessingConsentCheckBox.IsChecked = false;
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData.LoadEducationalLevels(EducationLevelComboBox);
            LoadData.LoadProgramTypes(FieldOfStudyComboBox);
            LoadData.LoadEducationForm(EducationFormComboBox);
        }

        private void PhoneNumberTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
                return;
            }

            TextBox textBox = sender as TextBox;
            string text = textBox.Text + e.Text;

            if (text.Length == 1)
            {
                textBox.Text = "+7 (";
                textBox.SelectionStart = textBox.Text.Length;
            }
            else if (text.Length == 8)
            {
                textBox.Text += ") ";
                textBox.SelectionStart = textBox.Text.Length;
            }
            else if (text.Length == 13 || text.Length == 16)
            {
                textBox.Text += "-";
                textBox.SelectionStart = textBox.Text.Length;
            }

            if (textBox.Text.Length >= 18)
            {
                e.Handled = true;
            }
        }
    }
}

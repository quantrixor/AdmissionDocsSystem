using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdmissionDocsSystem.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для CardApplicantWindow.xaml
    /// </summary>
    public partial class CardApplicantWindow : Window
    {
        private ApplicantViewModel _applicant;

        public CardApplicantWindow(ApplicantViewModel applicant)
        {
            InitializeComponent();
            _applicant = applicant;
            this.DataContext = _applicant;
        }

        private void ToggleEditMode(bool isEditing)
        {
            // Переключение доступности элементов управления
            FirstNameTextBox.IsReadOnly = !isEditing;
            LastNameTextBox.IsReadOnly = !isEditing;
            MiddleNameTextBox.IsReadOnly = !isEditing;
            BirthDatePicker.IsEnabled = isEditing;
            GenderComboBox.IsEnabled = isEditing;
            EducationLevelComboBox.IsEnabled = isEditing;
            FieldOfStudyComboBox.IsEnabled = isEditing;
            EducationFormComboBox.IsEnabled = isEditing;
            EmailTextBox.IsReadOnly = !isEditing;
            PhoneNumberTextBox.IsReadOnly = !isEditing;
            RegistrationAddressTextBox.IsReadOnly = !isEditing;
            ResidentialAddressTextBox.IsReadOnly = !isEditing;
            ApplicationStatusComboBox.IsEnabled = isEditing;

            // Изменение текста кнопки в зависимости от состояния
            EditButton.Content = isEditing ? "Сохранить изменения" : "Редактировать";
        }

        private bool _isEditing = false;

        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
                return;

            try
            {
                using (var db = new AdmissionDocsSystemEntities())
                {
                    var applicantId = _applicant.ApplicantID; // Предполагаем, что ID текущего абитуриента уже загружен
                    var applicant = db.Applicants.Include("Users").FirstOrDefault(a => a.ApplicantID == applicantId);
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
                    if (applicant != null)
                    {
                        applicant.FirstName = FirstNameTextBox.Text;
                        applicant.LastName = LastNameTextBox.Text;
                        applicant.MiddleName = MiddleNameTextBox.Text;
                        applicant.DateOfBirth = BirthDatePicker.SelectedDate;
                        applicant.Gender = GenderComboBox.SelectedValue as string;
                        applicant.EducationalLevelID = (int?)EducationLevelComboBox.SelectedValue;
                        applicant.ProgramTypeID = (int?)FieldOfStudyComboBox.SelectedValue;
                        applicant.PhoneNumber = PhoneNumberTextBox.Text;
                        applicant.Users.Email = EmailTextBox.Text;
                        applicant.RegistrationAddress = RegistrationAddressTextBox.Text;
                        applicant.ResidentialAddress = ResidentialAddressTextBox.Text;

                        int newStatusId = Convert.ToInt32(ApplicationStatusComboBox.SelectedValue);

                        bool statusChangedToApproved = newStatusId != applicant.ApplicationStatusID && newStatusId == 2; // Assuming 2 is the ID for "Approved"

                        applicant.ApplicationStatusID = newStatusId;
                        if (statusChangedToApproved)
                        {
                            // Update additional properties
                            applicant.IsConfirmed = true;
                            applicant.Users.IsActive = true;

                            // Generate new password
                            string newPassword = PasswordGenerator.GeneratePassword();
                            applicant.Users.Password = newPassword; // Change to hash the password

                            // Send email notification
                            string subject = "Your Application is Approved";
                            string body = $"Dear {applicant.FirstName},<br><br>Your application has been approved. You can now log in to the system using the following credentials:<br><br>Email: {applicant.Users.Email}<br>Password: {newPassword}<br><br>Please change your password after logging in.<br><br>Best regards,<br>Admission Team";
                            await NotifyClass.SendEmailAsync(applicant.Users.Email, subject, body);
                        }

                        db.SaveChanges();
                        MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        _isEditing = false;
                        ToggleEditMode(_isEditing);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Абитуриент не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                // Попытка сохранить изменения
                SaveChangesButton_Click(sender, e);
            }
            else
            {
                _isEditing = true;
                ToggleEditMode(_isEditing);
            }
        }

        private void LoadApplicantData(int applicantId)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var applicant = db.Applicants
                    .Include("Users")
                    .Include("EducationalLevels")
                    .Include("ProgramTypes.EducationForms")
                    .Include("ApplicationStatus")
                    .FirstOrDefault(a => a.ApplicantID == applicantId);

                if (applicant != null)
                {
                    // Установка текстовых полей
                    FirstNameTextBox.Text = applicant.FirstName;
                    LastNameTextBox.Text = applicant.LastName;
                    MiddleNameTextBox.Text = applicant.MiddleName;
                    BirthDatePicker.SelectedDate = applicant.DateOfBirth;
                    EmailTextBox.Text = applicant.Users.Email; // Предполагаем, что Email также нужно загрузить
                    PhoneNumberTextBox.Text = applicant.PhoneNumber;
                    RegistrationAddressTextBox.Text = applicant.RegistrationAddress;
                    ResidentialAddressTextBox.Text = applicant.ResidentialAddress;

                    GenderComboBox.Text = applicant.Gender;

                    EducationLevelComboBox.SelectedValue = applicant.EducationalLevelID;
                    FieldOfStudyComboBox.SelectedValue = applicant.ProgramTypeID;
                    EducationFormComboBox.SelectedValue = applicant.ProgramTypes.EducationFormID;
                    ApplicationStatusComboBox.SelectedValue = applicant.ApplicationStatusID;

                    LoadData.LoadEducationalLevels(EducationLevelComboBox, applicant.EducationalLevelID);
                    LoadData.LoadEducationForm(EducationFormComboBox, applicant.ProgramTypes.EducationFormID);
                    LoadData.LoadProgramTypes(FieldOfStudyComboBox, applicant.ProgramTypeID);
                    LoadData.LoadApplicationStatuses(ApplicationStatusComboBox, applicant.ApplicationStatusID);
                }
                else
                {
                    MessageBox.Show("Профиль абитуриента не найден.");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadApplicantData(_applicant.ApplicantID);
        }

        private void EmailTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
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

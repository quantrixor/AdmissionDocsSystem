using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.ViewModel;
using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
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
                        applicant.Users.Email = EmailTextBox.Text; // Предположим, что пользователь может менять email
                        applicant.RegistrationAddress = RegistrationAddressTextBox.Text;
                        applicant.ResidentialAddress = ResidentialAddressTextBox.Text;

                        int newStatusId = Convert.ToInt32(ApplicationStatusComboBox.SelectedValue);

                        bool statusChangedToApproved = newStatusId != applicant.ApplicationStatusID && newStatusId == 2;
                        bool statusChangedToRejected = newStatusId != applicant.ApplicationStatusID && newStatusId == 3;
                        bool statusChangedToDocumentsRequired = newStatusId != applicant.ApplicationStatusID && newStatusId == 4;

                        applicant.ApplicationStatusID = newStatusId;

                        if (statusChangedToApproved)
                        {
                            // Обновить дополнительные свойства
                            applicant.IsConfirmed = true;
                            applicant.Users.IsActive = true;

                            // Создать новый пароль
                            string newPassword = PasswordGenerator.GeneratePassword();
                            applicant.Users.Password = newPassword; // Hash the password if needed

                            // Отправить уведомление по электронной почте для одобрения
                            string subject = "Ваша заявка одобрена";
                            string body = $"Дорогой {applicant.FirstName},<br><br>Ваша заявка одобрена. Теперь вы можете войти в систему, используя следующие учетные данные:<br><br>Email: {applicant.Users.Email}<br>Password: {newPassword}<br><br>Пожалуйста, смените пароль после входа.<br><br>С уважением,<br>Команда приема";
                            await NotifyClass.SendEmailAsync(applicant.Users.Email, subject, body);
                        }
                        else if (statusChangedToRejected)
                        {
                            // Отправить уведомление по электронной почте об отказе
                            string subject = "Ваша заявка отклонена";
                            string body = $"Дорогой {applicant.FirstName},<br><br>К сожалению, Ваша заявка была отклонена. Пожалуйста, свяжитесь с нами для получения дополнительной информации.<br><br>С уважением,<br>Команда приема";
                            await NotifyClass.SendEmailAsync(applicant.Users.Email, subject, body);
                        }
                        else if (statusChangedToDocumentsRequired)
                        {
                            // Отправьте уведомление по электронной почте о необходимости дополнительных документов
                            string subject = "Требуются дополнительные документы";
                            string body = $"Дорогой {applicant.FirstName},<br><br>Для продолжения обработки вашей заявки необходимы дополнительные документы. Пожалуйста, предоставьте необходимые документы как можно скорее.<br><br>С уважением,<br>Команда приема";
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
                    EmailTextBox.Text = applicant.Users.Email;
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

        private void DownloadDocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            var applicantId = _applicant.ApplicantID;
            using (var db = new AdmissionDocsSystemEntities())
            {
                var documents = db.Documents.Where(d => d.ApplicantID == applicantId).ToList();
                if (documents.Any())
                {
                    string tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempFolderPath);

                    string zipPath = Path.Combine(tempFolderPath, $"Applicant_{applicantId}_Documents.zip");

                    using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {
                        foreach (var doc in documents)
                        {
                            string entryName = $"{doc.DocumentType}.pdf";
                            var zipEntry = zipArchive.CreateEntry(entryName);
                            using (var entryStream = zipEntry.Open())
                            {
                                entryStream.Write(doc.DocumentContent, 0, doc.DocumentContent.Length);
                            }
                        }
                    }

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        FileName = $"Applicant_{applicantId}_Documents.zip",
                        Filter = "ZIP files (*.zip)|*.zip",
                        Title = "Save Documents As"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.Copy(zipPath, saveFileDialog.FileName, true);
                        MessageBox.Show("Документы успешно сохранены.");
                    }

                    Directory.Delete(tempFolderPath, true);
                }
                else
                {
                    MessageBox.Show("Нет документов для скачивания.");
                }
            }
        }
    }
}

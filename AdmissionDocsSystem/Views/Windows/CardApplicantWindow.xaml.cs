using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.ViewModel;
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

            // Изменение текста кнопки в зависимости от состояния
            EditButton.Content = isEditing ? "Сохранить изменения" : "Редактировать";
        }

        private bool _isEditing = false;

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
                return;

            try
            {
                using (var db = new AdmissionDocsSystemEntities())
                {
                    var applicantId = _applicant.ApplicantID; // Предполагаем, что ID текущего абитуриента уже загружен
                    var applicant = db.Applicants.FirstOrDefault(a => a.ApplicantID == applicantId);
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

                        db.SaveChanges();
                        MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        _isEditing = false;
                        ToggleEditMode(_isEditing);
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
    }
}

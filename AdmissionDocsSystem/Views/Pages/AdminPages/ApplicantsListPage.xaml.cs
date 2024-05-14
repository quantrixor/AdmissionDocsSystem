using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.Views.Windows;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdmissionDocsSystem.Views.Pages.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для ApplicantsListPage.xaml
    /// </summary>
    public partial class ApplicantsListPage : Page
    {
        public ObservableCollection<ApplicationStatus> ApplicationStatuses { get; set; }
        public ObservableCollection<ApplicantViewModel> Applicants { get; set; }

        public ApplicantsListPage()
        {
            InitializeComponent();
           
        }
        private void LoadApplicationStatuses()
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                ApplicationStatuses = new ObservableCollection<ApplicationStatus>(db.ApplicationStatus.ToList());
                this.DataContext = this; // Обновляем DataContext для привязки статусов
            }
        }
        private void LoadApplicantData()
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var applicants = db.Applicants
                    .Include("Users")
                    .Include("EducationalLevels")
                    .Include("ProgramTypes.Specialties")
                    .Include("ProgramTypes.EducationForms")
                    .Include("ApplicationStatus")
                    .Where(a => a.Users.Roles.RoleName == "Applicant")
                    .Select(a => new ApplicantViewModel
                    {
                        ApplicantID = a.ApplicantID,
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        MiddleName = a.MiddleName,
                        DateOfBirth = a.DateOfBirth,
                        Email = a.Users.Email,
                        PhoneNumber = a.PhoneNumber,
                        RegistrationAddress = a.RegistrationAddress,
                        ResidentialAddress = a.ResidentialAddress,
                        EducationalLevel = a.EducationalLevels.Description,
                        ProgramType = a.ProgramTypes.Description,
                        ProgramTypeCode = a.ProgramTypes.Specialties.SpecialtyCode,
                        EducationForm = a.ProgramTypes.EducationForms.FormDescription,
                        ApplicationStatus = a.ApplicationStatus.StatusDescription,
                        ApplicationStatusID = a.ApplicationStatus.ApplicationStatusID,
                        IsConfirmend = a.IsConfirmed ?? false // Проверяем на null
                    })
                    .ToList();

                Applicants = new ObservableCollection<ApplicantViewModel>(applicants);
                ApplicantsDataGrid.ItemsSource = Applicants;
            }
        }

        private void ApplicantsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApplicantsDataGrid.SelectedItem is ApplicantViewModel selectedApplicant)
            {
                var detailsWindow = new CardApplicantWindow(selectedApplicant);
                detailsWindow.Show();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadApplicantData();
            LoadApplicationStatuses();
        }

        private void UpdateData_Click(object sender, RoutedEventArgs e)
        {
            Page_Loaded(sender, e);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicantsDataGrid.SelectedItem is ApplicantViewModel selectedApplicant)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого абитуриента и все связанные данные?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new AdmissionDocsSystemEntities())
                        {
                            // Find the applicant by ID
                            var applicant = db.Applicants.Include("Users").Include("Documents").FirstOrDefault(a => a.ApplicantID == selectedApplicant.ApplicantID);

                            if (applicant != null)
                            {
                                // Delete related documents
                                db.Documents.RemoveRange(applicant.Documents);

                                // Delete user
                                db.Users.Remove(applicant.Users);

                                // Delete applicant
                                db.Applicants.Remove(applicant);

                                db.SaveChanges();

                                MessageBox.Show("Абитуриент и все связанные данные успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Refresh the data grid after deletion
                                LoadApplicantData();
                            }
                            else
                            {
                                MessageBox.Show("Абитуриент не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка при удалении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите абитуриента для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}


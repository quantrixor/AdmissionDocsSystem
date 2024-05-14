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
            LoadApplicantData();
            LoadApplicationStatuses();
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
    }
}


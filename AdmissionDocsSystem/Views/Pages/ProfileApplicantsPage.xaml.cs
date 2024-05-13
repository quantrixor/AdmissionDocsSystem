using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.Views.Windows;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdmissionDocsSystem.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProfileApplicantsPage.xaml
    /// </summary>
    public partial class ProfileApplicantsPage : Page
    {
        private Users _currentUser;
        public ProfileApplicantsPage(Users currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        private void LoadApplicantData()
        {
            int currentUserId = _currentUser.UserID;
            using (var db = new AdmissionDocsSystemEntities())
            {
                var applicant = db.Applicants
                    .Include("Users")
                    .Include("EducationalLevels")
                    .Include("EducationForms")
                    .Include("ApplicationStatus")
                    .Where(a => a.UserID == currentUserId)
                    .Select(a => new ApplicantViewModel
                    {
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
                        EducationForm = a.ProgramTypes.EducationForms.FormDescription,
                        ApplicationStatus = a.ApplicationStatus.StatusDescription
                    })
                    .FirstOrDefault();

                if (applicant != null)
                {
                    this.DataContext = applicant;
                }
                else
                {
                    MessageBox.Show("Профиль абитуриента не найден.");
                }
            }
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadApplicantData();
        }

        private void CloseApplicantsMainWindow()
        {
            // Этот код предполагает, что страница содержится внутри Frame, который находится в ApplicantsMainWindow
            var applicantsMainWindow = Window.GetWindow(this);

            // Проверяем, не равен ли mainWindow null, и является ли он экземпляром MainWindow
            if (applicantsMainWindow != null && applicantsMainWindow is ApplicantsMainWindow)
            {
                applicantsMainWindow.Close();
            }
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            SignInWindow signInWindow = new SignInWindow();
            signInWindow.Show();
            CloseApplicantsMainWindow();
        }
    }
}

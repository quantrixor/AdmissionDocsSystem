using AdmissionDocsSystem.Model;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdmissionDocsSystem.ViewModel
{
    internal static class LoadData
    {
        private static AdmissionDocsSystemEntities context = new AdmissionDocsSystemEntities();

        public static void LoadEducationalLevels(ComboBox educationComboBox, int? selectedId = null)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var levels = db.EducationalLevels.ToList();
                educationComboBox.ItemsSource = levels;
                educationComboBox.DisplayMemberPath = "Description";
                educationComboBox.SelectedValuePath = "EducationalLevelID";
                educationComboBox.SelectedValue = selectedId; // Установка выбранного ID
            }
        }

        // Подобным образом обновите и другие ComboBox:
        public static void LoadProgramTypes(ComboBox fieldOfStudyComboBox, int? selectedId = null)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var programs = db.ProgramTypes.ToList();
                fieldOfStudyComboBox.ItemsSource = programs;
                fieldOfStudyComboBox.DisplayMemberPath = "Description";
                fieldOfStudyComboBox.SelectedValuePath = "ProgramTypeID";
                fieldOfStudyComboBox.SelectedValue = selectedId; // Установка выбранного ID
            }
        }

        public static void LoadEducationForm(ComboBox educationFormComboBox, int? selectedId = null)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var forms = db.EducationForms.ToList();
                educationFormComboBox.ItemsSource = forms;
                educationFormComboBox.DisplayMemberPath = "FormDescription";
                educationFormComboBox.SelectedValuePath = "EducationFormID";
                educationFormComboBox.SelectedValue = selectedId; // Установка выбранного ID
            }
        }

        public static void LoadApplicationStatuses(ComboBox ApplicationStatusComboBox, int? selectedId = null)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var statuses = db.ApplicationStatus.ToList();
                ApplicationStatusComboBox.ItemsSource = statuses;
                ApplicationStatusComboBox.DisplayMemberPath = "StatusDescription";
                ApplicationStatusComboBox.SelectedValuePath = "ApplicationStatusID";
                ApplicationStatusComboBox.SelectedValue = selectedId;  // Установите значение по умолчанию, если нужно
            }
        }
    }
}

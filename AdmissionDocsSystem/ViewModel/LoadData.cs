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

        public static void LoadEducationalLevels(ComboBox educationComboBox)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                educationComboBox.ItemsSource = context.EducationalLevels.ToList();
                educationComboBox.DisplayMemberPath = "Description"; // Указываете, какое свойство отображать
                educationComboBox.SelectedValuePath = "EducationalLevelID"; // Какое свойство является значением
            }
        }

        public static void LoadProgramTypes(ComboBox fieldOfStudyComboBox)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                fieldOfStudyComboBox.ItemsSource = db.ProgramTypes.ToList();
                fieldOfStudyComboBox.DisplayMemberPath = "Description";
                fieldOfStudyComboBox.SelectedValuePath = "ProgramTypeID";
            }

        }

        public static void LoadEducationForm(ComboBox educationFormComboBox)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                educationFormComboBox.ItemsSource = db.EducationForms.ToList();
                educationFormComboBox.DisplayMemberPath = "FormDescription";
                educationFormComboBox.SelectedValuePath = "EducationFormID";
            }
        }
    }
}

using AdmissionDocsSystem.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace AdmissionDocsSystem.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для SubmissionDocumentsPage.xaml
    /// </summary>
    public partial class SubmissionDocumentsPage : Page
    {
        private List<string> selectedFiles;
        private Users _currentUser;
        public SubmissionDocumentsPage(Users currentUser)
        {
            InitializeComponent();
            selectedFiles = new List<string>();
            _currentUser = currentUser;
        }

        private void SelectFilesButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFiles = openFileDialog.FileNames.ToList();
                SelectedFilesListBox.ItemsSource = selectedFiles;
            }
        }

        private void UploadFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите файлы для загрузки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new AdmissionDocsSystemEntities())
                {
                    foreach (var filePath in selectedFiles)
                    {
                        byte[] fileData = File.ReadAllBytes(filePath);
                        string fileName = System.IO.Path.GetFileName(filePath);

                        var document = new Documents
                        {
                            ApplicantID = _currentUser.UserID,
                            DocumentType = fileName,
                            DocumentContent = fileData,
                            IsVerified = false
                        };

                        db.Documents.Add(document);
                    }

                    db.SaveChanges();
                    MessageBox.Show("Файлы успешно загружены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    selectedFiles.Clear();
                    SelectedFilesListBox.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке файлов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

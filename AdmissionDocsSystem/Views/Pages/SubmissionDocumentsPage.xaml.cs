using AdmissionDocsSystem.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
                foreach (var fileName in openFileDialog.FileNames)
                {
                    if (!selectedFiles.Contains(fileName))
                    {
                        selectedFiles.Add(fileName);
                    }
                }
                SelectedFilesListBox.ItemsSource = null;
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}

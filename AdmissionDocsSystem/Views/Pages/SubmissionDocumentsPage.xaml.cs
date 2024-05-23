using AdmissionDocsSystem.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdmissionDocsSystem.Views.Pages
{
    public partial class SubmissionDocumentsPage : Page
    {
        private List<DocumentViewModel> selectedFiles;
        private Users _currentUser;
        public List<string> DocumentTypes { get; } = new List<string>
        {
            "Копия паспорта",
            "Документ об образовании",
            "Фотография (6 шт.)",
            "Согласие на обработку персональных данных",
            "Медицинская справка по форме 086/у",
            "Копия медицинского полиса",
            "Копия свидетельства (ИНН)",
            "Копия СНИЛС",
            "Приписной/военный билет",
            "Заявление на проживание в общежитии (для иногородних)"
        };

        public SubmissionDocumentsPage(Users currentUser)
        {
            InitializeComponent();
            selectedFiles = new List<DocumentViewModel>();
            _currentUser = currentUser;
            DataContext = this; // Устанавливаем DataContext для привязки
            LoadExistingDocuments();
        }

        private void LoadExistingDocuments()
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var existingDocuments = db.Documents
                    .Where(d => d.ApplicantID == _currentUser.UserID)
                    .Select(d => new DocumentViewModel
                    {
                        DocumentID = d.DocumentID,
                        DocumentType = d.DocumentType,
                        IsVerified = (bool)d.IsVerified
                    }).ToList();

                DocumentsDataGrid.ItemsSource = existingDocuments;
            }
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
                    if (!selectedFiles.Any(f => f.FilePath == fileName))
                    {
                        selectedFiles.Add(new DocumentViewModel { FilePath = fileName, SelectedDocumentType = DocumentTypes.First() });
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
                    bool hasDuplicate = false;

                    foreach (var file in selectedFiles)
                    {
                        byte[] fileData = File.ReadAllBytes(file.FilePath);

                        // Проверка на дублирование типов документов
                        if (db.Documents.Any(d => d.ApplicantID == _currentUser.UserID && d.DocumentType == file.SelectedDocumentType))
                        {
                            MessageBox.Show($"Документ с типом '{file.SelectedDocumentType}' уже существует для данного абитуриента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            hasDuplicate = true;
                            continue;
                        }

                        var document = new Documents
                        {
                            ApplicantID = _currentUser.UserID,
                            DocumentType = file.SelectedDocumentType,
                            DocumentContent = fileData,
                            IsVerified = false
                        };

                        db.Documents.Add(document);
                    }

                    if (!hasDuplicate)
                    {
                        db.SaveChanges();
                        MessageBox.Show("Файлы успешно загружены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        selectedFiles.Clear();
                        SelectedFilesListBox.ItemsSource = null;
                        LoadExistingDocuments(); // Обновить таблицу с загруженными документами
                    }
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

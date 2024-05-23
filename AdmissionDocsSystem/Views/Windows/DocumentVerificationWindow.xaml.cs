using AdmissionDocsSystem.Model;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace AdmissionDocsSystem.Views.Windows
{
    public partial class DocumentVerificationWindow : Window
    {
        private int _applicantID;

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

        public DocumentVerificationWindow(int applicantID, string applicantName)
        {
            InitializeComponent();
            DataContext = this; // Устанавливаем DataContext
            _applicantID = applicantID;
            OwnerInfoTextBlock.Text = $"Documents of {applicantName}";
            LoadDocuments(); // Загрузка документов при открытии окна
        }
        private void SaveDocuments_Click(object sender, RoutedEventArgs e)
        {
            var documents = DocumentsListBox.ItemsSource.Cast<DocumentViewModel>().ToList();
            bool hasDuplicate = false;

            using (var db = new AdmissionDocsSystemEntities())
            {
                foreach (var doc in documents)
                {
                    if (doc.DocumentID == 0) // Новый документ
                    {
                        // Проверка на дублирование типов документов
                        if (db.Documents.Any(d => d.ApplicantID == _applicantID && d.DocumentType == doc.SelectedDocumentType))
                        {
                            MessageBox.Show($"Документ с типом '{doc.SelectedDocumentType}' уже существует для данного абитуриента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            hasDuplicate = true;
                            continue;
                        }
                        db.Documents.Add(new Documents
                        {
                            ApplicantID = _applicantID,
                            DocumentType = doc.SelectedDocumentType,
                            DocumentContent = doc.DocumentContent,
                            IsVerified = doc.IsVerified
                        });
                    }
                    else // Существующий документ
                    {
                        var document = db.Documents.Find(doc.DocumentID);
                        if (document != null)
                        {
                            document.IsVerified = doc.IsVerified;
                            document.DocumentType = doc.SelectedDocumentType; // Сохраняем выбранный тип документа
                        }
                    }
                }

                // Если был дубликат, выходим из метода без сохранения изменений
                if (hasDuplicate)
                {
                    return;
                }

                db.SaveChanges();
            }
            MessageBox.Show("Документы успешно обновлены.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadDocuments();
        }
        private void LoadDocuments()
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var documents = db.Documents
                                  .Where(d => d.ApplicantID == _applicantID)
                                  .Select(d => new DocumentViewModel
                                  {
                                      DocumentID = d.DocumentID,
                                      DocumentType = d.DocumentType,
                                      IsVerified = d.IsVerified ?? false,
                                      SelectedDocumentType = d.DocumentType,
                                      DocumentContent = d.DocumentContent
                                  }).ToList();

                if (documents == null || !documents.Any())
                {
                    NoDocumentsTextBlock.Visibility = Visibility.Visible;
                    DocumentsListBox.Visibility = Visibility.Collapsed;
                    SaveButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DocumentsListBox.ItemsSource = documents;
                    NoDocumentsTextBlock.Visibility = Visibility.Collapsed;
                    DocumentsListBox.Visibility = Visibility.Visible;
                    SaveButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void AddDocument_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
            openFileDialog.Multiselect = true; // Разрешаем множественный выбор
            if (openFileDialog.ShowDialog() == true)
            {
                var documents = DocumentsListBox.ItemsSource as List<DocumentViewModel>;
                if (documents == null)
                {
                    documents = new List<DocumentViewModel>();
                    DocumentsListBox.ItemsSource = documents;
                }

                foreach (var filePath in openFileDialog.FileNames)
                {
                    var fileContent = File.ReadAllBytes(filePath);

                    // Проверка на дублирование типов документов в текущем списке
                    if (documents.Any(d => d.SelectedDocumentType == "Выберите тип"))
                    {
                        MessageBox.Show($"Вы уже добавили документ. Пожалуйста, выберите тип для всех документов перед добавлением новых.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    var newDocument = new DocumentViewModel
                    {
                        DocumentID = 0, // Временно, до сохранения в базу данных
                        DocumentType = "Выберите тип",
                        SelectedDocumentType = "Выберите тип",
                        IsVerified = false,
                        DocumentContent = fileContent
                    };
                    documents.Add(newDocument);
                }

                DocumentsListBox.Items.Refresh();
                DocumentsListBox.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Visible;
            }
        }
    }
}

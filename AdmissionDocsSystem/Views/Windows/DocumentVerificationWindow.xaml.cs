using AdmissionDocsSystem.Model;
using System.Collections.Generic;
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

        public DocumentVerificationWindow(int applicantID, List<DocumentViewModel> documents, string applicantName)
        {
            InitializeComponent();
            DataContext = this; // Устанавливаем DataContext
            _applicantID = applicantID;
            OwnerInfoTextBlock.Text = $"Documents of {applicantName}";
            if (documents == null || !documents.Any())
            {
                NoDocumentsTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                DocumentsListBox.ItemsSource = documents;
                DocumentsListBox.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Visible;
            }
        }

        private void SaveDocuments_Click(object sender, RoutedEventArgs e)
        {
            var documents = DocumentsListBox.ItemsSource.Cast<DocumentViewModel>().ToList();
            using (var db = new AdmissionDocsSystemEntities())
            {
                foreach (var doc in documents)
                {
                    var document = db.Documents.Find(doc.DocumentID);
                    if (document != null)
                    {
                        document.IsVerified = doc.IsVerified;
                        document.DocumentType = doc.SelectedDocumentType; // Сохраняем выбранный тип документа
                    }
                }
                db.SaveChanges();
            }

            MessageBox.Show("Documents updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}

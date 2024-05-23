using AdmissionDocsSystem.Model;
using AdmissionDocsSystem.Views.Windows;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;
using ClosedXML.Excel;
using System.Collections.Generic;

namespace AdmissionDocsSystem.Views.Pages.AdminPages
{
    public partial class ApplicantsListPage : System.Windows.Controls.Page
    {
        public ObservableCollection<ApplicationStatus> ApplicationStatuses { get; set; }
        public ObservableCollection<ApplicantViewModel> Applicants { get; set; } = new ObservableCollection<ApplicantViewModel>();

        public ObservableCollection<Specialties> Specialties { get; set; }

        public ApplicantsListPage()
        {
            InitializeComponent();
            ApplicationStatuses = new ObservableCollection<ApplicationStatus>();
            Applicants = new ObservableCollection<ApplicantViewModel>();
            Specialties = new ObservableCollection<Specialties>();
            this.Loaded += Page_Loaded; // Добавляем обработчик события Loaded
        }

        private void LoadApplicationStatuses()
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                ApplicationStatuses = new ObservableCollection<ApplicationStatus>(db.ApplicationStatus.ToList());
                this.DataContext = this; // Обновляем DataContext для привязки статусов
            }
        }

        private void LoadApplicantData(string searchText = "", int specialtyID = 0)
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var query = db.Applicants
                    .Include("Users")
                    .Include("EducationalLevels")
                    .Include("ProgramTypes.Specialties")
                    .Include("ProgramTypes.EducationForms")
                    .Include("ApplicationStatus")
                    .Include("Documents")
                    .Where(a => a.Users.Roles.RoleName == "Applicant");

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(a => a.FirstName.Contains(searchText) ||
                                             a.LastName.Contains(searchText) ||
                                             a.MiddleName.Contains(searchText) ||
                                             a.Users.Email.Contains(searchText) ||
                                             a.PhoneNumber.Contains(searchText));
                }

                if (specialtyID > 0)
                {
                    query = query.Where(a => a.ProgramTypes.Specialties.SpecialtyID == specialtyID);
                }

                var documentTypes = new List<string>
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

                var applicants = query.ToList().Select(a => new ApplicantViewModel
                {
                    ApplicantID = a.ApplicantID,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    MiddleName = a.MiddleName,
                    DateOfBirth = a.DateOfBirth,
                    Email = a.Users.Email,
                    PhoneNumber = a.PhoneNumber,
                    ProgramTypeCode = a.ProgramTypes.Specialties.SpecialtyCode,
                    EducationForm = a.ProgramTypes.EducationForms.FormDescription,
                    ApplicationStatus = a.ApplicationStatus.StatusDescription,
                    ApplicationStatusID = a.ApplicationStatus.ApplicationStatusID,
                    IsConfirmed = a.IsConfirmed ?? false,
                    Documents = a.Documents.Select(d => new DocumentViewModel
                    {
                        DocumentID = d.DocumentID,
                        DocumentType = d.DocumentType,
                        IsVerified = d.IsVerified ?? false,
                        SelectedDocumentType = d.DocumentType
                    }).ToList(),
                    DocumentStatuses = documentTypes.Select(dt => new DocumentStatusViewModel
                    {
                        DocumentType = dt,
                        IsUploaded = a.Documents.Any(d => d.DocumentType == dt && d.IsVerified.GetValueOrDefault())
                    }).ToList()
                }).ToList();

                if (Applicants == null)
                {
                    Applicants = new ObservableCollection<ApplicantViewModel>();
                }

                Applicants.Clear();
                foreach (var applicant in applicants)
                {
                    Applicants.Add(applicant);
                }

                if (ApplicantsDataGrid != null)
                {
                    ApplicantsDataGrid.ItemsSource = Applicants;
                }
            }
        }

        private void LoadSpecialties()
        {
            using (var db = new AdmissionDocsSystemEntities())
            {
                var specialties = db.Specialties.ToList();
                Specialties.Clear();
                SpecialtyComboBox.Items.Clear();

                // Добавление опции "Все специальности"
                SpecialtyComboBox.Items.Add(new ComboBoxItem { Content = "Все специальности", IsSelected = true });

                foreach (var specialty in specialties)
                {
                    Specialties.Add(specialty);
                    SpecialtyComboBox.Items.Add(new ComboBoxItem { Content = specialty.SpecialtyName, Tag = specialty.SpecialtyID });
                }
            }
        }

        private void ApplicantsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApplicantsDataGrid.SelectedItem is ApplicantViewModel selectedApplicant)
            {
                var detailsWindow = new CardApplicantWindow(selectedApplicant);
                detailsWindow.Show();
            }


            //if (ApplicantsDataGrid.SelectedItem is ApplicantViewModel selectedApplicant)
            //{
            //    var applicantName = $"{selectedApplicant.FirstName} {selectedApplicant.LastName} {selectedApplicant.MiddleName}";

            //    // Проверяем, что список документов не null и инициализирован
            //    var documents = selectedApplicant.Documents?.Select(d => new DocumentViewModel
            //    {
            //        DocumentID = d.DocumentID,
            //        DocumentType = d.DocumentType,
            //        IsVerified = d.IsVerified,
            //        SelectedDocumentType = d.DocumentType // Заполняем начальным значением
            //    }).ToList() ?? new List<DocumentViewModel>();

            //    var documentsWindow = new DocumentVerificationWindow(selectedApplicant.ApplicantID, documents, applicantName);
            //    documentsWindow.ShowDialog();
            //    // Перезагрузка данных для обновления статуса документов
            //    LoadApplicantData();
            //}
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LoadApplicantData();
                LoadApplicationStatuses();
                LoadSpecialties();
            });
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

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SpecialtyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string searchText = SearchBox.Text;
            int specialtyID = 0;

            if (SpecialtyComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is int id)
            {
                specialtyID = id;
            }

            LoadApplicantData(searchText, specialtyID);
        }
        private void ExportApplicantsToWord()
        {
            string filePath = "ApplicantsList.docx";
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                DocumentFormat.OpenXml.Wordprocessing.Body body = new DocumentFormat.OpenXml.Wordprocessing.Body();
                mainPart.Document.Append(body);

                // Заголовок документа
                DocumentFormat.OpenXml.Wordprocessing.Paragraph titleParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                    new DocumentFormat.OpenXml.Wordprocessing.Run(
                        new DocumentFormat.OpenXml.Wordprocessing.Text("Список абитуриентов")));
                titleParagraph.ParagraphProperties = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties(
                    new DocumentFormat.OpenXml.Wordprocessing.Justification() { Val = JustificationValues.Center });
                titleParagraph.ParagraphProperties.SpacingBetweenLines = new DocumentFormat.OpenXml.Wordprocessing.SpacingBetweenLines() { After = "200" };
                body.Append(titleParagraph);

                // Таблица данных абитуриентов
                DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                // Определение границ таблицы
                DocumentFormat.OpenXml.Wordprocessing.TableProperties tblProperties = new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
                    new DocumentFormat.OpenXml.Wordprocessing.TableBorders(
                        new DocumentFormat.OpenXml.Wordprocessing.TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new DocumentFormat.OpenXml.Wordprocessing.BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new DocumentFormat.OpenXml.Wordprocessing.LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new DocumentFormat.OpenXml.Wordprocessing.RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                    )
                );
                table.AppendChild(tblProperties);

                // Заголовок таблицы
                DocumentFormat.OpenXml.Wordprocessing.TableRow headerRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                string[] headers = { "ID", "Имя", "Фамилия", "Отчество", "Дата рождения", "Электронная почта", "Номер телефона", "Код обучения", "Форма обучения", "Статус заявки" };
                foreach (var header in headers)
                {
                    DocumentFormat.OpenXml.Wordprocessing.TableCell cell = new DocumentFormat.OpenXml.Wordprocessing.TableCell(
                        new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                            new DocumentFormat.OpenXml.Wordprocessing.Run(
                                new DocumentFormat.OpenXml.Wordprocessing.Text(header))));
                    cell.TableCellProperties = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" });
                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                // Данные абитуриентов
                foreach (var applicant in Applicants)
                {
                    DocumentFormat.OpenXml.Wordprocessing.TableRow row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                    row.Append(
                        CreateTableCell(applicant.ApplicantID.ToString()),
                        CreateTableCell(applicant.FirstName),
                        CreateTableCell(applicant.LastName),
                        CreateTableCell(applicant.MiddleName),
                        CreateTableCell(applicant.DateOfBirth.HasValue ? applicant.DateOfBirth.Value.ToShortDateString() : ""),
                        CreateTableCell(applicant.Email),
                        CreateTableCell(applicant.PhoneNumber),
                        CreateTableCell(applicant.ProgramTypeCode),
                        CreateTableCell(applicant.EducationForm),
                        CreateTableCell(applicant.ApplicationStatus)
                    );
                    table.Append(row);
                }

                body.Append(table);
                mainPart.Document.Save();
            }

            MessageBox.Show($"Данные успешно выгружены в файл {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private DocumentFormat.OpenXml.Wordprocessing.TableCell CreateTableCell(string text)
        {
            DocumentFormat.OpenXml.Wordprocessing.TableCell cell = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
            cell.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(text))));
            cell.TableCellProperties = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" });
            return cell;
        }


        private void ExportToWord_Click(object sender, RoutedEventArgs e)
        {
            ExportApplicantsToWord();
        }
        private void ExportApplicantsToExcel()
        {
            string filePath = "ApplicantsList.xlsx";

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Applicants");

                // Заголовки
                string[] headers = { "ID", "Имя", "Фамилия", "Отчество", "Дата рождения", "Электронная почта", "Номер телефона", "Код обучения", "Форма обучения", "Статус заявки" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                }

                // Данные абитуриентов
                for (int i = 0; i < Applicants.Count; i++)
                {
                    var applicant = Applicants[i];
                    worksheet.Cell(i + 2, 1).Value = applicant.ApplicantID;
                    worksheet.Cell(i + 2, 2).Value = applicant.FirstName;
                    worksheet.Cell(i + 2, 3).Value = applicant.LastName;
                    worksheet.Cell(i + 2, 4).Value = applicant.MiddleName;
                    worksheet.Cell(i + 2, 5).Value = applicant.DateOfBirth.HasValue ? applicant.DateOfBirth.Value.ToShortDateString() : "";
                    worksheet.Cell(i + 2, 6).Value = applicant.Email;
                    worksheet.Cell(i + 2, 7).Value = applicant.PhoneNumber;
                    worksheet.Cell(i + 2, 8).Value = applicant.ProgramTypeCode;
                    worksheet.Cell(i + 2, 9).Value = applicant.EducationForm;
                    worksheet.Cell(i + 2, 10).Value = applicant.ApplicationStatus;
                }

                workbook.SaveAs(filePath);
            }

            MessageBox.Show($"Данные успешно выгружены в файл {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportToExel_Click(object sender, RoutedEventArgs e)
        {
            ExportApplicantsToExcel();
        }

        private void StatusDocuments_Click(object sender, RoutedEventArgs e)
        {
            ApplicantsDataGrid.Visibility = Visibility.Collapsed;
            DocumentsDataGrid.Visibility = Visibility.Visible;
            DocumentsDataGrid.ItemsSource = Applicants;
        }
    }
}

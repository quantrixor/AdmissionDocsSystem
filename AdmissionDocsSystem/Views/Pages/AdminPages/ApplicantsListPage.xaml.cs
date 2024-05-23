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
using Word = DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;

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
            // Создание диалогового окна сохранения файла
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word files (*.docx)|*.docx|All files (*.*)|*.*",
                FileName = "ApplicantsList.docx"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                // Получение пути к файлу из диалогового окна
                string filePath = saveFileDialog.FileName;
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Word.Document();
                    Word.Body body = mainPart.Document.AppendChild(new Word.Body());

                    // Заголовок документа
                    Word.Paragraph titleParagraph = new Word.Paragraph(new Word.Run(new Word.Text("Список абитуриентов")));
                    titleParagraph.ParagraphProperties = new Word.ParagraphProperties(new Word.Justification() { Val = Word.JustificationValues.Center });
                    titleParagraph.ParagraphProperties.SpacingBetweenLines = new Word.SpacingBetweenLines() { After = "200" };
                    body.Append(titleParagraph);

                    // Создание таблицы
                    Word.Table table = new Word.Table();

                    // Определение границ таблицы
                    Word.TableProperties tblProperties = new Word.TableProperties(
                        new Word.TableBorders(
                            new Word.TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new Word.BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new Word.LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new Word.RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new Word.InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new Word.InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                        )
                    );
                    table.AppendChild(tblProperties);

                    // Заголовок таблицы
                    Word.TableRow headerRow = new Word.TableRow();
                    string[] headers = { "№ п/п", "номер заявления", "копия паспорта", "документ об образовании (аттестат, диплом, свидетельство об обучении, иной документ)", "фотографии (6 шт.)", "согласие на обработку персональных данных", "медицинская справка по форме 086/у", "копия медицинского полиса", "копия свидетельства (ИНН)", "копия СНИЛС", "приписной/военный билет", "заявление на проживание в общежитии (для иногородних)" };
                    foreach (var header in headers)
                    {
                        Word.TableCell cell = new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text(header))));
                        cell.TableCellProperties = new Word.TableCellProperties(new Word.TableCellWidth() { Type = Word.TableWidthUnitValues.Dxa, Width = "2400" });
                        headerRow.Append(cell);
                    }
                    table.Append(headerRow);

                    // Данные абитуриентов
                    int rowIndex = 1;
                    foreach (var applicant in Applicants)
                    {
                        Word.TableRow row = new Word.TableRow();
                        row.Append(
                            CreateTableCell(rowIndex.ToString()), // № п/п
                            CreateTableCell(applicant.ApplicantID.ToString()), // номер заявления
                            CreateTableCell(GetDocumentStatus(applicant, "Копия паспорта")), // копия паспорта
                            CreateTableCell(GetDocumentStatus(applicant, "Документ об образовании")), // документ об образовании
                            CreateTableCell(GetDocumentStatus(applicant, "Фотография (6 шт.)")), // фотографии (6 шт.)
                            CreateTableCell(GetDocumentStatus(applicant, "Согласие на обработку персональных данных")), // согласие на обработку персональных данных
                            CreateTableCell(GetDocumentStatus(applicant, "Медицинская справка по форме 086/у")), // медицинская справка по форме 086/у
                            CreateTableCell(GetDocumentStatus(applicant, "Копия медицинского полиса")), // копия медицинского полиса
                            CreateTableCell(GetDocumentStatus(applicant, "Копия свидетельства (ИНН)")), // копия свидетельства (ИНН)
                            CreateTableCell(GetDocumentStatus(applicant, "Копия СНИЛС")), // копия СНИЛС
                            CreateTableCell(GetDocumentStatus(applicant, "Приписной/военный билет")), // приписной/военный билет
                            CreateTableCell(GetDocumentStatus(applicant, "Заявление на проживание в общежитии (для иногородних)")) // заявление на проживание в общежитии (для иногородних)
                        );
                        table.Append(row);
                        rowIndex++;
                    }

                    body.Append(table);
                    mainPart.Document.Save();
                }

                MessageBox.Show($"Данные успешно выгружены в файл {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private Word.TableCell CreateTableCell(string text)
        {
            Word.TableCell cell = new Word.TableCell();
            cell.Append(new Word.Paragraph(new Word.Run(new Word.Text(text))));
            cell.TableCellProperties = new Word.TableCellProperties(new Word.TableCellWidth() { Type = Word.TableWidthUnitValues.Dxa, Width = "2400" });
            return cell;
        }

        private string GetDocumentStatus(ApplicantViewModel applicant, string documentType)
        {
            var document = applicant.Documents.FirstOrDefault(d => d.DocumentType == documentType);
            return document != null && document.IsVerified ? "+" : "";
        }

        private void ExportToWord_Click(object sender, RoutedEventArgs e)
        {
            ExportApplicantsToWord();
        }
        private void ExportApplicantsToExcel()
        {
            // Создание диалогового окна сохранения файла
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                FileName = "ApplicantsList.xlsx"
            };

            // Если пользователь выбрал место сохранения и имя файла
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Applicants");

                    // Заголовки
                    string[] headers = {
                "№ п/п", "номер заявления", "Фамилия", "Имя", "Отчество", "Дата рождения",
                "Электронная почта", "Номер телефона", "Код обучения", "Форма обучения", "Статус заявки",
                "Копия паспорта", "Документ об образовании", "Фотография (6 шт.)",
                "Согласие на обработку персональных данных", "Медицинская справка по форме 086/у",
                "Копия медицинского полиса", "Копия свидетельства (ИНН)", "Копия СНИЛС",
                "Приписной/военный билет", "Заявление на проживание в общежитии (для иногородних)"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    // Данные абитуриентов
                    for (int i = 0; i < Applicants.Count; i++)
                    {
                        var applicant = Applicants[i];
                        int row = i + 2;

                        worksheet.Cell(row, 1).Value = (i + 1); // № п/п
                        worksheet.Cell(row, 2).Value = applicant.ApplicantID; // номер заявления
                        worksheet.Cell(row, 3).Value = applicant.LastName;
                        worksheet.Cell(row, 4).Value = applicant.FirstName;
                        worksheet.Cell(row, 5).Value = applicant.MiddleName;
                        worksheet.Cell(row, 6).Value = applicant.DateOfBirth.HasValue ? applicant.DateOfBirth.Value.ToShortDateString() : "";
                        worksheet.Cell(row, 7).Value = applicant.Email;
                        worksheet.Cell(row, 8).Value = applicant.PhoneNumber;
                        worksheet.Cell(row, 9).Value = applicant.ProgramTypeCode;
                        worksheet.Cell(row, 10).Value = applicant.EducationForm;
                        worksheet.Cell(row, 11).Value = applicant.ApplicationStatus;

                        worksheet.Cell(row, 12).Value = GetDocumentStatus(applicant, "Копия паспорта");
                        worksheet.Cell(row, 13).Value = GetDocumentStatus(applicant, "Документ об образовании");
                        worksheet.Cell(row, 14).Value = GetDocumentStatus(applicant, "Фотография (6 шт.)");
                        worksheet.Cell(row, 15).Value = GetDocumentStatus(applicant, "Согласие на обработку персональных данных");
                        worksheet.Cell(row, 16).Value = GetDocumentStatus(applicant, "Медицинская справка по форме 086/у");
                        worksheet.Cell(row, 17).Value = GetDocumentStatus(applicant, "Копия медицинского полиса");
                        worksheet.Cell(row, 18).Value = GetDocumentStatus(applicant, "Копия свидетельства (ИНН)");
                        worksheet.Cell(row, 19).Value = GetDocumentStatus(applicant, "Копия СНИЛС");
                        worksheet.Cell(row, 20).Value = GetDocumentStatus(applicant, "Приписной/военный билет");
                        worksheet.Cell(row, 21).Value = GetDocumentStatus(applicant, "Заявление на проживание в общежитии (для иногородних)");
                    }

                    workbook.SaveAs(filePath);
                }

                MessageBox.Show($"Данные успешно выгружены в файл {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void ExportToExel_Click(object sender, RoutedEventArgs e)
        {
            ExportApplicantsToExcel();
        }

        private void StatusDocuments_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicantsDataGrid.Visibility == Visibility.Visible)
            {
                ApplicantsDataGrid.Visibility = Visibility.Collapsed;
                DocumentsDataGrid.Visibility = Visibility.Visible;
                StatusDocumentsButton.Content = "Показать абитуриентов";
                DocumentsDataGrid.ItemsSource = Applicants;
            }
            else
            {
                ApplicantsDataGrid.Visibility = Visibility.Visible;
                DocumentsDataGrid.Visibility = Visibility.Collapsed;
                StatusDocumentsButton.Content = "Показать документы";
            }
        }

    }
}

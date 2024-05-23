using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmissionDocsSystem.Model
{
    public class ApplicantViewModel
    {
        public int ApplicantID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string RegistrationAddress { get; set; }
        public string ResidentialAddress { get; set; }
        public string EducationalLevel { get; set; }
        public string ProgramType { get; set; }
        public string EducationForm { get; set; }
        public string ApplicationStatus { get; set; }
        public string ProgramTypeCode { get; set; }
        public int ApplicationStatusID { get; set; }
        public bool IsConfirmend { get; set; }
        public List<DocumentStatusViewModel> DocumentStatuses { get; set; }

        public List<DocumentViewModel> Documents { get; set; }
        public bool IsConfirmed { get; internal set; }
    }
    public class DocumentViewModel
    {
        public int DocumentID { get; set; }
        public string DocumentType { get; set; }
        public bool IsVerified { get; set; }
        public string SelectedDocumentType { get; set; }
        public byte[] DocumentContent { get; set; }
        public string FilePath { get; set; }
    }


    public class DocumentStatusViewModel
    {
        public string DocumentType { get; set; }
        public bool IsUploaded { get; set; }
    }

}

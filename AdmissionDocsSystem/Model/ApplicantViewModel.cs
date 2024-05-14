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
    }

}

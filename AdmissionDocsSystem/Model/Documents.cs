//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AdmissionDocsSystem.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Documents
    {
        public int DocumentID { get; set; }
        public Nullable<int> ApplicantID { get; set; }
        public string DocumentType { get; set; }
        public byte[] DocumentContent { get; set; }
        public Nullable<bool> IsVerified { get; set; }
    
        public virtual Applicants Applicants { get; set; }
    }
}
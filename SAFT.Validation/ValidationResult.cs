namespace SAFT.Validation
{
    public enum SAFTValidationSeverity
    {
        Error,
        Warning,
        Info
    }

    public enum SAFTValidationType
    {
        CrossReference,
        Consistency,
        RequiredField,
        Enum,
        Duplicate,
        BusinessRule,
        Schema
    }

    public class SAFTValidationResult
    {
        public string Field { get; set; }
        public string Message { get; set; }
        public string Section { get; set; }
        public SAFTValidationSeverity Severity { get; set; } = SAFTValidationSeverity.Error;
        public string MessageCode { get; set; } // For localization
        public string DocumentNumber { get; set; } // Context: e.g., InvoiceNo, DocumentNumber
        public int? LineNumber { get; set; } // Context: e.g., line number in document
        public SAFTValidationType Type { get; set; } = SAFTValidationType.CrossReference;
    }
} 
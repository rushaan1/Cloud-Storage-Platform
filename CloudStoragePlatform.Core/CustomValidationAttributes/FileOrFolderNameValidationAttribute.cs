using System.ComponentModel.DataAnnotations;

namespace Cloud_Storage_Platform.CustomValidationAttributes
{
    public class FileOrFolderNameValidationAttribute : ValidationAttribute
    {
        private string _errorMsg;
        public FileOrFolderNameValidationAttribute(string errorMsg) 
        {
            _errorMsg = errorMsg;
        }

        public FileOrFolderNameValidationAttribute() 
        {
            _errorMsg = "Invalid name";
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                char[] nameChars = ((string)value).ToCharArray();
                char[] invalidChars = Path.GetInvalidFileNameChars();
                foreach (char c in invalidChars)
                {
                    if (nameChars.Contains(c))
                    {
                        return new ValidationResult(_errorMsg);
                    }
                }
                return ValidationResult.Success;
            }
            else 
            {
                return null;
            }
        }
    }
}

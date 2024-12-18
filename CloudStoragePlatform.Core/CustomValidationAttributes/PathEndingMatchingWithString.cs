using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.CustomValidationAttributes
{
    public class PathEndingMatchingWithString : ValidationAttribute
    {
        private string _errorMsg;
        private string _matchWithPropertyName;
        public PathEndingMatchingWithString(string matchWithPropertyName, string errorMsg)
        {
            _errorMsg = errorMsg;
            _matchWithPropertyName = matchWithPropertyName;
        }

        public PathEndingMatchingWithString(string matchWithPropertyName)
        {
            _errorMsg = "Invalid path";
            _matchWithPropertyName = matchWithPropertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx) 
        {
            if (value != null)
            {
                string path = (string)value;
                PropertyInfo? otherProperty = ctx.ObjectType.GetProperty(_matchWithPropertyName);
                if (otherProperty == null) 
                {
                    return null;
                }
                string? ending = (string?)otherProperty.GetValue(ctx.ObjectInstance);
                if (ending == null) 
                {
                    return null;
                }
                if (path.EndsWith(ending) == false)
                {
                    return new ValidationResult(_errorMsg);
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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO.AuthDTO
{
    public class EmailVerificationDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required")]
        public string OTP { get; set; } = string.Empty;

        [Required]
        public bool RememberMe { get; set; } = false;
        [Required]
        public Guid UserId { get; set; }
    }
}

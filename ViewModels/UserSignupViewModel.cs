using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using System;

namespace EXPEDIT.Share.ViewModels
{
    [Flags]
    public enum SignupResponse : uint
    {
        Unknown = 0,
        Valid = 1,
        NoCaptcha = 2,
        BadCaptcha = 4,
        BadUsernamePassword = 8
    }
    
    public class UserSignupViewModel : UserLoginViewModel
    {
        public Guid? CaptchaCookie { get; set; }

        public string CaptchaKey { get; set; }

        public SignupResponse Response { get; set; }

        public bool? IsValid { get; set; }
    }
}
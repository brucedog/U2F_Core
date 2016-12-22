using System.ComponentModel.DataAnnotations;

namespace U2F.Demo.ViewModel
{
    public class CompleteRegisterViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
        
        [Display(Name = "Challenge")]
        public string Challenge { get; set; }

        [Display(Name = "Version")]
        public string Version { get; set; }

        [Display(Name = "App ID")]
        public string AppId { get; set; }

        [Display(Name = "Device Response")]
        public string DeviceResponse { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace U2F.Demo.ViewModel
{
    public class StartLoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
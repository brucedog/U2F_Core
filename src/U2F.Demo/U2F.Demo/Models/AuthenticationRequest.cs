using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace U2F.Demo.Models
{
    public class AuthenticationRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string KeyHandle { get; set; }

        [Required]
        public string Challenge { get; set; }

        [Required]
        [StringLength(200)]
        public string AppId { get; set; }

        [Required]
        [StringLength(50)]
        public string Version { get; set; }
    }
}
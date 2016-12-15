using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace U2F.Demo.Models
{
    public class Device
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        [Required]
        public byte[] KeyHandle { get; set; }

        [Required]
        public byte[] PublicKey { get; set; }

        [Required]
        public byte[] AttestationCert { get; set; }

        [Required]
        public int Counter { get; set; }

        public bool IsCompromised { get; set; }
    }
}
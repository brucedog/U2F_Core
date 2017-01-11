using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace U2F.Demo.Models
{
    public class User : IdentityUser
    {
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public virtual ICollection<Device> DeviceRegistrations { get; set; }

        public virtual ICollection<AuthenticationRequest> AuthenticationRequest { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace U2F.Demo.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public virtual ICollection<Device> DeviceRegistrations { get; set; }

        public virtual ICollection<AuthenticationRequest> AuthenticationRequest { get; set; }
    }
}
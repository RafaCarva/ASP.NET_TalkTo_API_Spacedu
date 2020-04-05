using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalkTo_API.V1.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; }
        public string Slogan { get; set; }
    }
}

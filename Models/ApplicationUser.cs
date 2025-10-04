using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

namespace PortalAcademico.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Matricula> Matriculas { get; set; }

        public ApplicationUser()
        {
            Matriculas = new List<Matricula>();
        }
    }
}
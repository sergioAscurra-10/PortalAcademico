using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PortalAcademico.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PortalAcademico.Models
{
    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }

    public class Matricula
    {
        public int Id { get; set; }

        public int CursoId { get; set; }
        public Curso Curso { get; set; }

        public string UsuarioId { get; set; }
        public ApplicationUser Usuario { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public EstadoMatricula Estado { get; set; }
    }
}

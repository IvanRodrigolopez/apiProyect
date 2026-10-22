using System;
using System.Collections.Generic;
using System.Text;

namespace samsa_asist_proyect.En.Asistencia
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public string GuardName { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;

        // Usamos DateTime para la fecha (se guardará como DATE o DATETIME en MySQL)
        public DateTime Date { get; set; }

        // Usamos TimeSpan para la hora (se guardará como TIME en MySQL)
        public TimeSpan Time { get; set; }

        // Opcional: Para cuando implementes la subida de la imagen
        public string? ImageUrl { get; set; }

        // Fecha en la que se creó el registro (buena práctica)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace samsa_asist_proyect.En.Asistencia
{
    public class Sucursal
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Campo para saber si está activa o no
        public bool Activa { get; set; } = true;

        // Opcional: Fecha de creación para tener un control
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

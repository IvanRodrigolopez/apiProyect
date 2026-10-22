using samsa_asist_proyect.Dt.AppBdContext;
using samsa_asist_proyect.En.Asistencia;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace samsa_asist_proyect.Ng.Asistencia
{
    public class SucursalService
    {
        private readonly AppDbContext _context;

        public SucursalService(AppDbContext context)
        {
            _context = context;
        }

        // Obtener solo las sucursales activas (ideal para llenar el combo en Angular)
        public async Task<List<Sucursal>> GetActivasAsync()
        {
            return await _context.Sucursales
                .Where(s => s.Activa == true)
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        // Obtener todas (por si luego haces una pantalla de administración)
        public async Task<List<Sucursal>> GetAllAsync()
        {
            return await _context.Sucursales.ToListAsync();
        }
    }
    }

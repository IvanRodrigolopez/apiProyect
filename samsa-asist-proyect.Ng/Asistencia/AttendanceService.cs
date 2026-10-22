using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using samsa_asist_proyect.Dt.AppBdContext;
using samsa_asist_proyect.En.Asistencia;

namespace samsa_asist_proyect.Ng.Asistencia
{
    public class AttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterAsync(AttendanceRecord record)
        {
            // REGLA DE NEGOCIO: Por ejemplo, verificar que el guardia no tenga ya un registro hoy
            var existe = await _context.AttendanceRecords
                .AnyAsync(r => r.GuardName == record.GuardName && r.Date.Date == record.Date.Date);

            if (existe) return false; // Ya está registrado hoy

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AttendanceRecord>> GetAllAsync()
        {
            // Traemos la lista ordenada por fecha y hora descendente (los más recientes primero)
            return await _context.AttendanceRecords
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.Time)
                .ToListAsync();
        }

        public async Task<List<AttendanceRecord>> GetPorSucursalAsync(string sucursal)
        {
            return await _context.AttendanceRecords
                .Where(r => r.Branch == sucursal)
                .ToListAsync();
        }
    }
}

using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using samsa_asist_proyect.Dt.AppBdContext;
using samsa_asist_proyect.En.Asistencia;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

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
    
    public async Task<byte[]> GenerarReporteMensualAsync(int mes, int anio)
        {
            // 1. Obtener todas las sucursales activas
            var sucursales = await _context.Sucursales.Where(s => s.Activa).OrderBy(s => s.Nombre).ToListAsync();

            // 2. Obtener todas las asistencias de ese mes
            var asistencias = await _context.AttendanceRecords
                .Where(a => a.Date.Month == mes && a.Date.Year == anio)
                .ToListAsync();

            // 3. Crear el libro de Excel
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"Reporte {mes}-{anio}");

            // 4. Título
            worksheet.Cell(1, 1).Value = $"REPORTE DE ASISTENCIA - {mes}/{anio}";
            worksheet.Range(1, 1, 1, 35).Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // 5. Cabeceras de la tabla
            worksheet.Cell(2, 1).Value = "SUCURSAL";
            worksheet.Cell(2, 2).Value = "ELEMENTO";

            int diasEnElMes = DateTime.DaysInMonth(anio, mes);
            for (int i = 1; i <= diasEnElMes; i++)
            {
                worksheet.Cell(2, i + 2).Value = i; // Días (1, 2, 3...)
            }

            worksheet.Cell(2, diasEnElMes + 3).Value = "FALTAS";
            worksheet.Cell(2, diasEnElMes + 4).Value = "ASISTENCIAS";

            // Estilo de cabecera
            var headerRange = worksheet.Range(2, 1, 2, diasEnElMes + 4);
            headerRange.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            // 6. Llenar los datos por sucursal
            int fila = 3;
            foreach (var suc in sucursales)
            {
                worksheet.Cell(fila, 1).Value = suc.Nombre;
                // Como no tienes el nombre del guardia en la sucursal, lo dejamos vacío o pones el que corresponda
                worksheet.Cell(fila, 2).Value = "Guardia Asignado";

                int totalAsistencias = 0;
                int totalFaltas = 0;

                for (int dia = 1; dia <= diasEnElMes; dia++)
                {
                    // Buscar si hay un registro para esta sucursal en este día
                    var registro = asistencias.FirstOrDefault(a => a.Branch == suc.Nombre && a.Date.Day == dia);

                    var celda = worksheet.Cell(fila, dia + 2);
                    celda.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    if (registro != null)
                    {
                        if (registro.IsRestDay)
                        {
                            celda.Value = "D"; // Descanso
                            celda.Style.Fill.SetBackgroundColor(XLColor.LightYellow);
                        }
                        else
                        {
                            celda.Value = 1; // Asistencia
                            celda.Style.Fill.SetBackgroundColor(XLColor.LightGreen);
                            totalAsistencias++;
                        }
                    }
                    else
                    {
                        // Si no hay registro, se asume Falta (0)
                        celda.Value = 0;
                        celda.Style.Fill.SetBackgroundColor(XLColor.LightPink);
                        totalFaltas++;
                    }
                }

                // Totales
                worksheet.Cell(fila, diasEnElMes + 3).Value = totalFaltas;
                worksheet.Cell(fila, diasEnElMes + 4).Value = totalAsistencias;
                fila++;
            }

            // 7. Ajustar tamaño de columnas
            worksheet.Columns().AdjustToContents();

            // 8. Convertir a arreglo de bytes para enviarlo por la API
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> GenerarReporteZipAsync(int mes, int anio)
        {
            // 1. Obtener sucursales y asistencias de la base de datos
            var sucursales = await _context.Sucursales.Where(s => s.Activa).OrderBy(s => s.Nombre).ToListAsync();

            var asistencias = await _context.AttendanceRecords
                .Where(a => a.Date.Month == mes && a.Date.Year == anio)
                .ToListAsync();

            // 2. Crear el libro de Excel
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"Reporte {mes}-{anio}");

            // Título
            worksheet.Cell(1, 1).Value = $"REPORTE DE ASISTENCIA - {mes}/{anio}";
            worksheet.Range(1, 1, 1, 35).Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Cabeceras
            worksheet.Cell(2, 1).Value = "SUCURSAL";
            worksheet.Cell(2, 2).Value = "ELEMENTO";

            int diasEnElMes = DateTime.DaysInMonth(anio, mes);
            for (int i = 1; i <= diasEnElMes; i++)
            {
                worksheet.Cell(2, i + 2).Value = i;
            }
            worksheet.Cell(2, diasEnElMes + 3).Value = "FALTAS";
            worksheet.Cell(2, diasEnElMes + 4).Value = "ASISTENCIAS";

            // Estilo de cabecera
            var headerRange = worksheet.Range(2, 1, 2, diasEnElMes + 4);
            headerRange.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

            // 3. Llenar los datos
            int fila = 3;
            foreach (var suc in sucursales)
            {
                worksheet.Cell(fila, 1).Value = suc.Nombre;
                worksheet.Cell(fila, 2).Value = "Guardia Asignado"; // (Si tienes el nombre real, cámbialo aquí)

                int totalAsistencias = 0;
                int totalFaltas = 0;

                for (int dia = 1; dia <= diasEnElMes; dia++)
                {
                    var registro = asistencias.FirstOrDefault(a => a.Branch == suc.Nombre && a.Date.Day == dia);
                    var celda = worksheet.Cell(fila, dia + 2);
                    celda.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    if (registro != null)
                    {
                        if (registro.IsAbsence)
                        {
                            celda.Value = 0; // Falta
                            celda.Style.Fill.SetBackgroundColor(XLColor.LightPink);
                            totalFaltas++;
                        }
                        else if (registro.IsRestDay)
                        {
                            celda.Value = "D"; // Descanso
                            celda.Style.Fill.SetBackgroundColor(XLColor.LightYellow);
                        }
                        else
                        {
                            celda.Value = 1; // Asistencia
                            celda.Style.Fill.SetBackgroundColor(XLColor.LightGreen);
                            totalAsistencias++;
                        }
                    }
                    else
                    {
                        // Si no hay registro, evaluamos si es un día futuro
                        var fechaCelda = new DateTime(anio, mes, dia);
                        if (fechaCelda <= DateTime.Today)
                        {
                            celda.Value = 0; // Asumimos falta si ya pasó el día y no hay registro
                            celda.Style.Fill.SetBackgroundColor(XLColor.LightPink);
                            totalFaltas++;
                        }
                    }
                }
                worksheet.Cell(fila, diasEnElMes + 3).Value = totalFaltas;
                worksheet.Cell(fila, diasEnElMes + 4).Value = totalAsistencias;
                fila++;
            }
            worksheet.Columns().AdjustToContents();

            // 4. Crear el archivo ZIP en memoria
            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // 4.1 Agregar el Excel al ZIP
                using (var excelStream = new MemoryStream())
                {
                    workbook.SaveAs(excelStream);
                    excelStream.Position = 0;

                    var excelEntry = archive.CreateEntry($"Reporte_Asistencia_{mes}_{anio}.xlsx");
                    using var entryStream = excelEntry.Open();
                    excelStream.CopyTo(entryStream);
                }

                // 4.2 Agregar las fotos organizadas por carpeta de sucursal
                // Filtramos solo las que tienen foto y no son descanso ni falta
                var registrosConFoto = asistencias
                    .Where(a => !a.IsAbsence && !a.IsRestDay && !string.IsNullOrEmpty(a.ImageUrl))
                    .ToList();

                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                foreach (var asistencia in registrosConFoto)
                {
                    // Limpiamos el nombre de la sucursal por si tiene caracteres no válidos
                    var nombreCarpeta = string.Join("_", asistencia.Branch.Split(Path.GetInvalidFileNameChars()));

                    // Construimos el nombre del archivo: dd-MM-yyyy_Sucursal.jpg
                    var fechaStr = asistencia.Date.ToString("dd-MM-yyyy");
                    var nombreSucursalLimpio = string.Join("_", asistencia.Branch.Split(Path.GetInvalidFileNameChars()));
                    var extension = Path.GetExtension(asistencia.ImageUrl);
                    var nombreArchivo = $"{fechaStr}_{nombreSucursalLimpio}{extension}";

                    // Ruta física de la imagen en el servidor
                    var rutaImagenFisica = Path.Combine(basePath, asistencia.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (File.Exists(rutaImagenFisica))
                    {
                        // Crear la entrada en el ZIP (Carpeta/NombreArchivo)
                        var entryPath = $"{nombreCarpeta}/{nombreArchivo}";
                        var imageEntry = archive.CreateEntry(entryPath);

                        using var imageEntryStream = imageEntry.Open();
                        using var fileStream = new FileStream(rutaImagenFisica, FileMode.Open, FileAccess.Read);
                        fileStream.CopyTo(imageEntryStream);
                    }
                }
            }

            return zipStream.ToArray();
        }

    }



    }

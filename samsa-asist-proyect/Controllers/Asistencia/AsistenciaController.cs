using Microsoft.AspNetCore.Mvc;
using samsa_asist_proyect.En.Asistencia;
using samsa_asist_proyect.Ng.Asistencia;

namespace samsa_asist_proyect.Controllers.Asistencia
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenciaController : ControllerBase
    {
        private readonly AttendanceService _attendanceService;

        // 👇 AQUÍ ESTABA EL ERROR: Cambiar AttendanceController por AsistenciaController
        public AsistenciaController(AttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET: api/Asistencia
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceRecord>>> GetAll()
        {
            try
            {
                var records = await _attendanceService.GetAllAsync();
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("por-sucursal")]
        public async Task<ActionResult<IEnumerable<AttendanceRecord>>> GetPorSucursal([FromQuery] string sucursal)
        {
            // El servicio debería tener un método que filtre por sucursal
            var records = await _attendanceService.GetPorSucursalAsync(sucursal);
            return Ok(records);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAttendance([FromBody] AttendanceRecord record)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdRecord = await _attendanceService.RegisterAsync(record);

                // Retornamos un 201 Created con el objeto guardado
                return CreatedAtAction(nameof(GetAll), new { id = 2 }, createdRecord);
            }
            catch (Exception ex)
            {
                // Retornamos un 400 Bad Request si la regla de negocio falla
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("reporte-excel")]
        public async Task<IActionResult> DescargarReporteExcel([FromQuery] int mes, [FromQuery] int anio)
        {
            try
            {
                var excelBytes = await _attendanceService.GenerarReporteMensualAsync(mes, anio);

                // Retornar el archivo. El nombre será Reporte_Asistencia_10_2026.xlsx
                string nombreArchivo = $"Reporte_Asistencia_{mes}_{anio}.xlsx";
                string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(excelBytes, mimeType, nombreArchivo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se envió ningún archivo.");

            // Validar que sea una imagen
            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("El archivo debe ser una imagen.");

            // Crear un nombre único para la imagen
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Ruta física donde se guardará (wwwroot/images)
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

            // Guardar el archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Retornar la URL relativa para guardarla en la base de datos
            var imageUrl = $"/images/{fileName}";
            return Ok(new { imageUrl = imageUrl });
        }

        [HttpGet("reporte-zip")]
        public async Task<IActionResult> DescargarReporteZip([FromQuery] int mes, [FromQuery] int anio)
        {
            try
            {
                var zipBytes = await _attendanceService.GenerarReporteZipAsync(mes, anio);

                string nombreArchivo = $"Fotos_Asistencias_{mes}_{anio}.zip";
                string mimeType = "application/zip";

                return File(zipBytes, mimeType, nombreArchivo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }


}
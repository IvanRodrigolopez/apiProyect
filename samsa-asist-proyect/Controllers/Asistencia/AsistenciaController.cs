using Microsoft.AspNetCore.Mvc;
using samsa_asist_proyect.En.Asistencia;
using samsa_asist_proyect.Ng.Asistencia;

namespace samsa_asist_proyect.Controllers.Asistencia
{
    [ApiController]
    [Route("[controller]")]
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

        [HttpPost]
        public async Task<IActionResult> RegisterAttendance([FromBody] AttendanceRecord record)
        {
            var success = await _attendanceService.RegisterAsync(record);

            if (!success)
                return BadRequest("El guardia ya tiene un registro para esta fecha.");

            return Ok(new { message = "Registro exitoso", data = record });
        }
    }
}
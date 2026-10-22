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
    }
}
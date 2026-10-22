using Microsoft.AspNetCore.Mvc;
using samsa_asist_proyect.En.Asistencia;
using samsa_asist_proyect.Ng.Asistencia;

namespace samsa_asist_proyect.Controllers.Asistencia
{
    [ApiController]
    [Route("api/[controller]")]
    public class SucursalController : ControllerBase
    {
        private readonly SucursalService _sucursalService;

        public SucursalController(SucursalService sucursalService)
        {
            _sucursalService = sucursalService;
        }

        // GET: api/Sucursal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sucursal>>> GetActivas()
        {
            var sucursales = await _sucursalService.GetActivasAsync();
            return Ok(sucursales);
        }
    }
}

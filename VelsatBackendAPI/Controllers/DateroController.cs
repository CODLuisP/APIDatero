using APIDatero.Model.Datero;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model.Datero;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DateroController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;

        public DateroController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("urbano/{placa}")]
        public async Task<ActionResult<UrbanoAsigna>> GetUrbanoAsigna(string placa)
        {
            var result = await _unitOfWork.DateroRepository.GetUrbanoAsigna(placa);

            if (result == null)
                return NoContent();

            return Ok(result);
        }

        [HttpGet("logurb/{codasig}")]
        public async Task<ActionResult<IEnumerable<LogUrbano>>> GetLogUrbano(string codasig)
        {
            var result = await _unitOfWork.DateroRepository.GetLogUrbano(codasig);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost("enviocontrol")]
        public async Task<IActionResult> EnvioControl([FromBody] LogUrbano log)
        {
            if (log == null)
                return BadRequest("Datos inválidos");

            var resultado = await _unitOfWork.DateroRepository.EnvioControl(log);

            return Ok(new { mensaje = resultado });
        }

        [HttpGet("devices/{accountID}")]
        public async Task<IActionResult> GetDevices(string accountID)
        {
            return Ok(await _unitOfWork.DateroRepository.GetDevices(accountID));
        }

        [HttpGet("ruta/{rutacct}")]
        public async Task<IActionResult> GetRuta(string rutacct)
        {
            return Ok(await _unitOfWork.DateroRepository.GetDevicesOrden(rutacct));
        }

        [HttpPost("endruta/{deviceID}")]
        public async Task<IActionResult> EndRuta(string deviceID)
        {
            if (string.IsNullOrWhiteSpace(deviceID))
            {
                return BadRequest("El deviceID es obligatorio.");
            }

            var resultado = await _unitOfWork.DateroRepository.EndRuta(deviceID);

            if (resultado.Contains("correctamente"))
            {
                return Ok(new { mensaje = resultado });
            }

            if (resultado.Contains("No se encontró"))
            {
                return NotFound(new { mensaje = resultado });
            }

            return StatusCode(500, new { mensaje = resultado });
        }

        [HttpPost("asignar")]
        public async Task<IActionResult> AsignarAndroidID([FromBody] ImeiAsignacionRequest request)
        {
            if (string.IsNullOrEmpty(request.Placa) || string.IsNullOrEmpty(request.AndroidID))
            {
                return BadRequest("Todos los campos son obligatorios.");
            }

            var resultado = await _unitOfWork.DateroRepository.AsignarID(request.Placa, request.AndroidID);

            return Ok(new { mensaje = resultado });
        }

        [HttpGet("control/{fecha}/{codruta}/{username}")]
        public async Task<ActionResult<IEnumerable<DespachoAgrupado>>> ControlDespachoAgrupado(string fecha, string codruta, string username)
        {
            var controles = await _unitOfWork.DateroRepository.ControlDespachoAgrupado(fecha, codruta, username);

            if (controles == null || !controles.Any())
            {
                return Ok(new
                {
                    data = new List<DespachoAgrupado>(),
                    message = "No se encontraron registros para la fecha indicada."
                });
            }

            return Ok(new
            {
                data = controles,
                message = "Registros encontrados correctamente."
            });
        }

        [HttpGet("controlEdu/{fecha}/{codruta}/{username}")]
        public async Task<ActionResult<IEnumerable<DespachoAgrupado>>> ControlDespachoAgrupadoEdu(string fecha, string codruta, string username)
        {
            var controles = await _unitOfWork.DateroRepository.ControlDespachoAgrupadoEdu(fecha, codruta, username);

            if (controles == null || !controles.Any())
            {
                return Ok(new
                {
                    data = new List<DespachoAgrupado>(),
                    message = "No se encontraron registros para la fecha indicada."
                });
            }

            return Ok(new
            {
                data = controles,
                message = "Registros encontrados correctamente."
            });
        }

        //[HttpGet("controlGPSE/{fecha}/{codruta}")]
        //public async Task<IActionResult> GetControlGPSE(string fecha, string codruta)
        //{
        //    try
        //    {
        //        var resultado = await _unitOfWork.DateroRepository.ControlDespachoGPSE(fecha, codruta);
        //        return Ok(new { data = resultado });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = ex.Message });
        //    }
        //}

        //APIS PARA REGISTROS DE GPS VEHICULAR

        [HttpGet("GPSUniDesp")]
        public async Task<IActionResult> GetDevicesDespacho([FromQuery] string fecha, [FromQuery] string ruta)
        {
            try
            {
                if (string.IsNullOrEmpty(fecha))
                {
                    return BadRequest("La fecha es requerida");
                }

                var devices = await _unitOfWork.DateroRepository.DevicesDespacho(fecha, ruta);
                return Ok(devices);
            }
            catch (FormatException)
            {
                return BadRequest("Formato de fecha inválido. Use dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("GPSUniDespEdu")]
        public async Task<IActionResult> GetDevicesDespachoEdu([FromQuery] string fecha, [FromQuery] string ruta)
        {
            try
            {
                if (string.IsNullOrEmpty(fecha))
                {
                    return BadRequest("La fecha es requerida");
                }

                var devices = await _unitOfWork.DateroRepository.DevicesDespachoEdu(fecha, ruta);
                return Ok(devices);
            }
            catch (FormatException)
            {
                return BadRequest("Formato de fecha inválido. Use dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("EnvioGPS/{username}")]
        public async Task<IActionResult> EnvioControlGPS([FromBody] ControlGPS[] logs, [FromRoute] string username)
        {
            try
            {
                if (logs == null || logs.Length == 0)
                {
                    return BadRequest("Se requiere al menos un registro para procesar");
                }

                var resultado = await _unitOfWork.DateroRepository.EnvioControlGPS(logs, username);

                return Ok(new
                {
                    success = true,
                    message = resultado,
                    totalRegistros = logs.Length
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpPost("EnvioControlB/{username}")]
        public async Task<IActionResult> EnvioControlB([FromBody] ControlGPS[] logs, [FromRoute] string username)
        {
            try
            {
                if (logs == null || logs.Length == 0)
                {
                    return BadRequest("Se requiere al menos un registro para procesar");
                }

                var resultado = await _unitOfWork.DateroRepository.EnvioControlB(logs, username);

                return Ok(new
                {
                    success = true,
                    message = resultado,
                    totalRegistros = logs.Length
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpPut("ActualizarRutaact/{placa}")]
        public async Task<IActionResult> ActualizarRutaact(string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La placa es requerida"
                    });
                }

                var resultado = await _unitOfWork.DateroRepository.ActualizarRutaact(placa);

                if (!resultado)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró el dispositivo con placa: {placa}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Ruta actualizada correctamente para la placa: {placa}",
                    placa = placa
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpPut("ActualizarRutaactG/{placa}")]
        public async Task<IActionResult> ActualizarRutaactG(string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La placa es requerida"
                    });
                }

                var resultado = await _unitOfWork.DateroRepository.ActualizarRutaactG(placa);

                if (!resultado)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró el dispositivo con placa: {placa}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Ruta actualizada correctamente para la placa: {placa}",
                    placa = placa
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpPut("ActualizarRutaactFR/{placa}")]
        public async Task<IActionResult> ActualizarRutaactFR(string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La placa es requerida"
                    });
                }

                var resultado = await _unitOfWork.DateroRepository.ActualizarRutaactFR(placa);

                if (!resultado)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró el dispositivo con placa: {placa}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Ruta actualizada correctamente para la placa: {placa}",
                    placa = placa
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }
    }
} 

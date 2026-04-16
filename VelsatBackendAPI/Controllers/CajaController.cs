using APIDatero.Model.Documentacion;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model.Caja;
using VelsatBackendAPI.Model.GestionVilla;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CajaController : ControllerBase
    {
        //APIs PARA GESTIÓN CAJA VILLA
        private readonly IUnitOfWork _unitOfWork;

        public CajaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("conductores")]
        public async Task<IActionResult> GetConductores([FromQuery] string username)
        {
            var conductores = await _unitOfWork.CajaRepository.GetConductores(username);

            if (conductores == null || conductores.Count == 0)
            {
                return NotFound(new { message = "No se encontraron conductores para este usuario" });
            }

            return Ok(conductores);
        }

        [HttpGet("unidades")]
        public async Task<IActionResult> GetUnidades([FromQuery] string username)
        {
            try
            {
                var unidades = await _unitOfWork.CajaRepository.GetUnidades(username);

                if (unidades == null || unidades.Count == 0)
                {
                    return NotFound(new { message = "No se encontraron unidades" });
                }

                return Ok(unidades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        //APIs PARA GESTION VILLA
        [HttpGet("Rutas/{usuario}")]
        public async Task<IActionResult> ListaRutas(string usuario)
        {
            var rutas = await _unitOfWork.CajaRepository.ListaRutas(usuario);

            if (rutas == null || !rutas.Any())
            {
                return NotFound("No se encontraron rutas para el usuario proporcionado.");
            }

            return Ok(rutas);
        }

        [HttpGet("conductoresDisp/{usuario}")]
        public async Task<IActionResult> ListaConducDisp(string usuario)
        {
            var conductores = await _unitOfWork.CajaRepository.ListaConducDisp(usuario);

            if (conductores == null || !conductores.Any())
            {
                return NotFound("No se encontraron conductores disponibles.");
            }

            return Ok(conductores);
        }

        [HttpGet("unidadesDisp/{usuario}")]
        public async Task<ActionResult<List<Carro>>> ListUnidDisp(string usuario)
        {
            try
            {
                var unidades = await _unitOfWork.CajaRepository.ListUnidDisp(usuario);

                if (unidades == null || !unidades.Any())
                {
                    return NotFound("No se encontraron unidades disponibles.");
                }

                return Ok(unidades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("detalleConductor/{codtaxi}")]
        public async Task<ActionResult<Usuario>> GetDetalleConductor(string codtaxi)
        {
            try
            {
                var conductor = await _unitOfWork.CajaRepository.GetDetalleConductor(codtaxi);

                if (conductor == null)
                {
                    return NotFound("No se encontró el conductor.");
                }

                return Ok(conductor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("despachoIni")]
        public async Task<IActionResult> ListDespachoIniciado([FromQuery] string codruta)
        {
            if (string.IsNullOrWhiteSpace(codruta))
            {
                return BadRequest("El parámetro 'codruta' es requerido.");
            }

            try
            {
                var despachos = await _unitOfWork.CajaRepository.ListDespachoIniciado(codruta);
                return Ok(despachos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("asignar")]
        public async Task<IActionResult> AsignarDespacho([FromBody] DespachoVilla despacho)
        {
            if (despacho == null || despacho.Carro == null || despacho.Ruta == null || despacho.Conductor == null)
            {
                return BadRequest("Datos incompletos.");
            }

            var resultado = await _unitOfWork.CajaRepository.AsignarDespacho(despacho);

            if (resultado == "Datos Guardados")
            {
                return Ok(new { mensaje = resultado });
            }
            else
            {
                return StatusCode(500, new { mensaje = resultado });
            }
        }

        //DESPACHO RUTA B
        [HttpPost("asignarB")]
        public async Task<IActionResult> AsignarDespachoB([FromBody] DespachoVilla despacho)
        {
            if (despacho == null || despacho.Carro == null || despacho.Ruta == null || despacho.Conductor == null)
            {
                return BadRequest("Datos incompletos.");
            }

            var resultado = await _unitOfWork.CajaRepository.AsignarDespacho(despacho);

            if (resultado == "Datos Guardados")
            {
                // Consultar el Codasig recién insertado y la hora de inicio
                var datosRecientes = await _unitOfWork.UrbanoAsignaService.GetUrbanoAsigna(despacho.Carro.Codunidad);

                // Obtener el Codasig del último despacho
                var codasig = await _unitOfWork.CajaRepository.ObtenerUltimoCodasig(despacho.Carro.Codunidad);

                // Notificar a la app de consola
                _ = NotificarAppConsolaAsync(despacho.Carro.Codunidad, datosRecientes?.Fechaini, codasig);

                return Ok(new { mensaje = resultado });
            }
            else
            {
                return StatusCode(500, new { mensaje = resultado });
            }
        }

        private async Task NotificarAppConsolaAsync(string placa, string fechaIni, string codasig)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var payload = new
                    {
                        placa = placa,
                        usuario = "etudvrb",
                        fechaIni = fechaIni,
                        codasig = codasig  // ← AGREGAR CODASIG
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        "http://localhost:5003/api/iniciar-monitoreo",
                        content
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✓ App de consola notificada para placa {placa}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error notificando app de consola: {ex.Message}");
            }
        }


        //DESPACHO RUTA G
        [HttpPost("asignarG")]
        public async Task<IActionResult> AsignarDespachoG([FromBody] DespachoVilla despacho)
        {
            if (despacho == null || despacho.Carro == null || despacho.Ruta == null || despacho.Conductor == null)
            {
                return BadRequest("Datos incompletos.");
            }

            var resultado = await _unitOfWork.CajaRepository.AsignarDespacho(despacho);

            if (resultado == "Datos Guardados")
            {
                // Consultar el Codasig recién insertado y la hora de inicio
                var datosRecientes = await _unitOfWork.UrbanoAsignaService.GetUrbanoAsigna(despacho.Carro.Codunidad);

                // Obtener el Codasig del último despacho
                var codasig = await _unitOfWork.CajaRepository.ObtenerUltimoCodasig(despacho.Carro.Codunidad);

                // Notificar a la app de consola (etudvrg - Puerto 5001)
                _ = NotificarAppConsolaG(despacho.Carro.Codunidad, datosRecientes?.Fechaini, codasig);

                return Ok(new { mensaje = resultado });
            }
            else
            {
                return StatusCode(500, new { mensaje = resultado });
            }
        }

        private async Task NotificarAppConsolaG(string placa, string fechaIni, string codasig)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var payload = new
                    {
                        placa = placa,
                        usuario = "etudvrg",
                        fechaIni = fechaIni,
                        codasig = codasig
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        "http://localhost:5001/api/iniciar-monitoreo",
                        content
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✓ App de consola (etudvrg) notificada para placa {placa}");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Error al notificar etudvrg: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error notificando app de consola etudvrg: {ex.Message}");
            }
        }

        //DESPACHO RUTA 22
        [HttpPost("asignar22")]
        public async Task<IActionResult> AsignarDespacho22([FromBody] DespachoVilla despacho)
        {
            if (despacho?.Carro == null || despacho?.Ruta == null || despacho?.Conductor == null)
            {
                return BadRequest("Datos incompletos.");
            }

            var resultado = await _unitOfWork.CajaRepository.AsignarDespacho(despacho);

            if (resultado == "Datos Guardados")
            {
                var datosRecientes = await _unitOfWork.UrbanoAsignaService.GetUrbanoAsigna(despacho.Carro.Codunidad);
                var codasig = await _unitOfWork.CajaRepository.ObtenerUltimoCodasig(despacho.Carro.Codunidad);

                string codigoRuta = datosRecientes?.Codruta ?? "11";

                // ✅ Log para verificar
                Console.WriteLine($"🔍 Datos a enviar:");
                Console.WriteLine($"   Placa: {despacho.Carro.Codunidad}");
                Console.WriteLine($"   FechaIni: {datosRecientes?.Fechaini}");
                Console.WriteLine($"   Codasig: {codasig}");
                Console.WriteLine($"   CodigoRuta: {codigoRuta}");

                _ = NotificarAppConsola22(
                    despacho.Carro.Codunidad,
                    datosRecientes?.Fechaini,
                    codasig,
                    codigoRuta
                );

                return Ok(new { mensaje = resultado });
            }

            return StatusCode(500, new { mensaje = resultado });
        }

        private async Task NotificarAppConsola22(string placa, string fechaIni, string codasig, string codigoRuta)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var payload = new
                    {
                        placa = placa,
                        usuario = "etudv22",
                        fechaIni = fechaIni,
                        codasig = codasig,
                        codigoRuta = codigoRuta  // ✅ "11" o "12" desde BD
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        "http://localhost:5002/api/iniciar-monitoreo",
                        content
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        string tipoRuta = codigoRuta == "11" ? "IDA" : "REGRESO";
                        Console.WriteLine($"✓ App de consola (etudv22) notificada para placa {placa} - Tipo: {tipoRuta} ({codigoRuta})");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Error al notificar etudv22: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error notificando app de consola etudv22: {ex.Message}");
            }
        }


        //DESPACHO RUTA 1148
        [HttpPost("asignarFR")]
        public async Task<IActionResult> AsignarDespachoFR([FromBody] DespachoVilla despacho)
        {
            if (despacho == null || despacho.Carro == null || despacho.Ruta == null || despacho.Conductor == null)
            {
                return BadRequest("Datos incompletos.");
            }

            var resultado = await _unitOfWork.CajaRepository.AsignarDespacho(despacho);

            if (resultado == "Datos Guardados")
            {
                // Consultar el Codasig recién insertado y la hora de inicio
                var datosRecientes = await _unitOfWork.UrbanoAsignaService.GetUrbanoAsigna(despacho.Carro.Codunidad);

                // Obtener el Codasig del último despacho
                var codasig = await _unitOfWork.CajaRepository.ObtenerUltimoCodasig(despacho.Carro.Codunidad);

                // Notificar a la app de consola
                _ = NotificarAppConsolaFR(despacho.Carro.Codunidad, datosRecientes?.Fechaini, codasig);

                return Ok(new { mensaje = resultado });
            }
            else
            {
                return StatusCode(500, new { mensaje = resultado });
            }
        }

        private async Task NotificarAppConsolaFR(string placa, string fechaIni, string codasig)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var payload = new
                    {
                        placa = placa,
                        usuario = "serfrymh",
                        fechaIni = fechaIni,
                        codasig = codasig  // ← AGREGAR CODASIG
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        "http://localhost:5004/api/iniciar-monitoreo",
                        content
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✓ App de consola notificada para placa {placa}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error notificando app de consola: {ex.Message}");
            }
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> EliminarDespacho([FromBody] DespachoVilla despacho)
        {
            try
            {
                if (despacho == null || string.IsNullOrEmpty(despacho.Codigo))
                {
                    return BadRequest("Datos incompletos.");
                }

                var resultado = await _unitOfWork.CajaRepository.EliminarDespacho(despacho);

                if (resultado == "Datos Guardados")
                    return Ok(new { mensaje = resultado });

                return StatusCode(500, new { error = resultado });
            }
            catch (Exception ex)
            {
                // Loguear error si es necesario
                return StatusCode(500, new { error = "Error al procesar la solicitud." });
            }

        }
        //FIN


        [HttpGet("conductores/{usuario}")]
        public async Task<ActionResult<List<Conductor>>> GetConductoresByUsuario(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario))
                {
                    return BadRequest("El parámetro usuario es requerido");
                }

                var conductores = await _unitOfWork.CajaRepository.GetConductoresxUsuario(usuario);

                if (conductores == null || !conductores.Any())
                {
                    return NotFound("No se encontraron conductores para el usuario especificado");
                }

                return Ok(conductores);
            }
            catch (Exception ex)
            {
                // Log del error aquí si tienes un logger configurado
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("carros/{usuario}")]
        public async Task<ActionResult<List<Carro>>> GetUnidadesxUsuario(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario))
                {
                    return BadRequest("El parámetro usuario es requerido");
                }

                var carros = await _unitOfWork.CajaRepository.GetUnidadesxUsuario(usuario);

                if (carros == null || !carros.Any())
                {
                    return NotFound("No se encontraron carros para el usuario especificado");
                }

                return Ok(carros);
            }
            catch (Exception ex)
            {
                // Log del error aquí si tienes un logger configurado
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("cobradores/{usuario}")]
        public async Task<ActionResult<List<Conductor>>> GetCobradoresByUsuario(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario))
                {
                    return BadRequest("El parámetro usuario es requerido");
                }

                var conductores = await _unitOfWork.CajaRepository.GetCobradoresxUsuario(usuario);

                if (conductores == null || !conductores.Any())
                {
                    return NotFound("No se encontraron cobradores para el usuario especificado");
                }

                return Ok(conductores);
            }
            catch (Exception ex)
            {
                // Log del error aquí si tienes un logger configurado
                return StatusCode(500, "Error interno del servidor");
            }
        }


        [HttpGet("vueltas")]
        public async Task<IActionResult> GetReporteVueltas([FromQuery] string fecha, [FromQuery] string ruta)
        {
            try
            {
                if (string.IsNullOrEmpty(fecha))
                {
                    return BadRequest("El parámetro 'fecha' es requerido");
                }

                if (string.IsNullOrEmpty(ruta))
                {
                    return BadRequest("El parámetro 'ruta' es requerido");
                }

                if (!DateTime.TryParseExact(fecha, "dd/MM/yyyy", null, DateTimeStyles.None, out _))
                {
                    return BadRequest("El formato de fecha debe ser dd/MM/yyyy");
                }

                var resultado = await _unitOfWork.CajaRepository.ReporteVueltas(fecha, ruta);

                if (resultado == null || !resultado.Any())
                {
                    return Ok("No se encontraron vueltas para la fecha y ruta especificada");
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("NuevoConductor/{usuario}")]
        public async Task<IActionResult> NuevoConductorAsync([FromBody] Conductor conductor, [FromRoute] string usuario)
        {
            try
            {
                var conductorID = await _unitOfWork.CajaRepository.NuevoConductorAsync(conductor, usuario);

                if (conductorID > 0)
                {
                    return Ok(new { mensaje = "Conductor guardado exitosamente", id = conductorID });
                }

                return BadRequest("Error al guardar conductor");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost("NuevoCobrador/{usuario}")]
        public async Task<IActionResult> GuardarCobrador([FromBody] Conductor cobrador, [FromRoute] string usuario)
        {
            try
            {
                var cobradorId = await _unitOfWork.CajaRepository.GuardarCobradorAsync(cobrador, usuario);

                if (cobradorId > 0)
                {
                    return Ok(new { mensaje = "Cobrador guardado exitosamente", id = cobradorId });
                }

                return BadRequest("Error al guardar cobrador");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // El repositorio queda igual, retornando el ID

        [HttpPut("ModificarConductor/{id}")]
        public async Task<IActionResult> ModificarConductor(int id, [FromBody] Conductor conductor)
        {
            try
            {
                // Ejecutar la modificación
                var resultado = await _unitOfWork.CajaRepository.ModificarConductorAsync(conductor);

                return resultado switch
                {
                    0 => NotFound(new { message = "Conductor no encontrado o no se pudo modificar", code = 0 }),
                    1 => Ok(new { message = "Conductor modificado exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                // Log del error si tienes logger
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("ModificarCobrador/{id}")]
        public async Task<IActionResult> ModificarCobrador(int id, [FromBody] Conductor cobrador)
        {
            try
            {
                // Ejecutar la modificación
                var resultado = await _unitOfWork.CajaRepository.ModificarCobradorAsync(cobrador);

                return resultado switch
                {
                    0 => NotFound(new { message = "Cobrador no encontrado o no se pudo modificar", code = 0 }),
                    1 => Ok(new { message = "Cobrador modificado exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                // Log del error si tienes logger
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("HabilitarCond/{id}")]
        public async Task<IActionResult> HabilitarConductor(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Código de conductor inválido" });

                var resultado = await _unitOfWork.CajaRepository.HabilitarConductorAsync(id);

                return resultado switch
                {
                    0 => NotFound(new { message = "Conductor no encontrado", code = 0 }),
                    1 => Ok(new { message = "Conductor habilitado exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("HabilitarCob/{id}")]
        public async Task<IActionResult> HabilitarCobrador(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Código de cobrador inválido" });

                var resultado = await _unitOfWork.CajaRepository.HabilitarCobradorAsync(id);

                return resultado switch
                {
                    0 => NotFound(new { message = "Cobrador no encontrado", code = 0 }),
                    1 => Ok(new { message = "Cobrador habilitado exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("DeshabilitarCond/{id}")]
        public async Task<IActionResult> DeshabilitarConductor(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Código de conductor inválido" });

                var resultado = await _unitOfWork.CajaRepository.DeshabilitarConductorAsync(id);

                return resultado switch
                {
                    0 => NotFound(new { message = "Conductor no encontrado", code = 0 }),
                    1 => Ok(new { message = "Conductor deshabilitado exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("DeshabilitarCob/{id}")]
        public async Task<IActionResult> DeshabilitarCobrador(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Código de cobrador inválido" });

                var resultado = await _unitOfWork.CajaRepository.DeshabilitarCobradorAsync(id);

                return resultado switch
                {
                    0 => NotFound(new { message = "Cobrador no encontrado", code = 0 }),
                    1 => Ok(new { message = "Cobrador deshabilitado exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("Liberar/{id}")]
        public async Task<IActionResult> LiberarConductor(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Código de conductor inválido" });

                var resultado = await _unitOfWork.CajaRepository.LiberarConductorAsync(id);

                return resultado switch
                {
                    0 => NotFound(new { message = "Conductor no encontrado", code = 0 }),
                    1 => Ok(new { message = "Conductor liberado exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> EliminarConductorDelete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Código de conductor inválido" });

                var resultado = await _unitOfWork.CajaRepository.EliminarConductorAsync(id);

                return resultado switch
                {
                    0 => NotFound(new { message = "Conductor no encontrado", code = 0 }),
                    1 => Ok(new { message = "Conductor eliminado exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("EliminarCobrador/{id}")]
        public async Task<IActionResult> EliminarCobradorDelete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Código de cobrador inválido" });

                var resultado = await _unitOfWork.CajaRepository.EliminarCobradorAsync(id);

                return resultado switch
                {
                    0 => NotFound(new { message = "Cobrador no encontrado", code = 0 }),
                    1 => Ok(new { message = "Cobrador eliminado exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("HabilitarUnidad/{placa}")]
        public async Task<IActionResult> HabilitarUnidad(string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                    return BadRequest(new { message = "La placa de la unidad es requerida" });

                var resultado = await _unitOfWork.CajaRepository.HabilitarUnidadAsync(placa);

                return resultado switch
                {
                    0 => NotFound(new { message = "Unidad no encontrada", code = 0 }),
                    1 => Ok(new { message = "Unidad habilitada exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("DeshabilitarUnidad/{placa}")]
        public async Task<IActionResult> DeshabilitarUnidad(string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                    return BadRequest(new { message = "La placa de la unidad es requerida" });

                var resultado = await _unitOfWork.CajaRepository.DeshabilitarUnidadAsync(placa);

                return resultado switch
                {
                    0 => NotFound(new { message = "Unidad no encontrada", code = 0 }),
                    1 => Ok(new { message = "Unidad deshabilitada exitosamente", code = 1 }),
                    _ => StatusCode(500, new { message = "Error interno", code = -1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("LiberarUnidad/{placa}")]
        public async Task<IActionResult> LiberarUnidad(string placa)
        {
            try
            {
                var resultado = await _unitOfWork.CajaRepository.LiberarUnidadAsync(placa);
                return resultado switch
                {
                    0 => NotFound(new { message = "Unidad no encontrada o no se pudo liberar", code = 0 }),
                    1 => Ok(new { message = "Unidad liberada exitosamente", code = 1 }),
                    _ => Ok(new { message = $"{resultado} unidades liberadas exitosamente", code = 1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("LiberarTotal")]
        public async Task<IActionResult> LiberarTodasUnidades([FromQuery] string username)
        {
            try
            {
                var resultado = await _unitOfWork.CajaRepository.LiberarTotalUnidadesAsync(username);
                return resultado switch
                {
                    0 => NotFound(new { message = "No se encontraron unidades para liberar", code = 0 }),
                    _ => Ok(new { message = $"{resultado} unidades liberadas exitosamente", code = 1 })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        //APIS PARA CONTROL GPS
        [HttpPost("HoraSolicitud")]
        public async Task<IActionResult> EnviarHoraSolicitud([FromQuery] string codruta)
        {
            try
            {
                if (codruta == null)
                    return BadRequest("El código de ruta es requerido");

                var resultado = await _unitOfWork.CajaRepository.EnviarHoraSolicitud(codruta);

                return Ok(new
                {
                    success = true,
                    message = resultado
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

        [HttpGet("UltimoRegistro")]
        public async Task<IActionResult> GetHoraSolicitud([FromQuery] string codruta)
        {
            try
            {
                var ultimaFecha = await _unitOfWork.CajaRepository.GetHoraSolicitud(codruta);

                if (ultimaFecha == "No hay registros")
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No hay registros de hora de solicitud"
                    });
                }

                return Ok(new
                {
                    success = true,
                    fecha = ultimaFecha
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

        //DOCUMENTACIÓN
        //----------------------------------UNIDAD--------------------------------------------------//

        [HttpGet("GetByDeviceID")]
        public async Task<IActionResult> GetByDeviceID([FromQuery] string deviceID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deviceID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El DeviceID es requerido"
                    });
                }

                var documentos = await _unitOfWork.CajaRepository.GetByDeviceID(deviceID);

                if (documentos == null || !documentos.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontraron documentos para el dispositivo {deviceID}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = documentos,
                    count = documentos.Count
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

        [HttpPost("CreateDocUnidad")]
        public async Task<IActionResult> Create([FromBody] Docunidad docunidad)
        {
            try
            {
                if (docunidad == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Los datos del documento son requeridos"
                    });
                }

                var id = await _unitOfWork.CajaRepository.Create(docunidad);

                return Ok(new
                {
                    success = true,
                    message = "Documento creado exitosamente",
                    id = id
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

        [HttpDelete("DeleteDocUnidad")]
        public async Task<IActionResult> DeleteUnidad([FromQuery] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID inválido"
                    });
                }

                var resultado = await _unitOfWork.CajaRepository.DeleteUnidad(id);

                if (!resultado)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró el documento con ID {id}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Documento eliminado exitosamente"
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

        [HttpGet("GetDocUnidadPorVencer")]
        public async Task<IActionResult> GetDocumentosUnidadProximosVencer([FromQuery] string usuario)
        {
            try
            {
                var documentos = await _unitOfWork.CajaRepository.GetDocumentosUnidadProximosVencer(usuario);

                if (documentos == null || !documentos.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No se encontraron documentos próximos a vencer"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = documentos                   
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

        //----------------------------------CONDUCTOR--------------------------------------------------//
        [HttpGet("GetByCodtaxi")]
        public async Task<IActionResult> GetByCodtaxi([FromQuery] int codtaxi)
        {
            try
            {
                if (codtaxi <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El Codtaxi debe ser mayor a 0"
                    });
                }

                var documentos = await _unitOfWork.CajaRepository.GetByCodtaxi(codtaxi);

                if (documentos == null || !documentos.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontraron documentos para el conductor {codtaxi}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = documentos,
                    count = documentos.Count
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

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Docconductor docconductor)
        {
            try
            {
                if (docconductor == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Los datos del documento son requeridos"
                    });
                }

                var id = await _unitOfWork.CajaRepository.Create(docconductor);

                return Ok(new
                {
                    success = true,
                    message = "Documento creado exitosamente",
                    id = id
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

        [HttpDelete("DeleteConductor")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID inválido"
                    });
                }

                var resultado = await _unitOfWork.CajaRepository.DeleteConductor(id);

                if (!resultado)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró el documento con ID {id}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Documento eliminado exitosamente"
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

        [HttpGet("GetDocCondcutorPorVencer")]
        public async Task<IActionResult> GetDocumentosProximosVencer(string usuario)
        {
            try
            {
                var documentos = await _unitOfWork.CajaRepository.GetDocumentosConductorProximosVencer(usuario);

                if (documentos == null || !documentos.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No se encontraron documentos próximos a vencer"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = documentos                   
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

        //----------------------------------COBRADOR--------------------------------------------------//
        [HttpGet("GetByCobrador")]
        public async Task<IActionResult> GetByCobrador([FromQuery] int codtaxi)
        {
            try
            {
                if (codtaxi <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El Codtaxi debe ser mayor a 0"
                    });
                }

                var documentos = await _unitOfWork.CajaRepository.GetByCobrador(codtaxi);

                if (documentos == null || !documentos.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontraron documentos para el conductor {codtaxi}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = documentos,
                    count = documentos.Count
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

        [HttpPost("CreateCobrador")]
        public async Task<IActionResult> CreateCobrador([FromBody] Docconductor docconductor)
        {
            try
            {
                if (docconductor == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Los datos del documento son requeridos"
                    });
                }

                var id = await _unitOfWork.CajaRepository.CreateCobrador(docconductor);

                return Ok(new
                {
                    success = true,
                    message = "Documento creado exitosamente",
                    id = id
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

        [HttpDelete("DeleteCobrador")]
        public async Task<IActionResult> DeleteCobrador([FromQuery] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID inválido"
                    });
                }

                var resultado = await _unitOfWork.CajaRepository.DeleteCobrador(id);

                if (!resultado)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No se encontró el documento con ID {id}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Documento eliminado exitosamente"
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

        [HttpGet("GetDocumentosCobradorProximosVencer")]
        public async Task<IActionResult> GetDocumentosCobradorProximosVencer(string usuario)
        {
            try
            {
                var documentos = await _unitOfWork.CajaRepository.GetDocumentosCobradorProximosVencer(usuario);

                if (documentos == null || !documentos.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No se encontraron documentos próximos a vencer"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = documentos
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

        [HttpGet("detalleCobrador/{codtaxi}")]
        public async Task<ActionResult<Usuario>> GetDetalleCobrador(string codtaxi)
        {
            try
            {
                var conductor = await _unitOfWork.CajaRepository.GetDetalleCobrador(codtaxi);

                if (conductor == null)
                {
                    return NotFound("No se encontró el conductor.");
                }

                return Ok(conductor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}

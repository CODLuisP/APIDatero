using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model.Administracion;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private static bool MotivoValido(string motivo) => !string.IsNullOrWhiteSpace(motivo) && motivo.Trim().Length >= 6;

        [HttpGet("Usuarios")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _unitOfWork.AdminRepository.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los usuarios", error = ex.Message });
            }
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] Usuarioadmin usuario, [FromQuery] string actor, [FromQuery] string motivo)
        {
            try
            {
                if (usuario == null)
                {
                    return BadRequest(new { message = "El usuario no puede ser nulo" });
                }

                if (!MotivoValido(motivo))
                {
                    return BadRequest(new { message = "El motivo es obligatorio y debe tener al menos 6 caracteres" });
                }

                var rowsAffected = await _unitOfWork.AdminRepository.UpdateUser(usuario);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                await _unitOfWork.AdminRepository.RegistrarAuditoria(actor, "Usuarios", "Actualizar", usuario.AccountID, $"Se actualizó el usuario {usuario.AccountID}. Motivo: {motivo.Trim()}");

                return Ok(new { message = "Usuario actualizado correctamente", rowsAffected });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el usuario", error = ex.Message });
            }
        }

        [HttpDelete("DeleteUsuario/{accountID}")]
        public async Task<IActionResult> DeleteUser(string accountID, [FromQuery] string actor, [FromQuery] string motivo)
        {
            try
            {
                if (string.IsNullOrEmpty(accountID))
                {
                    return BadRequest(new { message = "El accountID no puede ser nulo o vacío" });
                }

                if (!MotivoValido(motivo))
                {
                    return BadRequest(new { message = "El motivo es obligatorio y debe tener al menos 6 caracteres" });
                }

                var rowsAffected = await _unitOfWork.AdminRepository.DeleteUser(accountID);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                await _unitOfWork.AdminRepository.RegistrarAuditoria(actor, "Usuarios", "Eliminar", accountID, $"Se eliminó el usuario {accountID}. Motivo: {motivo.Trim()}");

                return Ok(new { message = "Usuario eliminado correctamente", accountID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el usuario", error = ex.Message });
            }
        }

        [HttpPost("InsertUsuario")]
        public async Task<IActionResult> InsertUser([FromBody] Usuarioadmin usuario, [FromQuery] string actor, [FromQuery] string motivo)
        {
            try
            {
                if (usuario == null)
                {
                    return BadRequest(new { message = "El usuario no puede ser nulo" });
                }

                if (string.IsNullOrEmpty(usuario.AccountID) || string.IsNullOrEmpty(usuario.Password))
                {
                    return BadRequest(new { message = "AccountID y Password son obligatorios" });
                }

                if (!MotivoValido(motivo))
                {
                    return BadRequest(new { message = "El motivo es obligatorio y debe tener al menos 6 caracteres" });
                }

                var rowsAffected = await _unitOfWork.AdminRepository.InsertUser(usuario);

                if (rowsAffected == 0)
                {
                    return StatusCode(500, new { message = "No se pudo insertar el usuario" });
                }

                await _unitOfWork.AdminRepository.RegistrarAuditoria(actor, "Usuarios", "Crear", usuario.AccountID, $"Se creó el usuario {usuario.AccountID}. Motivo: {motivo.Trim()}");

                return Ok(new { message = "Usuario creado correctamente", accountID = usuario.AccountID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el usuario", error = ex.Message });
            }
        }

        [HttpGet("SubUsuarios")]
        public async Task<IActionResult> GetSubUsers()
        {
            try
            {
                var users = await _unitOfWork.AdminRepository.GetSubUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los subusuarios", error = ex.Message });
            }
        }

        [HttpPost("InsertDeviceUser")]
        public async Task<IActionResult> InsertSubUser([FromBody] Deviceuser usuario, [FromQuery] string actor, [FromQuery] string motivo)
        {
            try
            {
                if (usuario == null)
                {
                    return BadRequest(new { message = "El device user no puede ser nulo" });
                }

                if (!MotivoValido(motivo))
                {
                    return BadRequest(new { message = "El motivo es obligatorio y debe tener al menos 6 caracteres" });
                }

                var rowsAffected = await _unitOfWork.AdminRepository.InsertSubUser(usuario);

                if (rowsAffected == 0)
                {
                    return StatusCode(500, new { message = "No se pudo insertar el device user" });
                }

                await _unitOfWork.AdminRepository.RegistrarAuditoria(actor, "SubUsuarios", "Crear", usuario.UserId, $"Se creó el subusuario {usuario.UserId}. Motivo: {motivo.Trim()}");

                return Ok(new { message = "Device user creado correctamente", id = usuario.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el device user", error = ex.Message });
            }
        }

        [HttpPut("UpdateDeviceUser")]
        public async Task<IActionResult> UpdateSubUser([FromBody] Deviceuser usuario, [FromQuery] string actor, [FromQuery] string motivo)
        {
            try
            {
                if (usuario == null)
                {
                    return BadRequest(new { message = "El device user no puede ser nulo" });
                }

                if (!MotivoValido(motivo))
                {
                    return BadRequest(new { message = "El motivo es obligatorio y debe tener al menos 6 caracteres" });
                }

                var rowsAffected = await _unitOfWork.AdminRepository.UpdateSubUser(usuario);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Device user no encontrado" });
                }

                await _unitOfWork.AdminRepository.RegistrarAuditoria(actor, "SubUsuarios", "Actualizar", usuario.UserId, $"Se actualizó el subusuario {usuario.UserId}. Motivo: {motivo.Trim()}");

                return Ok(new { message = "Device user actualizado correctamente", rowsAffected });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el device user", error = ex.Message });
            }
        }

        [HttpDelete("DeleteDeviceUser/{id}")]
        public async Task<IActionResult> DeleteSubUser(string id, [FromQuery] string actor, [FromQuery] string motivo)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "El id no puede ser nulo o vacío" });
                }

                if (!MotivoValido(motivo))
                {
                    return BadRequest(new { message = "El motivo es obligatorio y debe tener al menos 6 caracteres" });
                }

                var rowsAffected = await _unitOfWork.AdminRepository.DeleteSubUser(id);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Device user no encontrado" });
                }

                await _unitOfWork.AdminRepository.RegistrarAuditoria(actor, "SubUsuarios", "Eliminar", id, $"Se eliminó el subusuario {id}. Motivo: {motivo.Trim()}");

                return Ok(new { message = "Device user eliminado correctamente", id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el device user", error = ex.Message });
            }
        }

        [HttpGet("GetDevices")]
        public async Task<IActionResult> GetDevices()
        {
            try
            {
                var devices = await _unitOfWork.AdminRepository.GetDevices();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las unidades", error = ex.Message });
            }
        }

        [HttpPut("UpdateDevice")]
        public async Task<IActionResult> UpdateDevice([FromBody] DeviceAdmin device, string oldDeviceID, string oldAccountID, [FromQuery] string actor, [FromQuery] string motivo)
        {
            try
            {
                if (!MotivoValido(motivo))
                {
                    return BadRequest(new { message = "El motivo es obligatorio y debe tener al menos 6 caracteres" });
                }

                var resultado = await _unitOfWork.AdminRepository.UpdateDevice(device, oldDeviceID, oldAccountID);

                if (resultado == 0)
                {
                    return NotFound(new { message = "Dispositivo no encontrado" });
                }

                await _unitOfWork.AdminRepository.RegistrarAuditoria(actor, "Unidades", "Actualizar", device.DeviceID, $"Se actualizó la unidad {device.DeviceID}. Motivo: {motivo.Trim()}");

                return Ok(new { message = "Dispositivo actualizado correctamente", rowsAffected = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el dispositivo", error = ex.Message });
            }
        }

        [HttpPost("InsertDevice")]
        public async Task<IActionResult> InsertDevice([FromBody] DeviceAdmin device, [FromQuery] string actor, [FromQuery] string motivo)
        {
            try
            {
                if (!MotivoValido(motivo))
                {
                    return BadRequest(new { message = "El motivo es obligatorio y debe tener al menos 6 caracteres" });
                }

                var resultado = await _unitOfWork.AdminRepository.InsertDevice(device);

                if (resultado == 0)
                {
                    return BadRequest(new { message = "No se pudo crear el dispositivo" });
                }

                await _unitOfWork.AdminRepository.RegistrarAuditoria(actor, "Unidades", "Crear", device.DeviceID, $"Se creó la unidad {device.DeviceID}. Motivo: {motivo.Trim()}");

                return Ok(new { message = "Dispositivo creado correctamente", rowsAffected = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el dispositivo", error = ex.Message });
            }
        }

        [HttpDelete("DeleteDevice/{deviceID}/{accountID}")]
        public async Task<IActionResult> DeleteDevice(string deviceID, string accountID, [FromQuery] string actor, [FromQuery] string motivo)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceID) || string.IsNullOrEmpty(accountID))
                {
                    return BadRequest(new { message = "El deviceID y accountID no pueden ser nulos o vacíos" });
                }

                if (!MotivoValido(motivo))
                {
                    return BadRequest(new { message = "El motivo es obligatorio y debe tener al menos 6 caracteres" });
                }

                var rowsAffected = await _unitOfWork.AdminRepository.DeleteDevice(deviceID, accountID);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Dispositivo no encontrado" });
                }

                await _unitOfWork.AdminRepository.RegistrarAuditoria(actor, "Unidades", "Eliminar", deviceID, $"Se eliminó la unidad {deviceID}. Motivo: {motivo.Trim()}");

                return Ok(new { message = "Dispositivo eliminado correctamente", deviceID, accountID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el dispositivo", error = ex.Message });
            }
        }

        [HttpGet("GetDevicesConex")]
        public async Task<IActionResult> GetConexDesconex()
        {
            try
            {
                var devices = await _unitOfWork.AdminRepository.GetConexDesconex();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las unidades", error = ex.Message });
            }
        }

        [HttpGet("GetAuditoriaGeneral")]
        public async Task<IActionResult> GetAuditoriaGeneral(int limit = 200, string modulo = null, string usuario = null)
        {
            try
            {
                var registros = await _unitOfWork.AdminRepository.GetAuditoriaGeneral(limit, modulo, usuario);
                return Ok(registros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la auditoría general", error = ex.Message });
            }
        }
    }
}

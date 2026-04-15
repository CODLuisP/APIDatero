using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using VelsatBackendAPI.Data.Repositories;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{accountID}")]
        public async Task<IActionResult> GetServidor(string accountID)
        {
            var server = await _unitOfWork.ServidorRepository.GetServidor(accountID);

            if (server == null)
            {
                return Ok(new { mensaje = "usuario incorrecto" });
            }

            return Ok(new { Servidor = server.servidor });
        }
    }
}

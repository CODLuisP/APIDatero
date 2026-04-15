using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model;

namespace VelsatBackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DeviceListController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeviceListController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> ObtenerDatosCargaInicial(string username)
        {
            var datosCargaInicial = await ObtenerDatosCargaDesdeBD(username);
            var fechaActual = DateTime.Now;
            datosCargaInicial.FechaActual = fechaActual;
            return Ok(datosCargaInicial);
        }

        private async Task<DatosCargainicial> ObtenerDatosCargaDesdeBD(string username)
        {
            var datosCargaInicial = await _unitOfWork.DatosCargainicialService.ObtenerDatosCargaInicialAsync(username);
            return datosCargaInicial;
        }

        [HttpGet("simplified/{username}")]
        public async Task<IActionResult> SimplifiedList(string username)
        {
            var datosCargaInicial = await _unitOfWork.DatosCargainicialService.SimplifiedList(username);
            return Ok(datosCargaInicial);
        }

        [HttpGet("Unidad/{username}/{placa}")]
        public async Task<IActionResult> ObtenerDeviceList(string username, string placa)
        {
            var datosDeviceList = await _unitOfWork.DatosCargainicialService.ObtenerDatosVehiculoAsync(username, placa);
            datosDeviceList.FechaActual = DateTime.Now;
            return Ok(datosDeviceList);
        }
    }
}
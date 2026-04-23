using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VelsatBackendAPI.Data.Repositories;
using System.Data;
using VelsatBackendAPI.Model;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using ClosedXML.Excel.Drawings;
using System.Drawing;
using VelsatBackendAPI.Data.Services;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ReportingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //Reporte general y excel
        [HttpGet("general/{fechaini}/{fechafin}/{deviceID}/{accountID}")]
        public async Task<IActionResult> GetDataReporting(string fechaini, string fechafin, string deviceID, string accountID)
        {
            try
            {
                var resultado = await _unitOfWork.HistoricosRepository.GetDataReporting(fechaini, fechafin, deviceID, accountID);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("downloadExcelG/{fechaini}/{fechafin}/{deviceID}/{accountID}")]
        public async Task<IActionResult> DownloadExcelG(string fechaini, string fechafin, string deviceID, string accountID)
        {
            var datos = await _unitOfWork.HistoricosRepository.GetDataReporting(fechaini, fechafin, deviceID, accountID);

            var user = _unitOfWork.HistoricosRepository.UserName(deviceID) ?? "SIN USUARIO";
            var excelBytes = ConvertDataExcel(datos.ListaTablas, fechaini, fechafin, deviceID, user);
            string fileName = $"reporte_general_gps_{deviceID}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private byte[] ConvertDataExcel(List<TablasReporting> datos, string fechaini, string fechafin, string deviceID, string user)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DatosGps");

                worksheet.Cell(12, 2).Value = "ITEM";
                worksheet.Cell(12, 3).Value = "FECHA";
                worksheet.Cell(12, 4).Value = "HORA";
                worksheet.Cell(12, 5).Value = "VELOCIDAD";
                worksheet.Cell(12, 6).Value = "LATITUD";
                worksheet.Cell(12, 7).Value = "LONGITUD";
                worksheet.Cell(12, 8).Value = "ODÓMETRO";
                worksheet.Cell(12, 9).Value = "UBICACIÓN";

                bool colorAzul = true;

                for (int i = 0; i < datos.Count; i++)
                {
                    var gps = datos[i];

                }

                worksheet.Range("B9:C9").Merge();
                worksheet.Cell(9, 2).Value = "Fecha de Inicio:";
                worksheet.Cell(9, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(9, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(9, 2).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(9, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(9, 2).Style.Font.FontName = "Cambria";
                worksheet.Cell(9, 2).Style.Font.FontSize = 10;
                worksheet.Cell(9, 2).Style.Font.SetBold();

                worksheet.Range("B10:C10").Merge();
                worksheet.Cell(10, 2).Value = "Fecha de Fin:";
                worksheet.Cell(10, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(10, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(10, 2).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(10, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(10, 2).Style.Font.FontName = "Cambria";
                worksheet.Cell(10, 2).Style.Font.FontSize = 10;
                worksheet.Cell(10, 2).Style.Font.SetBold();

                string fechainiFormateada = DateTime.Parse(fechaini).ToString("dd/MM/yyyy  HH:mm");
                worksheet.Range("D9:E9").Merge();
                worksheet.Cell(9, 4).Value = fechainiFormateada;
                worksheet.Cell(9, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(9, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(9, 4).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(9, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(9, 4).Style.Font.FontName = "Cambria";
                worksheet.Cell(9, 4).Style.Font.FontSize = 10;
                worksheet.Cell(9, 4).Style.Font.SetBold();

                string fechafinFormateada = DateTime.Parse(fechafin).ToString("dd/MM/yyyy  HH:mm");
                worksheet.Range("D10:E10").Merge();
                worksheet.Cell(10, 4).Value = fechafinFormateada;
                worksheet.Cell(10, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(10, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(10, 4).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(10, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(10, 4).Style.Font.FontName = "Cambria";
                worksheet.Cell(10, 4).Style.Font.FontSize = 10;
                worksheet.Cell(10, 4).Style.Font.SetBold();

                worksheet.Range("B10:E10").Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Range("B10:E10").Style.Border.BottomBorderColor = XLColor.FromHtml("#1a3446");
                worksheet.Range("F10:I10").Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Range("F10:I10").Style.Border.BottomBorderColor = XLColor.FromHtml("#1a3446");

                var rangoCeldas = worksheet.Range("B4:H7");
                rangoCeldas.Merge();
                rangoCeldas.Value = "REPORTE GENERAL DE LA UNIDAD: " + deviceID.ToUpper();
                rangoCeldas.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangoCeldas.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                rangoCeldas.Style.Font.FontColor = XLColor.White;
                rangoCeldas.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                rangoCeldas.Style.Font.FontName = "Calibri";
                rangoCeldas.Style.Font.FontSize = 16;
                rangoCeldas.Style.Font.SetBold();

                worksheet.Cell("I9").Value = "Generado el " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
                worksheet.Cell("I9").Style.Font.FontName = "Cambria";
                worksheet.Cell("I9").Style.Font.FontSize = 10;
                worksheet.Cell("I9").Style.Font.SetBold();
                worksheet.Cell("I9").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("I9").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Cell("I10").Value = "USUARIO : " + user.ToUpper();
                worksheet.Cell("I10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("I10").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell("I10").Style.Font.FontName = "Cambria";
                worksheet.Cell("I10").Style.Font.FontSize = 10;
                worksheet.Cell("I10").Style.Font.SetBold();

                string imagePath = "C:\\inetpub\\wwwroot\\CarLogo.jpg";
                var image = worksheet.AddPicture(imagePath).MoveTo(worksheet.Cell("B4")).WithSize(81, 81);


                var mergedRange = worksheet.Range("I4:I7");
                mergedRange.Merge();
                mergedRange.Style.Fill.BackgroundColor = XLColor.FromColor(System.Drawing.Color.FromArgb(224, 224, 224));
                mergedRange.Merge().Style.Alignment.WrapText = true;
                mergedRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                mergedRange.Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                string imagePath2 = "C:\\inetpub\\wwwroot\\VelsatLogo.png";
                var image2 = worksheet.AddPicture(imagePath2).MoveTo(worksheet.Cell("I4"), new System.Drawing.Point(100, 0)).WithSize(240, 80);


                worksheet.Row(12).Height = 40;
                for (int i = 2; i <= 9; i++)
                {
                    worksheet.Cell(12, i).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                    worksheet.Cell(12, i).Style.Font.Bold = true;
                    worksheet.Cell(12, i).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(12, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(12, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                for (int i = 0; i < datos.Count; i++)
                {
                    var gps = datos[i];

                    string filaColorHex = colorAzul ? "#EDEFF2" : "#FFFFFF";
                    XLColor filaColor = XLColor.FromHtml(filaColorHex);


                    for (int j = 2; j <= 9; j++)
                    {
                        worksheet.Cell(i + 13, j).Value =
                            j == 2 ? (i + 1) :
                            (j == 3 ? gps.Fecha :
                            (j == 4 ? gps.Hora :
                            (j == 5 ? gps.SpeedKPH.ToString("0.0") + " KM/H" :
                            (j == 6 ? Math.Round(gps.Latitude, 5) :
                            (j == 7 ? Math.Round(gps.Longitude, 5) :
                            (j == 8 ? Math.Round(gps.OdometerKM, 2) :
                            gps.Address))))));

                        worksheet.Cell(i + 13, j).Style.Fill.BackgroundColor = filaColor;
                        worksheet.Cell(i + 13, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(i + 13, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        if (j == 9)
                        {
                            worksheet.Cell(i + 13, j).Style.Alignment.WrapText = true;
                        }
                    }

                    colorAzul = !colorAzul;

                }

                worksheet.ShowGridLines = false;

                worksheet.Column(2).Width = 7;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 18;
                worksheet.Column(6).Width = 18;
                worksheet.Column(7).Width = 18;
                worksheet.Column(8).Width = 18;
                worksheet.Column(9).Width = 65;

                var tableRange = worksheet.Range(worksheet.Cell(12, 2), worksheet.LastCellUsed(XLCellsUsedOptions.All));

                // Centra horizontal y verticalmente toda la tabla

                tableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int ultimaFila = 12 + datos.Count + 1;

                worksheet.Cell(ultimaFila, 2).Value = "Datos obtenidos del Sistema de Rastreo Vehicular de www.velsat.com.pe";

                worksheet.Range(ultimaFila, 2, ultimaFila, 8).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Range(ultimaFila, 2, ultimaFila, 8).Merge().Style.Font.Italic = true;

                worksheet.Cell(ultimaFila, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");

                worksheet.Cell(ultimaFila, 2).Style.Font.FontColor = XLColor.White;


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        [HttpGet("speed/{fechaini}/{fechafin}/{deviceId}/{speedKPH}/{accountID}")]
        public async Task<IActionResult> GetDataSpeed(string fechaini, string fechafin, string deviceId, double speedKPH, string accountID)
        {
            try
            {
                var resultado = await _unitOfWork.HistoricosRepository.GetSpeedData(fechaini, fechafin, deviceId, speedKPH, accountID);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        //Datos de las velocidades y excel
        [HttpGet("downloadExcelV/{fechaini}/{fechafin}/{deviceId}/{speedKPH}/{accountID}")]
        public async Task<IActionResult> DownloadExcelV(string fechaini, string fechafin, string deviceId, double speedKPH, string accountID)
        {
            var datos = await _unitOfWork.HistoricosRepository.GetSpeedData(fechaini, fechafin, deviceId, speedKPH, accountID);

            var user = _unitOfWork.HistoricosRepository.UserName(deviceId);
            var excelBytes = ConvertSpeedDataExcel(datos, fechaini, fechafin, deviceId, user);
            string fileName = $"reporte_velocidad_gps_{deviceId}.xlsx";


            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private byte[] ConvertSpeedDataExcel(List<SpeedReporting> speedData, string fechaini, string fechafin, string deviceId, string user)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SpeedData");

                worksheet.Cell(12, 2).Value = "ITEM";
                worksheet.Cell(12, 3).Value = "VELOCIDAD";
                worksheet.Cell(12, 4).Value = "FECHA";
                worksheet.Cell(12, 5).Value = "HORA";
                worksheet.Cell(12, 6).Value = "LATITUD";
                worksheet.Cell(12, 7).Value = "LONGITUD";
                worksheet.Cell(12, 8).Value = "UBICACIÓN";

                worksheet.Range("H12:J12").Merge();


                bool colorAzul = true;

                for (int i = 0; i < speedData.Count; i++)
                {
                    var speed = speedData[i];

                    worksheet.Cell(i + 13, 2).Value = speed.Item;
                    worksheet.Cell(i + 13, 3).Value = speed.SpeedKPH;
                    worksheet.Cell(i + 13, 4).Value = speed.Date;
                    worksheet.Cell(i + 13, 5).Value = speed.Time;
                    worksheet.Cell(i + 13, 6).Value = Math.Round(speed.Latitude, 5);
                    worksheet.Cell(i + 13, 7).Value = Math.Round(speed.Longitude, 5);
                    worksheet.Cell(i + 13, 8).Value = speed.Address;

                    worksheet.Range(i + 13, 8, i + 13, 10).Merge();

                }

                worksheet.Range("B9:C9").Merge();
                worksheet.Cell(9, 2).Value = "Fecha de Inicio:";
                worksheet.Cell(9, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(9, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(9, 2).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(9, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(9, 2).Style.Font.FontName = "Cambria";
                worksheet.Cell(9, 2).Style.Font.FontSize = 10;
                worksheet.Cell(9, 2).Style.Font.SetBold();

                worksheet.Range("B10:C10").Merge();
                worksheet.Cell(10, 2).Value = "Fecha de Fin:";
                worksheet.Cell(10, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(10, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(10, 2).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(10, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(10, 2).Style.Font.FontName = "Cambria";
                worksheet.Cell(10, 2).Style.Font.FontSize = 10;
                worksheet.Cell(10, 2).Style.Font.SetBold();

                worksheet.Range("B10:E10").Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Range("B10:E10").Style.Border.BottomBorderColor = XLColor.FromHtml("#1a3446");

                string fechainiFormateada = DateTime.Parse(fechaini).ToString("dd/MM/yyyy  HH:mm");
                worksheet.Range("D9:E9").Merge();
                worksheet.Cell(9, 4).Value = fechainiFormateada;
                worksheet.Cell(9, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(9, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(9, 4).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(9, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(9, 4).Style.Font.FontName = "Cambria";
                worksheet.Cell(9, 4).Style.Font.FontSize = 10;
                worksheet.Cell(9, 4).Style.Font.SetBold();

                string fechafinFormateada = DateTime.Parse(fechafin).ToString("dd/MM/yyyy  HH:mm");
                worksheet.Range("D10:E10").Merge();
                worksheet.Cell(10, 4).Value = fechafinFormateada;
                worksheet.Cell(10, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(10, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(10, 4).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(10, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(10, 4).Style.Font.FontName = "Cambria";
                worksheet.Cell(10, 4).Style.Font.FontSize = 10;
                worksheet.Cell(10, 4).Style.Font.SetBold();

                worksheet.Range("F10:J10").Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Range("F10:J10").Style.Border.BottomBorderColor = XLColor.FromHtml("#1a3446");

                var rangoCeldas = worksheet.Range("B4:H7");
                rangoCeldas.Merge();
                rangoCeldas.Value = "REPORTE DE VELOCIDAD DE LA UNIDAD: " + deviceId.ToUpper();
                rangoCeldas.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangoCeldas.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                rangoCeldas.Style.Font.FontColor = XLColor.White;
                rangoCeldas.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                rangoCeldas.Style.Font.FontName = "Calibri";
                rangoCeldas.Style.Font.FontSize = 16;
                rangoCeldas.Style.Font.SetBold();

                worksheet.Cell("J9").Value = "Generado el " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
                worksheet.Cell("J9").Style.Font.FontName = "Cambria";
                worksheet.Cell("J9").Style.Font.FontSize = 10;
                worksheet.Cell("J9").Style.Font.SetBold();
                worksheet.Cell("J9").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("J9").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Cell("J10").Value = "USUARIO : " + user.ToUpper();
                worksheet.Cell("J10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("J10").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell("J10").Style.Font.FontName = "Cambria";
                worksheet.Cell("J10").Style.Font.FontSize = 10;
                worksheet.Cell("J10").Style.Font.SetBold();

                string imagePath = "C:\\inetpub\\wwwroot\\CarLogo.jpg";
                var image = worksheet.AddPicture(imagePath).MoveTo(worksheet.Cell("B4")).WithSize(81, 81);


                var mergedRange = worksheet.Range("I4:J7");
                mergedRange.Merge();
                mergedRange.Style.Fill.BackgroundColor = XLColor.FromColor(System.Drawing.Color.FromArgb(224, 224, 224));
                mergedRange.Merge().Style.Alignment.WrapText = true;
                mergedRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                mergedRange.Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                string imagePath2 = "C:\\inetpub\\wwwroot\\VelsatLogo.png";
                var image2 = worksheet.AddPicture(imagePath2).MoveTo(worksheet.Cell("I4")).WithSize(240, 80).MoveTo(815, 60);

                worksheet.Row(12).Height = 40;
                for (int i = 2; i <= 10; i++)
                {
                    worksheet.Cell(12, i).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                    worksheet.Cell(12, i).Style.Font.Bold = true;
                    worksheet.Cell(12, i).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(12, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(12, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                for (int i = 0; i < speedData.Count; i++)
                {
                    var gps = speedData[i];

                    string filaColorHex = colorAzul ? "#EDEFF2" : "#FFFFFF";
                    XLColor filaColor = XLColor.FromHtml(filaColorHex);

                    for (int j = 2; j <= 10; j++)
                    {
                        worksheet.Cell(i + 13, j).Style.Fill.BackgroundColor = filaColor;
                        worksheet.Cell(i + 13, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(i + 13, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        if (j == 8)
                        {
                            worksheet.Cell(i + 13, j).Style.Alignment.WrapText = true;
                        }
                    }

                    colorAzul = !colorAzul;

                    colorAzul = !colorAzul;

                }

                worksheet.ShowGridLines = false;

                worksheet.Column(2).Width = 7;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 15;
                worksheet.Column(7).Width = 18;
                worksheet.Column(8).Width = 17;
                worksheet.Column(9).Width = 26;
                worksheet.Column(10).Width = 7;

                var tableRange = worksheet.Range(worksheet.Cell(12, 2), worksheet.LastCellUsed(XLCellsUsedOptions.All));

                tableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int ultimaFila = 13 + speedData.Count;

                worksheet.Cell(ultimaFila, 2).Value = "Datos obtenidos del Sistema de Rastreo Vehicular de www.velsat.com.pe";

                worksheet.Range(ultimaFila, 2, ultimaFila, 10).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Range(ultimaFila, 2, ultimaFila, 10).Merge().Style.Font.Italic = true;

                worksheet.Cell(ultimaFila, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");

                worksheet.Cell(ultimaFila, 2).Style.Font.FontColor = XLColor.White;



                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        //Datos de las paradas y excel
        [HttpGet("stops/{fechaini}/{fechafin}/{deviceId}/{accountID}")]
        public async Task<IActionResult> GetDataStops(string fechaini, string fechafin, string deviceId, string accountID)
        {
            try
            {
                var resultado = await _unitOfWork.HistoricosRepository.GetStopData(fechaini, fechafin, deviceId, accountID);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("downloadExcelS/{fechaini}/{fechafin}/{deviceID}/{accountID}")]
        public async Task<IActionResult> DownloadExcelS(string fechaini, string fechafin, string deviceID, string accountID)
        {

            var datos = await _unitOfWork.HistoricosRepository.GetStopData(fechaini, fechafin, deviceID, accountID);

            var user = _unitOfWork.HistoricosRepository.UserName(deviceID);
            var excelBytes = ConvertStopDataExcel(datos, fechaini, fechafin, deviceID, user);
            string fileName = $"reporte_paradas_gps_{deviceID}.xlsx";


            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private byte[] ConvertStopDataExcel(List<StopsReporting> stopsData, string fechaini, string fechafin, string deviceId, string user)
        {

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("StopsData");

                worksheet.Cell(12, 2).Value = "ITEM";
                worksheet.Cell(12, 3).Value = "FECHA INICIO";
                worksheet.Cell(12, 4).Value = "HORA INICIO";
                worksheet.Cell(12, 5).Value = "FECHA FINAL";
                worksheet.Cell(12, 6).Value = "HORA FINAL";
                worksheet.Cell(12, 7).Value = "TIEMPO TOTAL";
                worksheet.Cell(12, 8).Value = "LATITUD";
                worksheet.Cell(12, 9).Value = "LONGITUD";
                worksheet.Cell(12, 10).Value = "UBICACIÓN";

                bool colorAzul = true;

                for (int i = 0; i < stopsData.Count; i++)
                {
                    var stop = stopsData[i];

                    worksheet.Cell(i + 13, 2).Value = stop.Item;
                    worksheet.Cell(i + 13, 3).Value = stop.StartDate;
                    worksheet.Cell(i + 13, 4).Value = stop.StartTime;
                    worksheet.Cell(i + 13, 5).Value = stop.EndDate;
                    worksheet.Cell(i + 13, 6).Value = stop.EndTime;
                    worksheet.Cell(i + 13, 7).Value = stop.TotalTime;
                    worksheet.Cell(i + 13, 8).Value = Math.Round(stop.Latitude, 5);
                    worksheet.Cell(i + 13, 9).Value = Math.Round(stop.Longitude, 5);
                    worksheet.Cell(i + 13, 10).Value = stop.Address;

                }

                worksheet.Range("B9:C9").Merge();
                worksheet.Cell(9, 2).Value = "Fecha de Inicio:";
                worksheet.Cell(9, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(9, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(9, 2).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(9, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(9, 2).Style.Font.FontName = "Cambria";
                worksheet.Cell(9, 2).Style.Font.FontSize = 10;
                worksheet.Cell(9, 2).Style.Font.SetBold();

                worksheet.Range("B10:C10").Merge();
                worksheet.Cell(10, 2).Value = "Fecha de Fin:";
                worksheet.Cell(10, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(10, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(10, 2).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(10, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(10, 2).Style.Font.FontName = "Cambria";
                worksheet.Cell(10, 2).Style.Font.FontSize = 10;
                worksheet.Cell(10, 2).Style.Font.SetBold();

                worksheet.Range("B10:E10").Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Range("B10:E10").Style.Border.BottomBorderColor = XLColor.FromHtml("#1a3446");

                string fechainiFormateada = DateTime.Parse(fechaini).ToString("dd/MM/yyyy  HH:mm");
                worksheet.Range("D9:E9").Merge();
                worksheet.Cell(9, 4).Value = fechainiFormateada;
                worksheet.Cell(9, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(9, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(9, 4).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(9, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(9, 4).Style.Font.FontName = "Cambria";
                worksheet.Cell(9, 4).Style.Font.FontSize = 10;
                worksheet.Cell(9, 4).Style.Font.SetBold();

                string fechafinFormateada = DateTime.Parse(fechafin).ToString("dd/MM/yyyy  HH:mm");
                worksheet.Range("D10:E10").Merge();
                worksheet.Cell(10, 4).Value = fechafinFormateada;
                worksheet.Cell(10, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(10, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(10, 4).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(10, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                worksheet.Cell(10, 4).Style.Font.FontName = "Cambria";
                worksheet.Cell(10, 4).Style.Font.FontSize = 10;
                worksheet.Cell(10, 4).Style.Font.SetBold();

                worksheet.Range("F10:J10").Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Range("F10:J10").Style.Border.BottomBorderColor = XLColor.FromHtml("#1a3446");

                var rangoCeldas = worksheet.Range("B4:H7");
                rangoCeldas.Merge();
                rangoCeldas.Value = "REPORTE DE PARADAS DE LA UNIDAD: " + deviceId.ToUpper();
                rangoCeldas.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangoCeldas.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                rangoCeldas.Style.Font.FontColor = XLColor.White;
                rangoCeldas.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                rangoCeldas.Style.Font.FontName = "Calibri";
                rangoCeldas.Style.Font.FontSize = 16;
                rangoCeldas.Style.Font.SetBold();

                worksheet.Cell("J9").Value = "Generado el " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
                worksheet.Cell("J9").Style.Font.FontName = "Cambria";
                worksheet.Cell("J9").Style.Font.FontSize = 10;
                worksheet.Cell("J9").Style.Font.SetBold();
                worksheet.Cell("J9").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("J9").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Cell("J10").Value = "USUARIO : " + user.ToUpper();
                worksheet.Cell("J10").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("J10").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell("J10").Style.Font.FontName = "Cambria";
                worksheet.Cell("J10").Style.Font.FontSize = 10;
                worksheet.Cell("J10").Style.Font.SetBold();

                string imagePath = "C:\\inetpub\\wwwroot\\CarLogo.jpg";
                var image = worksheet.AddPicture(imagePath).MoveTo(worksheet.Cell("B4")).WithSize(81, 81);


                var mergedRange = worksheet.Range("I4:J7");
                mergedRange.Merge();
                mergedRange.Style.Fill.BackgroundColor = XLColor.FromColor(System.Drawing.Color.FromArgb(224, 224, 224));
                mergedRange.Merge().Style.Alignment.WrapText = true;
                mergedRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                mergedRange.Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                string imagePath2 = "C:\\inetpub\\wwwroot\\VelsatLogo.png";
                var image2 = worksheet.AddPicture(imagePath2).MoveTo(worksheet.Cell("I4")).WithSize(240, 80).MoveTo(960, 60);

                worksheet.Row(12).Height = 40;
                for (int i = 2; i <= 10; i++)
                {
                    worksheet.Cell(12, i).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                    worksheet.Cell(12, i).Style.Font.Bold = true;
                    worksheet.Cell(12, i).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(12, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(12, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                for (int i = 0; i < stopsData.Count; i++)
                {
                    var gps = stopsData[i];

                    string filaColorHex = colorAzul ? "#EDEFF2" : "#FFFFFF";
                    XLColor filaColor = XLColor.FromHtml(filaColorHex);

                    for (int j = 2; j <= 10; j++)
                    {
                        worksheet.Cell(i + 13, j).Style.Fill.BackgroundColor = filaColor;
                        worksheet.Cell(i + 13, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(i + 13, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        if (j == 8)
                        {
                            worksheet.Cell(i + 13, j).Style.Alignment.WrapText = true;
                        }
                    }

                    colorAzul = !colorAzul;

                    colorAzul = !colorAzul;

                }

                worksheet.ShowGridLines = false;

                worksheet.Column(2).Width = 7;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 15;
                worksheet.Column(7).Width = 18;
                worksheet.Column(8).Width = 18;
                worksheet.Column(9).Width = 18;
                worksheet.Column(10).Width = 55;

                var tableRange = worksheet.Range(worksheet.Cell(12, 2), worksheet.LastCellUsed(XLCellsUsedOptions.All));

                tableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int ultimaFila = 13 + stopsData.Count;

                worksheet.Cell(ultimaFila, 2).Value = "Datos obtenidos del Sistema de Rastreo Vehicular de www.velsat.com.pe";

                worksheet.Range(ultimaFila, 2, ultimaFila, 10).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Range(ultimaFila, 2, ultimaFila, 10).Merge().Style.Font.Italic = true;

                worksheet.Cell(ultimaFila, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");

                worksheet.Cell(ultimaFila, 2).Style.Font.FontColor = XLColor.White;


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        [HttpGet("details/{fechaini}/{fechafin}/{deviceId}/{accountID}")]
        public async Task<IActionResult> GetRouteDetails(string fechaini, string fechafin, string deviceId, string accountID)
        {
            try
            {
                var resultado = await _unitOfWork.HistoricosRepository.GetRouteDetails(fechaini, fechafin, deviceId, accountID);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

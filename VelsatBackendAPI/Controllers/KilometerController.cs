using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class KilometerController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;

        public KilometerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        private async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetByteArrayAsync(imageUrl);
            }
        }

        [HttpGet("kilometer/{fechaini}/{fechafin}/{deviceID}/{accountID}")]
        public async Task<IActionResult> GetReportingKilometer(string fechaini, string fechafin, string deviceID, string accountID)
        {
            var result = await _unitOfWork.KilometrosRepository.GetKmReporting(fechaini, fechafin, deviceID, accountID);
            return Ok(result);
        }

        [HttpGet("downloadExcelK/{fechaini}/{fechafin}/{deviceID}/{accountID}")]
        public async Task<IActionResult> DownloadExcelK(string fechaini, string fechafin, string deviceID, string accountID)
        {

            var datos = await _unitOfWork.KilometrosRepository.GetKmReporting(fechaini, fechafin, deviceID, accountID);

            var excelBytes = await ConvertKilometerDataExcel(datos.ListaKilometros, fechaini, fechafin, deviceID);
            string fileName = $"reporte_kilometros_gps.xlsx";


            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task<byte[]> ConvertKilometerDataExcel(List<KilometrosRecorridos> datos, string fechaini, string fechafin, string deviceID)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DatosGps");

                worksheet.Cell(12, 2).Value = "ITEM";
                worksheet.Cell(12, 3).Value = "UNIDAD";
                worksheet.Cell(12, 4).Value = "KILOMETROS";

                bool colorAzul = true;

                for (int i = 0; i < datos.Count; i++)
                {
                    var gps = datos[i];

                    string filaColorHex = colorAzul ? "#EDEFF2" : "#FFFFFF";
                    XLColor filaColor = XLColor.FromHtml(filaColorHex);

                    worksheet.Cell(i + 13, 2).Value = i + 1;
                    worksheet.Cell(i + 13, 3).Value = gps.DeviceId;
                    worksheet.Cell(i + 13, 4).Value = Math.Round(gps.Kilometros, 2) + " Km";

                    for (int j = 2; j <= 4; j++)
                    {
                        worksheet.Cell(i + 13, j).Style.Fill.BackgroundColor = filaColor;
                        worksheet.Cell(i + 13, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(i + 13, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    colorAzul = !colorAzul;
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

                worksheet.Range("F10:H10").Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Range("F10:H10").Style.Border.BottomBorderColor = XLColor.FromHtml("#1a3446");

                var rangoCeldas = worksheet.Range("B4:G7");
                rangoCeldas.Merge();
                rangoCeldas.Value = "REPORTE DE KILÓMETROS RECORRIDOS";
                rangoCeldas.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangoCeldas.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                rangoCeldas.Style.Font.FontColor = XLColor.White;
                rangoCeldas.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                rangoCeldas.Style.Font.FontName = "Calibri";
                rangoCeldas.Style.Font.FontSize = 16;
                rangoCeldas.Style.Font.SetBold();

                worksheet.Cell("H9").Value = "Generado el " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
                worksheet.Cell("H9").Style.Font.FontName = "Cambria";
                worksheet.Cell("H9").Style.Font.FontSize = 10;
                worksheet.Cell("H9").Style.Font.SetBold();
                worksheet.Cell("H9").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("H9").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                string imageUrl1 = "https://imagedelivery.net/o0E1jB_kGKnYacpYCBFmZA/e880b9a3-e8f9-4278-9d06-6c2f661b8800/public";
                byte[] imageBytes1 = await DownloadImageAsync(imageUrl1);
                using (var ms1 = new MemoryStream(imageBytes1))
                {
                    var image = worksheet.AddPicture(ms1).MoveTo(worksheet.Cell("B4")).WithSize(81, 81);
                }

                var mergedRange = worksheet.Range("H4:H7");
                mergedRange.Merge();
                mergedRange.Style.Fill.BackgroundColor = XLColor.FromColor(System.Drawing.Color.FromArgb(224, 224, 224));
                mergedRange.Merge().Style.Alignment.WrapText = true;
                mergedRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                mergedRange.Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                string imageUrl2 = "https://imagedelivery.net/o0E1jB_kGKnYacpYCBFmZA/5fb05ad0-957b-4de1-ca5a-3eb24882fa00/public";
                byte[] imageBytes2 = await DownloadImageAsync(imageUrl2);
                using (var ms2 = new MemoryStream(imageBytes2))
                {
                    var image2 = worksheet.AddPicture(ms2).MoveTo(worksheet.Cell("I4"), new System.Drawing.Point(100, 0)).WithSize(240, 80);
                }

                worksheet.Row(12).Height = 40;
                for (int i = 2; i <= 4; i++)
                {
                    worksheet.Cell(12, i).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                    worksheet.Cell(12, i).Style.Font.Bold = true;
                    worksheet.Cell(12, i).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(12, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(12, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                worksheet.ShowGridLines = false;

                worksheet.Column(2).Width = 7;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 18;
                worksheet.Column(6).Width = 18;
                worksheet.Column(7).Width = 18;
                worksheet.Column(8).Width = 55;

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

        [HttpGet("kilometerall/{fechaini}/{fechafin}/{accountID}")]
        public async Task<IActionResult> GetAllKilometer(string fechaini, string fechafin, string accountID)
        {
            var result = await _unitOfWork.KilometrosRepository.GetAllKmReporting(fechaini, fechafin, accountID);
            return Ok(result);
        }

        [HttpGet("downloadExcelKall/{fechaini}/{fechafin}/{accountID}")]
        public async Task<IActionResult> DownloadExcelKall(string fechaini, string fechafin, string accountID)
        {

            var datos = await _unitOfWork.KilometrosRepository.GetAllKmReporting(fechaini, fechafin, accountID);

            var excelBytes = await ConvertKilometerAllExcel(datos.ListaKilometros, fechaini, fechafin, accountID);
            string fileName = $"reporte_kilometros_gps.xlsx";


            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task<byte[]> ConvertKilometerAllExcel(List<KilometrosRecorridos> listaKilometros, string fechaini, string fechafin, string accountID)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DatosGps");

                worksheet.Cell(12, 2).Value = "ITEM";
                worksheet.Cell(12, 3).Value = "UNIDAD";
                worksheet.Cell(12, 4).Value = "KILOMETROS";

                bool colorAzul = true;

                for (int i = 0; i < listaKilometros.Count; i++)
                {
                    var gps = listaKilometros[i];

                    string filaColorHex = colorAzul ? "#EDEFF2" : "#FFFFFF";
                    XLColor filaColor = XLColor.FromHtml(filaColorHex);

                    worksheet.Cell(i + 13, 2).Value = i + 1;
                    worksheet.Cell(i + 13, 3).Value = gps.DeviceId;
                    worksheet.Cell(i + 13, 4).Value = Math.Round((gps.Maximo - gps.Minimo), 2) + " Km";

                    for (int j = 2; j <= 4; j++)
                    {
                        worksheet.Cell(i + 13, j).Style.Fill.BackgroundColor = filaColor;
                        worksheet.Cell(i + 13, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(i + 13, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    colorAzul = !colorAzul;
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

                worksheet.Range("F10:H10").Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Range("F10:H10").Style.Border.BottomBorderColor = XLColor.FromHtml("#1a3446");

                var rangoCeldas = worksheet.Range("B4:G7");
                rangoCeldas.Merge();
                rangoCeldas.Value = "REPORTE DE KILÓMETROS RECORRIDOS";
                rangoCeldas.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangoCeldas.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                rangoCeldas.Style.Font.FontColor = XLColor.White;
                rangoCeldas.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                rangoCeldas.Style.Font.FontName = "Calibri";
                rangoCeldas.Style.Font.FontSize = 16;
                rangoCeldas.Style.Font.SetBold();

                worksheet.Cell("H9").Value = "Generado el " + DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
                worksheet.Cell("H9").Style.Font.FontName = "Cambria";
                worksheet.Cell("H9").Style.Font.FontSize = 10;
                worksheet.Cell("H9").Style.Font.SetBold();
                worksheet.Cell("H9").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("H9").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                string imageUrl1 = "https://imagedelivery.net/o0E1jB_kGKnYacpYCBFmZA/e880b9a3-e8f9-4278-9d06-6c2f661b8800/public";
                byte[] imageBytes1 = await DownloadImageAsync(imageUrl1);
                using (var ms1 = new MemoryStream(imageBytes1))
                {
                    var image = worksheet.AddPicture(ms1).MoveTo(worksheet.Cell("B4")).WithSize(81, 81);
                }

                var mergedRange = worksheet.Range("H4:H7");
                mergedRange.Merge();
                mergedRange.Style.Fill.BackgroundColor = XLColor.FromColor(System.Drawing.Color.FromArgb(224, 224, 224));
                mergedRange.Merge().Style.Alignment.WrapText = true;
                mergedRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                mergedRange.Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                string imageUrl2 = "https://imagedelivery.net/o0E1jB_kGKnYacpYCBFmZA/5fb05ad0-957b-4de1-ca5a-3eb24882fa00/public";
                byte[] imageBytes2 = await DownloadImageAsync(imageUrl2);
                using (var ms2 = new MemoryStream(imageBytes2))
                {
                    var image2 = worksheet.AddPicture(ms2).MoveTo(worksheet.Cell("I4"), new System.Drawing.Point(100, 0)).WithSize(240, 80);
                }

                worksheet.Row(12).Height = 40;
                for (int i = 2; i <= 4; i++)
                {
                    worksheet.Cell(12, i).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3446");
                    worksheet.Cell(12, i).Style.Font.Bold = true;
                    worksheet.Cell(12, i).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(12, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(12, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                worksheet.ShowGridLines = false;

                worksheet.Column(2).Width = 7;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 18;
                worksheet.Column(6).Width = 18;
                worksheet.Column(7).Width = 18;
                worksheet.Column(8).Width = 55;

                var tableRange = worksheet.Range(worksheet.Cell(12, 2), worksheet.LastCellUsed(XLCellsUsedOptions.All));

                // Centra horizontal y verticalmente toda la tabla
                tableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int ultimaFila = 12 + listaKilometros.Count + 1;

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
    }
}

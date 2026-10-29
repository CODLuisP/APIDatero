namespace VelsatBackendAPI.Model.Administracion
{
    // Mapea la tabla serverprueba (web) de Urbano 125: id, loginusu, servidor.
    public class RedirUsuario
    {
        public int Id { get; set; }
        public string? Loginusu { get; set; }
        public string? Servidor { get; set; }
    }
}

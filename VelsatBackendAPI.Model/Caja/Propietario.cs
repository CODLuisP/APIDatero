namespace VelsatBackendAPI.Model.Caja
{
    /// <summary>
    /// Unidad asignada a un subusuario (tabla deviceuser).
    /// </summary>
    public class PropietarioUnidad
    {
        public int Id { get; set; }
        public string? Placa { get; set; }
        public string? DeviceID { get; set; }
    }

    /// <summary>
    /// Subusuario (propietario) con sus datos de la tabla usuarios y sus unidades.
    /// </summary>
    public class Propietario
    {
        public string UserId { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Password { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public string? Ruc { get; set; }
        public bool? IsActive { get; set; }
        public long? LastLoginTime { get; set; }
        public List<PropietarioUnidad> Unidades { get; set; } = new();
    }

    public class PropietarioUpdateRequest
    {
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Password { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}

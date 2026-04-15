using Dapper;
using System.Data;
using VelsatBackendAPI.Model.Administracion;

namespace VelsatBackendAPI.Data.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AdminRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            Console.WriteLine("[DEBUG] AdminRepository inicializado con IDbConnectionFactory");
        }

        private IDbConnection CreateConnection() => _connectionFactory.CreateConnection();

        public async Task<IEnumerable<Usuarioadmin>> GetAllUsers()
        {
            var sql = @"SELECT accountID, password, contactPhone, contactEmail, description, creationTime, isActive, ruc from usuarios where isActive = 1";

            using var connection = CreateConnection();
            return await connection.QueryAsync<Usuarioadmin>(sql);
        }

        public async Task<int> UpdateUser(Usuarioadmin usuario)
        {
            var sql = @"UPDATE usuarios SET password = @Password, contactPhone = @ContactPhone, contactEmail = @ContactEmail, description = @Description, ruc = @Ruc WHERE accountID = @AccountID";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, usuario);
        }

        public async Task<int> DeleteUser(string accountID)
        {
            var sql = @"UPDATE usuarios SET isActive = 0 WHERE accountID = @AccountID";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, new { AccountID = accountID });
        }

        public async Task<int> InsertUser(Usuarioadmin usuario)
        {
            var peruTime = DateTime.UtcNow.AddHours(-5);
            var unixTimestamp = ((DateTimeOffset)peruTime).ToUnixTimeSeconds();

            usuario.CreationTime = (int)unixTimestamp;
            usuario.IsActive = true;

            using var connection = CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();
            try
            {
                var sqlUsuario = @"INSERT INTO usuarios 
                    (accountID, userID, password, contactPhone, contactEmail, description, creationTime, isActive, ruc) 
                    VALUES 
                    (@AccountID, 'admin', @Password, @ContactPhone, @ContactEmail, @Description, @CreationTime, @IsActive, @Ruc)";

                var resultado = await connection.ExecuteAsync(sqlUsuario, usuario, transaction);

                var sqlServerPrueba = @"INSERT INTO serverprueba (loginusu, servidor) 
                                VALUES (@AccountID, 'https://villa.velsat.pe:8443')";

                await connection.ExecuteAsync(sqlServerPrueba, new { AccountID = usuario.AccountID }, transaction);

                transaction.Commit();

                // INSERTAR EN LA BASE DE DATOS GTS (conexión separada, sin transacción)
                using var gtsConnection = _connectionFactory.GetGtsConnection();
                if (gtsConnection.State != ConnectionState.Open)
                {
                    gtsConnection.Open();
                }

                var sqlServerMobile = @"INSERT INTO servermobile (loginusu, servidor, tipo) 
                                VALUES (@AccountID, 'https://villa.velsat.pe:2087', 'n')";

                await gtsConnection.ExecuteAsync(sqlServerMobile, new { AccountID = usuario.AccountID });

                return resultado;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<Deviceuser>> GetSubUsers()
        {
            var sql = @"SELECT id, UserId, DeviceName, Status, DeviceID from deviceuser WHERE status = '1'";

            using var connection = CreateConnection();
            return await connection.QueryAsync<Deviceuser>(sql);
        }

        public async Task<int> InsertSubUser(Deviceuser usuario)
        {
            var sql = @"INSERT INTO deviceuser (id, UserId, DeviceName, Status, DeviceID) VALUES (@Id, @UserId, @DeviceName, '1', @DeviceID)";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, usuario);
        }

        public async Task<int> UpdateSubUser(Deviceuser usuario)
        {
            var sql = @"UPDATE deviceuser SET UserID = @UserId, DeviceName = @DeviceName, deviceID = @DeviceID WHERE id = @Id";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, usuario);
        }

        public async Task<int> DeleteSubUser(string id)
        {
            var sql = @"UPDATE deviceuser SET status = '0' WHERE id = @Id";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<DeviceAdmin>> GetDevices()
        {
            var sql = @"SELECT deviceID, accountID, equipmentType, uniqueID, deviceCode, simPhoneNumber, imeiNumber, habilitada as IsActive from device order by accountID";

            using var connection = CreateConnection();
            return await connection.QueryAsync<DeviceAdmin>(sql);
        }

        public async Task<int> UpdateDevice(DeviceAdmin device, string oldDeviceID, string oldAccountID)
        {
            var sql = @"UPDATE device SET deviceID = @DeviceID, accountID = @AccountID, equipmentType = @EquipmentType, uniqueID = @UniqueID, deviceCode = @DeviceCode, simPhoneNumber = @SimPhoneNumber, imeiNumber = @ImeiNumber WHERE deviceID = @OldDeviceID AND accountID = @OldAccountID";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, new
            {
                device.DeviceID,
                device.AccountID,
                device.EquipmentType,
                device.UniqueID,
                device.DeviceCode,
                device.SimPhoneNumber,
                device.ImeiNumber,
                OldDeviceID = oldDeviceID,
                OldAccountID = oldAccountID
            });
        }

        public async Task<int> InsertDevice(DeviceAdmin device)
        {
            var sql = @"INSERT INTO device (deviceID, accountID, equipmentType, uniqueID, deviceCode, simPhoneNumber, imeiNumber, habilitada) 
                VALUES (@DeviceID, @AccountID, @EquipmentType, @UniqueID, @DeviceCode, @SimPhoneNumber, @ImeiNumber, '1')";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, device);
        }

        public async Task<IEnumerable<ConexDevice>> GetConexDesconex()
        {
            var sql = @"SELECT deviceID, accountID, lastValidSpeed, lastGPSTimestamp, deviceCode, imeiNumber, lastValidLatitude, lastValidLongitude FROM device ORDER BY accountID";

            using var connection = CreateConnection();
            return await connection.QueryAsync<ConexDevice>(sql);
        }
    }
}
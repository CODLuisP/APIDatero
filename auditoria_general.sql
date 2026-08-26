CREATE TABLE IF NOT EXISTS auditoria_general (
  id INT AUTO_INCREMENT PRIMARY KEY,
  usuario VARCHAR(50) NOT NULL,
  modulo VARCHAR(50) NOT NULL,
  accion VARCHAR(20) NOT NULL,
  entidad VARCHAR(100) NULL,
  detalle TEXT NULL,
  fecharegistro DATETIME NOT NULL,
  INDEX idx_auditoria_modulo (modulo),
  INDEX idx_auditoria_usuario (usuario),
  INDEX idx_auditoria_fecha (fecharegistro)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

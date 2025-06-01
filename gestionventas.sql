-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 02-06-2025 a las 00:58:32
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `gestionventas`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `clientes`
--

CREATE TABLE `clientes` (
  `idCliente` int(11) NOT NULL,
  `dniCliente` varchar(11) NOT NULL,
  `nombreCliente` varchar(50) NOT NULL,
  `domicilio` varchar(100) DEFAULT NULL,
  `localidad` varchar(50) DEFAULT NULL,
  `telefonoCliente` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `clientes`
--

INSERT INTO `clientes` (`idCliente`, `dniCliente`, `nombreCliente`, `domicilio`, `localidad`, `telefonoCliente`) VALUES
(1, '20922882', 'Farias Analia', 'publica sin nro La paz', 'Cordoba', '2664123555'),
(2, '20999222', 'Pedro Diaz', 'Belgrano 459 ', 'San Luisa', '3544-1234567'),
(4, '6160756', 'Beatriz Ramseyer', 'Av. San Martin 784', 'Villa Mercedes', '2664 456987');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `facturaitem`
--

CREATE TABLE `facturaitem` (
  `idFacturaItem` int(11) NOT NULL,
  `idFactura` int(11) DEFAULT NULL,
  `idItem` int(11) DEFAULT NULL,
  `nombreProd` varchar(100) DEFAULT NULL,
  `cantidad` int(11) DEFAULT NULL,
  `precio` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `facturaitem`
--

INSERT INTO `facturaitem` (`idFacturaItem`, `idFactura`, `idItem`, `nombreProd`, `cantidad`, `precio`) VALUES
(3, 4, 4, 'Cinta Métrica 5m', 1, 1540.00),
(8, 7, 9, 'Desmalezadora', 1, 432000.00),
(9, 8, 2, 'Caja de Tornillos x200', 1, 2250.00),
(10, 8, 4, 'Cinta Métrica 5m', 1, 1540.00),
(11, 8, 11, 'LLave Inglesa', 1, 7965.00),
(12, 9, 4, 'Cinta Métrica 5m', 1, 1540.00),
(13, 9, 18, 'Pico de Loro', 1, 9600.00),
(14, 9, 5, 'Taladro Eléctrico 650W', 1, 14400.00);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `facturas`
--

CREATE TABLE `facturas` (
  `idFactura` int(11) NOT NULL,
  `diaVenta` date NOT NULL,
  `montoVenta` decimal(10,0) NOT NULL,
  `vendedor` varchar(50) NOT NULL,
  `idCliente` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `facturas`
--

INSERT INTO `facturas` (`idFactura`, `diaVenta`, `montoVenta`, `vendedor`, `idCliente`) VALUES
(3, '2025-05-30', 3790, 'Mbalestrieri', 4),
(4, '2025-05-30', 1540, 'Mbalestrieri', 2),
(7, '2025-05-30', 432000, 'Sol Anabela Balestrieri', 1),
(8, '2025-05-30', 11755, 'Maximiliano Balestrieri', 2),
(9, '2025-06-01', 25540, 'Maximiliano Balestrieri', 2);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `presupuesto`
--

CREATE TABLE `presupuesto` (
  `idPresupuesto` int(11) NOT NULL,
  `nombreCliente` varchar(50) NOT NULL,
  `telefonoCliente` varchar(12) DEFAULT NULL,
  `fecha` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `presupuesto`
--

INSERT INTO `presupuesto` (`idPresupuesto`, `nombreCliente`, `telefonoCliente`, `fecha`) VALUES
(2, 'maximiliano Bales', '12345678', '2025-05-26'),
(3, 'jose perez', '3544-123456', '2025-05-26'),
(7, 'Gael Monfils', '2664123555', '2025-05-27');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `presupuestoitem`
--

CREATE TABLE `presupuestoitem` (
  `IdItem` int(11) NOT NULL,
  `IdPresupuesto` int(11) NOT NULL,
  `Nombre` varchar(255) DEFAULT NULL,
  `Cantidad` int(11) DEFAULT NULL,
  `PrecioUnitario` decimal(10,2) DEFAULT NULL,
  `total` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `presupuestoitem`
--

INSERT INTO `presupuestoitem` (`IdItem`, `IdPresupuesto`, `Nombre`, `Cantidad`, `PrecioUnitario`, `total`) VALUES
(2, 2, 'Martillo de acero con mango de goma', 1, 3250.00, NULL),
(3, 2, 'Taladro percutor con velocidad variable', 1, 14400.00, NULL),
(4, 3, 'Martillo de acero con mango de goma', 1, 3250.00, NULL),
(5, 3, 'Llave ajustable de acero templado', 1, 4185.00, NULL),
(6, 3, 'Cinta retráctil de 5 metros con traba', 1, 1540.00, NULL),
(13, 7, 'Cinta Métrica 5m', 1, 154000.00, NULL),
(14, 7, 'Caja de Tornillos x200', 1, 225000.00, NULL);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `productos`
--

CREATE TABLE `productos` (
  `idProducto` int(11) NOT NULL,
  `codigo` varchar(255) NOT NULL,
  `nombre` varchar(255) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `categoria` varchar(50) DEFAULT NULL,
  `precioCosto` decimal(10,0) NOT NULL,
  `recargoPorcentaje` decimal(10,0) NOT NULL,
  `precioVenta` decimal(10,0) NOT NULL,
  `stockActual` int(11) NOT NULL,
  `stockMinimo` int(11) DEFAULT NULL,
  `nombreProveedor` varchar(50) DEFAULT NULL,
  `imagen` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `productos`
--

INSERT INTO `productos` (`idProducto`, `codigo`, `nombre`, `descripcion`, `categoria`, `precioCosto`, `recargoPorcentaje`, `precioVenta`, `stockActual`, `stockMinimo`, `nombreProveedor`, `imagen`) VALUES
(1, 'FER001', 'Martillo de Uña', 'Martillo de acero con mango de goma', 'Herramientas', 2500, 30, 3250, 3, 5, 'FerreMax', '\\Imagenes\\Productos\\martillo.png'),
(2, 'FER002', 'Caja de Tornillos x200', 'Tornillos de acero zincado de varias medidas', 'Tornillería', 1800, 25, 2250, 20, 10, 'FerreMax', '/imagenes/productos/dbcfe910-164d-46c5-a570-eee8b694574f.png'),
(3, 'FER003', 'Llave Francesa 10”', 'Llave ajustable de acero templado', 'Herramientas', 3100, 35, 4185, 4, 5, 'Stihl Mercedes', '\\Imagenes\\Productos\\francesa.png'),
(4, 'FER004', 'Cinta Métrica 5m', 'Cinta retráctil de 5 metros con traba', 'Medición', 1100, 40, 1540, 23, 8, 'Medidas Pro', '/imagenes/productos/ed534390-f9f4-4975-a4cc-366251d6eb35.jpg'),
(5, 'FER005', 'Taladro Eléctrico 650W', 'Taladro percutor con velocidad variable', 'Eléctrico', 12000, 20, 14400, 6, 2, 'ElectroHerramientas', '/imagenes/productos/05e2aa5a-96d0-4466-9c16-6a8d204ffc0d.png'),
(9, 'FERR007', 'Desmalezadora', 'Cilindrada: 37,7 cm³, ideal para trabajos exigentes. Potencia: 2,3 CV. Par de giro: 2,2 Nm. Peso: 7,7 kg.', 'Stihl', 320000, 35, 432000, 1, 1, 'FerreMax', '/imagenes/productos/14606618-1da4-4979-b6ca-b16eba26df4c.webp'),
(10, 'FERR008', 'Cabezal para desmalezadora automática LUSQTOFF', 'Carretel automático reforzado LUSQTOFF, código DE001.067.', 'LUSQTOFF', 23000, 35, 31050, 10, 1, 'Hernandez hnos', '/imagenes/productos/892fe139-3c58-45bc-9285-0f255c7bd2ef.jpg'),
(11, 'FERR006', 'LLave Inglesa', 'Pinza Stilson 10 ajustable para caño, 250mm', 'Baco', 5900, 35, 7965, 3, 2, 'Herramientas Gómez', '/imagenes/productos/b6410510-5ed1-4e03-a955-3ed8dfc958eb.webp'),
(12, 'FERR010', 'Rastrillo', 'Para hojas y arar la tierra...', 'sin marca', 7000, 30, 9100, 10, 2, 'FerreMax', ''),
(17, 'FERR123', 'Desmalezadora mano', 'fdsafdf', 'Stihl', 250000, 24, 310000, 10, 1, 'FerreMax', '/imagenes/productos/f7a53a30-0f17-435b-9afa-01512e8326c7.png'),
(18, 'FERR124', 'Pico de Loro', 'pico de Loro baco', 'Baco', 8000, 20, 9600, 8, 2, 'Baco', '/imagenes/productos/d93c8ccb-a2be-4484-8190-ecffd2fd19a7.png');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `proveedor`
--

CREATE TABLE `proveedor` (
  `idProv` int(11) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `telefono` varchar(30) DEFAULT NULL,
  `domicilio` varchar(100) DEFAULT NULL,
  `localidad` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `proveedor`
--

INSERT INTO `proveedor` (`idProv`, `nombre`, `telefono`, `domicilio`, `localidad`) VALUES
(1, 'FerreMax', '123456789', 'Calle Falsa 123', 'Ciudad A'),
(2, 'Tornillos del Sur', '987654321', 'Avenida Siempre Viva 456', 'Ciudad B'),
(3, 'Herramientas Gómez', '555555555', 'Calle Real 789', 'Ciudad C'),
(4, 'Medidas Pro', '444444444', 'Boulevard Central 101', 'Ciudad D'),
(5, 'ElectroHerramientas', '333333333', 'Pasaje Industrial 202', 'Ciudad E'),
(6, 'Stihl Mercedes', '222222222', 'Ruta 50 Km 10', 'Ciudad F'),
(7, 'Hernandez hnos', '111111111', 'Calle Los Pinos 303', 'Ciudad G');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `idUsuario` int(11) NOT NULL,
  `usuario` varchar(50) NOT NULL,
  `contraseña` varchar(255) NOT NULL,
  `rol` varchar(50) NOT NULL,
  `nombreyApellido` varchar(50) NOT NULL,
  `fotoPerfil` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `usuarios`
--

INSERT INTO `usuarios` (`idUsuario`, `usuario`, `contraseña`, `rol`, `nombreyApellido`, `fotoPerfil`) VALUES
(1, 'Mbalestrieri', '123', 'Administrador', 'Maximiliano Balestrieri', '/imagenes/usuarios/c584d1f8-81ab-4c19-a1f8-42e1c29d418a.PNG'),
(2, 'Selene', '123', 'Administrador', 'Selene Balestrieri', '/imagenes/usuarios/298c1cb9-9e87-47ac-9495-bd673e850227.jpg'),
(4, 'Solcy', '123', 'Usuario', 'Sol Anabela Balestrieri', '/imagenes/usuarios/bcc60c6b-afd0-4302-97a0-0e4bc0be6f2d.jpeg');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `ventas`
--

CREATE TABLE `ventas` (
  `idFactura` int(11) NOT NULL,
  `diaVenta` date NOT NULL,
  `montoVenta` decimal(10,0) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `ventas`
--

INSERT INTO `ventas` (`idFactura`, `diaVenta`, `montoVenta`) VALUES
(33, '2025-09-08', 1325),
(34, '2025-09-12', 1640),
(35, '2025-09-25', 981),
(36, '2026-03-04', 1520),
(37, '2026-03-19', 2106),
(38, '2026-03-28', 1870),
(39, '2025-05-25', 2250),
(40, '2025-05-28', 35235),
(41, '2025-05-28', 14400),
(42, '2025-05-29', 2250),
(43, '2025-05-29', 2250),
(44, '2025-05-29', 2250),
(45, '2025-05-29', 2250),
(46, '2025-05-29', 4185),
(47, '2025-05-29', 4185),
(48, '2025-05-29', 2250),
(49, '2025-05-29', 2250),
(50, '2025-05-29', 2250),
(51, '2025-05-29', 2250),
(52, '2025-05-29', 10215),
(53, '2025-05-29', 10215),
(54, '2025-05-29', 2250),
(55, '2025-05-29', 2250),
(56, '2025-05-29', 2250),
(57, '2025-05-29', 2250),
(58, '2025-05-29', 2250),
(59, '2025-05-29', 4185),
(60, '2025-05-29', 2250),
(61, '2025-05-29', 2250),
(62, '2025-05-29', 2250),
(63, '2025-05-29', 2250),
(64, '2025-05-29', 9600),
(65, '2025-05-29', 2250),
(66, '2025-05-29', 9600),
(67, '2025-05-29', 2250),
(68, '2025-05-29', 9100),
(69, '2025-05-29', 310000),
(70, '2025-05-29', 2250),
(71, '2025-05-29', 2250);

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `clientes`
--
ALTER TABLE `clientes`
  ADD PRIMARY KEY (`idCliente`);

--
-- Indices de la tabla `facturaitem`
--
ALTER TABLE `facturaitem`
  ADD PRIMARY KEY (`idFacturaItem`);

--
-- Indices de la tabla `facturas`
--
ALTER TABLE `facturas`
  ADD PRIMARY KEY (`idFactura`),
  ADD KEY `fk_facturas_clientes` (`idCliente`);

--
-- Indices de la tabla `presupuesto`
--
ALTER TABLE `presupuesto`
  ADD PRIMARY KEY (`idPresupuesto`);

--
-- Indices de la tabla `presupuestoitem`
--
ALTER TABLE `presupuestoitem`
  ADD PRIMARY KEY (`IdItem`),
  ADD KEY `IdPresupuesto` (`IdPresupuesto`);

--
-- Indices de la tabla `productos`
--
ALTER TABLE `productos`
  ADD PRIMARY KEY (`idProducto`),
  ADD KEY `fk_productos_proveedor` (`nombreProveedor`);

--
-- Indices de la tabla `proveedor`
--
ALTER TABLE `proveedor`
  ADD PRIMARY KEY (`idProv`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`idUsuario`);

--
-- Indices de la tabla `ventas`
--
ALTER TABLE `ventas`
  ADD PRIMARY KEY (`idFactura`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `clientes`
--
ALTER TABLE `clientes`
  MODIFY `idCliente` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `facturaitem`
--
ALTER TABLE `facturaitem`
  MODIFY `idFacturaItem` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=15;

--
-- AUTO_INCREMENT de la tabla `facturas`
--
ALTER TABLE `facturas`
  MODIFY `idFactura` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de la tabla `presupuesto`
--
ALTER TABLE `presupuesto`
  MODIFY `idPresupuesto` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT de la tabla `presupuestoitem`
--
ALTER TABLE `presupuestoitem`
  MODIFY `IdItem` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- AUTO_INCREMENT de la tabla `productos`
--
ALTER TABLE `productos`
  MODIFY `idProducto` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT de la tabla `proveedor`
--
ALTER TABLE `proveedor`
  MODIFY `idProv` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `idUsuario` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `ventas`
--
ALTER TABLE `ventas`
  MODIFY `idFactura` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=72;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `facturas`
--
ALTER TABLE `facturas`
  ADD CONSTRAINT `fk_facturas_clientes` FOREIGN KEY (`idCliente`) REFERENCES `clientes` (`idCliente`);

--
-- Filtros para la tabla `presupuestoitem`
--
ALTER TABLE `presupuestoitem`
  ADD CONSTRAINT `presupuestoitem_ibfk_1` FOREIGN KEY (`IdPresupuesto`) REFERENCES `presupuesto` (`idPresupuesto`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

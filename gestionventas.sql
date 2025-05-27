-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 27-05-2025 a las 03:02:25
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
(2, 'maximiliano Bales', '123456789', '2025-05-26'),
(3, 'jose perez', '3544-123456', '2025-05-26');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `presupuestoitem`
--

CREATE TABLE `presupuestoitem` (
  `IdItem` int(11) NOT NULL,
  `IdPresupuesto` int(11) NOT NULL,
  `Descripcion` varchar(255) DEFAULT NULL,
  `Cantidad` int(11) DEFAULT NULL,
  `PrecioUnitario` decimal(10,2) DEFAULT NULL,
  `total` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `presupuestoitem`
--

INSERT INTO `presupuestoitem` (`IdItem`, `IdPresupuesto`, `Descripcion`, `Cantidad`, `PrecioUnitario`, `total`) VALUES
(2, 2, 'Martillo de acero con mango de goma', 1, 3250.00, NULL),
(3, 2, 'Taladro percutor con velocidad variable', 1, 14400.00, NULL),
(4, 3, 'Martillo de acero con mango de goma', 1, 3250.00, NULL),
(5, 3, 'Llave ajustable de acero templado', 1, 4185.00, NULL),
(6, 3, 'Cinta retráctil de 5 metros con traba', 1, 1540.00, NULL);

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
  `proveedor` varchar(50) DEFAULT NULL,
  `imagen` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `productos`
--

INSERT INTO `productos` (`idProducto`, `codigo`, `nombre`, `descripcion`, `categoria`, `precioCosto`, `recargoPorcentaje`, `precioVenta`, `stockActual`, `stockMinimo`, `proveedor`, `imagen`) VALUES
(1, 'FER001', 'Martillo de Uña', 'Martillo de acero con mango de goma', 'Herramientas', 2500, 30, 3250, -1, 5, 'FerreMax', '\\Imagenes\\Productos\\martillo.png\n'),
(2, 'FER002', 'Caja de Tornillos x200', 'Tornillos de acero zincado de varias medidas', 'Tornillería', 1800, 25, 2250, 34, 10, 'Tornillos del Sur', 'tornillos.jpg'),
(3, 'FER003', 'Llave Francesa 10”', 'Llave ajustable de acero templado', 'Herramientas', 3100, 35, 4185, 7, 5, 'Herramientas Gómez', '\\Imagenes\\Productos\\francesa.png'),
(4, 'FER004', 'Cinta Métrica 5m', 'Cinta retráctil de 5 metros con traba', 'Medición', 1100, 40, 1540, 30, 8, 'Medidas Pro', '/imagenes/productos/9783d1fa-1e9e-4e70-903b-892a31bad85b.jpg'),
(5, 'FER005', 'Taladro Eléctrico 650W', 'Taladro percutor con velocidad variable', 'Eléctrico', 12000, 20, 14400, 10, 2, 'ElectroHerramientas', 'taladro.jpg'),
(9, 'FERR007', 'Desmalezadora', 'Cilindrada: 37,7 cm³, ideal para trabajos exigentes.\r\nPotencia: 2,3 CV que garantizan un desempeño superior.\r\nPar de giro máximo: 2,2 Nm, proporcionando la fuerza necesaria para desmalezar sin esfuerzo.\r\nPeso ligero: Solo 7,7 kg (sin combustible), lo que ', 'Stihl', 320000, 35, 432000, 2, 1, 'Stihl Mercedes', '/imagenes/productos/547b3ff1-fb76-4218-8375-afa13d92473d.webp'),
(10, 'FERR008', 'Cabezal para desmalezadora automática LUSQTOFF ', 'CARRETEL PARA DESMALEZADORA AUTOMATICO REFORZADO - LUSQTOFF, CODIGO: DE001.067, /Características/, - Mecanismo: Automático, - Tipo: Carretel de doble ', 'LUSQTOFF', 23000, 35, 1, 1, 1, 'Hernandez hnos', '/imagenes/productos/892fe139-3c58-45bc-9285-0f255c7bd2ef.jpg'),
(11, 'FERR006', 'LLave Inglesa', 'Pinza Llave Inglesa Stilson 10 Ajustable Para Caño 250mm', 'Baco', 5900, 35, 7965, 8, 2, 'Hernandez hnos', '/imagenes/productos/b6410510-5ed1-4e03-a955-3ed8dfc958eb.webp'),
(12, 'FERR010', 'Rastrillo', 'Para hojas y arar la tierra', 'sin marca', 7000, 30, 9100, 1, 2, 'Hernandez hnos', '/imagenes/productos/no-disponible.png');

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
(1, 'Mbalestrieri', '123', 'Usuario', 'Maximiliano Balestrieri', '/imagenes/usuarios/c584d1f8-81ab-4c19-a1f8-42e1c29d418a.PNG'),
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
(1, '2026-05-24', 1540),
(2, '2025-05-24', 4185),
(3, '2025-05-24', 2250),
(4, '2025-05-24', 6435),
(5, '2025-06-24', 6435),
(22, '2025-05-24', 4185),
(23, '2025-05-25', 12150),
(24, '2026-02-03', 1201),
(25, '2026-02-15', 1851),
(26, '2026-02-27', 940),
(27, '2025-06-01', 2100),
(28, '2025-06-10', 1735),
(29, '2025-06-22', 881),
(30, '2025-07-05', 1990),
(31, '2025-07-14', 1450),
(32, '2025-07-30', 2201),
(33, '2025-09-08', 1325),
(34, '2025-09-12', 1640),
(35, '2025-09-25', 981),
(36, '2026-03-04', 1520),
(37, '2026-03-19', 2106),
(38, '2026-03-28', 1870),
(39, '2025-05-25', 2250);

--
-- Índices para tablas volcadas
--

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
  ADD PRIMARY KEY (`idProducto`);

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
-- AUTO_INCREMENT de la tabla `presupuesto`
--
ALTER TABLE `presupuesto`
  MODIFY `idPresupuesto` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `presupuestoitem`
--
ALTER TABLE `presupuestoitem`
  MODIFY `IdItem` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT de la tabla `productos`
--
ALTER TABLE `productos`
  MODIFY `idProducto` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `idUsuario` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `ventas`
--
ALTER TABLE `ventas`
  MODIFY `idFactura` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=40;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `presupuestoitem`
--
ALTER TABLE `presupuestoitem`
  ADD CONSTRAINT `presupuestoitem_ibfk_1` FOREIGN KEY (`IdPresupuesto`) REFERENCES `presupuesto` (`idPresupuesto`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

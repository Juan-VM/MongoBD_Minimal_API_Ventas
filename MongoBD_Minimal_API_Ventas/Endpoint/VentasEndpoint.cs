using MongoDB.Driver;

namespace MongoBD_Minimal_API_Ventas.Endpoint
{
    public static class VentasEndpoint
    {

        public static void MapVentasEndpoint(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/ventas").WithTags("Ventas");

            group.MapGet("/", GetAll);

            group.MapGet("/todas-ventas", GetResumen);

            group.MapGet("/productos-rentables", GetProductosRentables);

            group.MapGet("/mejores-clientes", GetMejoresClientes);

            group.MapGet("/vendedores", GetDesempenoVendedores);

            group.MapGet("/por-canal", GetVentasPorCanal);

            group.MapGet("/ingresos-por-categoria", GetIngresosPorCategoria);

            group.MapGet("/ventas-por-ciudad", GetVentasPorCiudad);

        }

        private static IMongoCollection<Venta> ObtenerColeccion(IMongoClient client)
        {
            return client.GetDatabase("EmpresaComercial").GetCollection<Venta>("Ventas");
        }

        private static async Task<IResult> GetAll(IMongoClient client)
        {
            var collection = ObtenerColeccion(client);

            var ventas = await collection.Find(FilterDefinition<Venta>.Empty).ToListAsync();

            return TypedResults.Ok(ventas);
        }

        
        private static async Task<IResult> GetResumen(IMongoClient client)
        {
            var collection = ObtenerColeccion(client);
            var ventas = await collection.Find(FilterDefinition<Venta>.Empty).ToListAsync();

            var completadas = ventas
                .Where(v => v.Estado == "Completada")
                .ToList();

            var resumen = new
            {
                TotalVentasRegistradas = ventas.Count,
                VentasCompletadas = completadas.Count,
                VentasPendientes = ventas.Count(
                    v => v.Estado == "Pendiente"),
                VentasCanceladas = ventas.Count(
                    v => v.Estado == "Cancelada"),
                IngresosTotales = completadas.Sum(v => v.Total),
                TicketPromedio = completadas.Any()
                    ? completadas.Average(v => v.Total)
                    : 0m,
                TotalDescuentos = completadas.Sum(
                    v => v.DescuentoTotal),
                TotalImpuestos = completadas.Sum(
                    v => v.Impuesto)
            };

            return TypedResults.Ok(resumen);
        }

        private static async Task<IResult> GetProductosRentables(IMongoClient client, int top = 10)
        {
            var collection = ObtenerColeccion(client);
            var ventas = await collection.Find(FilterDefinition<Venta>.Empty).ToListAsync();

            var productos = ventas
                .Where(v => v.Estado == "Completada")
                .SelectMany(v => v.Productos)
                .GroupBy(p => new
                {
                    p.IdProducto,
                    p.Nombre,
                    p.Categoria
                })
                .Select(g => new
                {
                    g.Key.IdProducto,
                    g.Key.Nombre,
                    g.Key.Categoria,

                    UnidadesVendidas = g.Sum(p => p.Cantidad),
                    IngresoAntesDeDescuentos = g.Sum(
                        p => p.Cantidad * p.PrecioUnitario),
                    DescuentoAplicado = g.Sum(
                        p => p.Cantidad
                             * p.PrecioUnitario
                             * p.Descuento / 100m),
                    IngresosGenerados = g.Sum(
                        p => p.Cantidad
                             * p.PrecioUnitario
                             * (1 - p.Descuento / 100m))
                })
                .OrderByDescending(p => p.IngresosGenerados)
                .Take(Math.Clamp(top, 1, 100))
                .ToList();

            return TypedResults.Ok(productos);
        }

        private static async Task<IResult> GetMejoresClientes(IMongoClient client, int top = 10)
        {
            var collection = ObtenerColeccion(client);
            var ventas = await collection.Find(FilterDefinition<Venta>.Empty).ToListAsync();

            var clientes = ventas
                .Where(v => v.Estado == "Completada")
                .GroupBy(v => new
                {
                    v.Cliente.Identificacion,
                    v.Cliente.Nombre,
                    v.Cliente.Ciudad,
                    v.Cliente.Provincia,
                    v.Cliente.Categoria
                })
                .Select(g => new
                {
                    g.Key.Identificacion,
                    g.Key.Nombre,
                    g.Key.Ciudad,
                    g.Key.Provincia,
                    g.Key.Categoria,

                    CantidadCompras = g.Count(),
                    DineroGastado = g.Sum(v => v.Total),
                    PromedioPorCompra = g.Average(v => v.Total),
                    UltimaCompra = g.Max(v => v.Fecha)
                })
                .OrderByDescending(c => c.DineroGastado)
                .Take(Math.Clamp(top, 1, 100))
                .ToList();

            return TypedResults.Ok(clientes);
        }

        private static async Task<IResult> GetDesempenoVendedores(IMongoClient client, int top = 10)
        {
            var collection = ObtenerColeccion(client);
            var ventas = await collection.Find(FilterDefinition<Venta>.Empty).ToListAsync();

            var vendedores = ventas
                .Where(v => v.Estado == "Completada")
                .GroupBy(v => new
                {
                    v.Vendedor.Identificacion,
                    v.Vendedor.Nombre,
                    v.Vendedor.Sucursal,
                    v.Vendedor.Region
                })
                .Select(g => new
                {
                    g.Key.Identificacion,
                    g.Key.Nombre,
                    g.Key.Sucursal,
                    g.Key.Region,

                    VentasCompletadas = g.Count(),
                    IngresosGenerados = g.Sum(v => v.Total),
                    PromedioPorVenta = g.Average(v => v.Total),
                    UnidadesVendidas = g.Sum(
                        v => v.Productos.Sum(p => p.Cantidad)),
                    DescuentosAplicados = g.Sum(
                        v => v.DescuentoTotal)
                })
                .OrderByDescending(v => v.IngresosGenerados)
                .Take(Math.Clamp(top, 1, 100))
                .ToList();

            return TypedResults.Ok(vendedores);
        }

        private static async Task<IResult> GetVentasPorCanal(IMongoClient client)
        {
            var collection = ObtenerColeccion(client);
            var ventas = await collection.Find(FilterDefinition<Venta>.Empty).ToListAsync();

            var canales = ventas
                .Where(v => v.Estado == "Completada")
                .GroupBy(v => v.CanalVenta)
                .Select(g => new
                {
                    Canal = g.Key,
                    CantidadVentas = g.Count(),
                    IngresosGenerados = g.Sum(v => v.Total),
                    TicketPromedio = g.Average(v => v.Total),
                    DescuentosAplicados = g.Sum(
                        v => v.DescuentoTotal)
                })
                .OrderByDescending(c => c.IngresosGenerados)
                .ToList();

            return TypedResults.Ok(canales);
        }

        private static async Task<IResult> GetIngresosPorCategoria(IMongoClient client, int top = 10)
        {
            var collection = ObtenerColeccion(client);
            var ventas = await collection.Find(FilterDefinition<Venta>.Empty).ToListAsync();

            var categorias = ventas
                .Where(v => v.Estado == "Completada")
                .SelectMany(v => v.Productos)
                .GroupBy(p => p.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    UnidadesVendidas = g.Sum(p => p.Cantidad),
                    IngresosGenerados = g.Sum(
                        p => p.Cantidad
                             * p.PrecioUnitario
                             * (1 - p.Descuento / 100m)),
                    TicketPromedio = g.Average(
                        p => p.Cantidad * p.PrecioUnitario),
                    DescuentoAplicado = g.Sum(
                        p => p.Cantidad
                             * p.PrecioUnitario
                             * p.Descuento / 100m)
                })
                .OrderByDescending(c => c.IngresosGenerados)
                .Take(Math.Clamp(top, 1, 100))
                .ToList();

            return TypedResults.Ok(categorias);
        }

        private static async Task<IResult> GetVentasPorCiudad(IMongoClient client, int top = 10)
        {
            var collection = ObtenerColeccion(client);
            var ventas = await collection.Find(FilterDefinition<Venta>.Empty).ToListAsync();

            var ciudades = ventas
                .Where(v => v.Estado == "Completada")
                .GroupBy(v => new
                {
                    v.Cliente.Ciudad,
                    v.Cliente.Provincia
                })
                .Select(g => new
                {
                    g.Key.Ciudad,
                    g.Key.Provincia,

                    CantidadVentas = g.Count(),
                    IngresosGenerados = g.Sum(v => v.Total),
                    TicketPromedio = g.Average(v => v.Total),
                    UnidadesVendidas = g.Sum(
                        v => v.Productos.Sum(p => p.Cantidad)),
                    ClientesUnicos = g.Select(v => v.Cliente.Identificacion)
                                      .Distinct()
                                      .Count()
                })
                .OrderByDescending(c => c.IngresosGenerados)
                .Take(Math.Clamp(top, 1, 100))
                .ToList();

            return TypedResults.Ok(ciudades);
        }
    }
}
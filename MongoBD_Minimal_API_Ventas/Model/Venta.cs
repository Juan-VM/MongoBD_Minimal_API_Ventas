
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Venta
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("fecha")]
    public DateTime Fecha { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = string.Empty;

    [BsonElement("metodoPago")]
    public string MetodoPago { get; set; } = string.Empty;

    [BsonElement("moneda")]
    public string Moneda { get; set; } = string.Empty;

    [BsonElement("cliente")]
    public Cliente Cliente { get; set; } = new();

    [BsonElement("vendedor")]
    public Vendedor Vendedor { get; set; } = new();

    [BsonElement("productos")]
    public List<ProductoVenta> Productos { get; set; } = new();

    [BsonElement("subtotal")]
    public decimal Subtotal { get; set; }

    [BsonElement("descuentoTotal")]
    public decimal DescuentoTotal { get; set; }

    [BsonElement("impuesto")]
    public decimal Impuesto { get; set; }

    [BsonElement("total")]
    public decimal Total { get; set; }

    [BsonElement("canalVenta")]
    public string CanalVenta { get; set; } = string.Empty;

    [BsonElement("tienePromocion")]
    public bool TienePromocion { get; set; }
}

public class Cliente
{
    [BsonElement("identificacion")]
    public string Identificacion { get; set; } = string.Empty;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("ciudad")]
    public string Ciudad { get; set; } = string.Empty;

    [BsonElement("provincia")]
    public string Provincia { get; set; } = string.Empty;

    [BsonElement("categoria")]
    public string Categoria { get; set; } = string.Empty;
}

public class Vendedor
{
    [BsonElement("identificacion")]
    public string Identificacion { get; set; } = string.Empty;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("sucursal")]
    public string Sucursal { get; set; } = string.Empty;

    [BsonElement("region")]
    public string Region { get; set; } = string.Empty;
}

public class ProductoVenta
{
    [BsonElement("idProducto")]
    public string IdProducto { get; set; } = string.Empty;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("categoria")]
    public string Categoria { get; set; } = string.Empty;

    [BsonElement("cantidad")]
    public int Cantidad { get; set; }

    [BsonElement("precioUnitario")]
    public decimal PrecioUnitario { get; set; }

    [BsonElement("descuento")]
    public decimal Descuento { get; set; }
}

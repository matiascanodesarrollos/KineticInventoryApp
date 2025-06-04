namespace Infrastructure.Domain
{
    public class Product
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; }
        public DateTime ModifiedDateTimeUtc { get; set; }
    }
}

using System.Text.Json.Serialization;

public class Product
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }
}

public class ProductDetail
{
    public string Name { get; set; }
    public string Status { get; set; }
}

public static class ProductExtensions
{
    public static ProductDetail ToDetail(this Product product)
    {
        return new ProductDetail() { Name = product.Name, Status = product.Status };
    }
}
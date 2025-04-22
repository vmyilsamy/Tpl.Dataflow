
using System.Net.Http.Headers;
using System.Text.Json;

internal class ProductClient
{
    private readonly string _baseAddress;
    private HttpClient _client;

    public ProductClient(string baseAddress)
    {
        _client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
        this._baseAddress = baseAddress;
    }

    internal async Task<Product> GetProductDetails(string productId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseAddress}/product/{productId}");

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var product = JsonSerializer.Deserialize<Product>(content);

        return product;
    }

    internal async Task SaveProduct(ProductDetail productDetail)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseAddress}/product");

        var content = JsonSerializer.Serialize(productDetail);

        request.Content = new StringContent(content, new MediaTypeHeaderValue("application/json"));

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    internal async Task SaveProducts(IEnumerable<ProductDetail> products)
    {
        Console.WriteLine($"Batch contains {products.Count()} items.");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseAddress}/product");

        var content = JsonSerializer.Serialize(products);

        request.Content = new StringContent(content, new MediaTypeHeaderValue("application/json"));

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }
}
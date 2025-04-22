internal class NormalFlow
{
    ProductClient client;
    FilePersistence filePersistence;

    internal string Start(string id)
    {
        client = new ProductClient("http://localhost:5237/api");

        filePersistence = new FilePersistence(id);

        var filePath = filePersistence.Initialise();

        return filePath;
    }

    internal async Task Run(string id)
    {
        var productDetail = await GetProductDetails(id);

        await SaveProduct(productDetail);

        await PostProduct(productDetail);
    }

    private async Task<ProductDetail> GetProductDetails(string productId)
    {
        var product = await client.GetProductDetails(productId);

        return product.ToDetail();
    }

    private async Task SaveProduct(ProductDetail product)
    {
        await filePersistence.Save(product);
    }

    private async Task PostProduct(ProductDetail product)
    {
        await client.SaveProduct(product);
    }
}

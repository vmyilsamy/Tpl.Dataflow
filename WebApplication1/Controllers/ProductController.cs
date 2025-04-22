using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("/api/[controller]")]
    public class ProductController : ControllerBase
    {

        [HttpGet("{productId}")]
        public Product Get(string productId)
        {
            var product = new Product() { Name = $"Name{productId}", Description = $"Blah..Blah..{productId}", Status = "Active" };

            return product;
        }

        [HttpPost]
        public void Save([FromBody]Product product)
        {
            Task.CompletedTask.Wait();
        }
    }
}

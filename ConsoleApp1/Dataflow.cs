using System.Threading.Tasks.Dataflow;

internal class Dataflow
{

    BufferBlock<string> bufferBlock;

    TransformBlock<string, ProductDetail> transformBlock;

    BatchBlock<ProductDetail> batchBlock;

    BroadcastBlock<ProductDetail> broadcastBlock;

    ActionBlock<ProductDetail> persistBlock;

    ActionBlock<ProductDetail> apiBlock;
    
    ActionBlock<IEnumerable<ProductDetail>> batchApiBlock;

    ProductClient client;
    FilePersistence filePersistence;


    internal Completion Start(string id)
    {
        int maxParallelism = 5;
        int maxParallelismApi = 2;

        client = new ProductClient("http://localhost:5237/api");

        filePersistence = new FilePersistence(id);

        var filePath = filePersistence.Initialise();

        bufferBlock = new BufferBlock<string>();
        transformBlock = new TransformBlock<string, ProductDetail>(GetProductDetails, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelism, BoundedCapacity = 1 });
        broadcastBlock = new BroadcastBlock<ProductDetail>(p => p);
        persistBlock = new ActionBlock<ProductDetail>(SaveProduct, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelism });
        apiBlock = new ActionBlock<ProductDetail>(PostProduct, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelismApi });
        batchBlock = new BatchBlock<ProductDetail>(300);
        batchApiBlock = new ActionBlock<IEnumerable<ProductDetail>>(PostProducts, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelismApi });


        bufferBlock.LinkTo(transformBlock, new DataflowLinkOptions { PropagateCompletion = true });
        transformBlock.LinkTo(broadcastBlock, new DataflowLinkOptions { PropagateCompletion = true });

        broadcastBlock.LinkTo(persistBlock, new DataflowLinkOptions { PropagateCompletion = true });
        broadcastBlock.LinkTo(apiBlock, new DataflowLinkOptions { PropagateCompletion = true });
        //broadcastBlock.LinkTo(batchBlock, new DataflowLinkOptions { PropagateCompletion = true });
        //batchBlock.LinkTo(batchApiBlock, new DataflowLinkOptions { PropagateCompletion = true });

        return new Completion(bufferBlock, new[]{ transformBlock.Completion, broadcastBlock.Completion, persistBlock.Completion, apiBlock.Completion }, filePath);
        //return new Completion(bufferBlock, new[] { transformBlock.Completion, broadcastBlock.Completion, persistBlock.Completion, batchApiBlock.Completion }, filePath);
    }

    internal void Post(string productId)
    {
        if (bufferBlock.Completion.IsCompleted) 
        {
            throw new Exception("Dataflow completed. Please start new pipeline.");
        }

        bufferBlock.Post(productId);
    }

    internal void Wait()
    {
        var tasks =  new Task[] { transformBlock.Completion, broadcastBlock.Completion, persistBlock.Completion, apiBlock.Completion };

        Task.WhenAll(tasks).Wait();
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

    private async Task PostProducts(IEnumerable<ProductDetail> products)
    {
        await client.SaveProducts(products);
    }

    internal class Completion
    {
        public Completion(BufferBlock<string> startBlock, Task[] tasks, string filePath)
        {
            Tasks = tasks;
            FilePath = filePath;
            StartBlock = startBlock;
        }

        public Task[] Tasks { get; }
        public string FilePath { get; }
        public BufferBlock<string> StartBlock { get; }

        internal void Complete()
        {
            StartBlock.Complete();

            Task.WhenAll(Tasks).Wait();
        }
    }
}

using System.Text.Json;

public class FilePersistence
{
    private readonly string _rootPath;
    private string _filePath; 
    private readonly string _id;
    private readonly SemaphoreSlim _handle;

    public FilePersistence(string id)
    {
        _rootPath = ".";
        _id = id;
        _handle = new SemaphoreSlim(1);
    }

    public string Initialise()
    {
        var savePath = Path.Combine(_rootPath, "Temp");

        _filePath = Path.Combine(savePath, $"{_id}.txt");

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        return _filePath;
    }

    public async Task Save(ProductDetail product)
    {
        try
        {
            await _handle.WaitAsync();

            using (FileStream stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                var content = JsonSerializer.Serialize(product);

                await sw.WriteLineAsync(content);
            }
        }
        finally 
        {
            _handle.Release();
        }
    }
}
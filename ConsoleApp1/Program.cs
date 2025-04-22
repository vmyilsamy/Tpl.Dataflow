
using System.Diagnostics;

bool useDatflow = true;

int limit = 1000;

Stopwatch sw = new Stopwatch();

Console.WriteLine("Started!");

string filePath = string.Empty;

sw.Start();

if (useDatflow)
{
    Dataflow.Completion completion = RunDataflow(limit);
    filePath = completion.FilePath;
}
else
{
    filePath = RunNormalflow(limit).GetAwaiter().GetResult();
}

sw.Stop();

Console.WriteLine($"File persisted in path: {filePath}");

Console.WriteLine($"Completed!. Took: {sw.Elapsed.TotalSeconds}");

Console.ReadLine();

IEnumerable<int> GetItems(int limit)
{
    for (int i = 1; i <= limit; i++)
    {
        yield return i;
    }
}

async Task<string> RunNormalflow(int limit)
{
    NormalFlow nf = new NormalFlow();

    var filePath = nf.Start(Guid.NewGuid().ToString());

    foreach (var item in GetItems(limit))
    {
        await nf.Run(item.ToString());
    }

    return filePath;
}

Dataflow.Completion RunDataflow(int limit)
{
    Dataflow df = new Dataflow();

    var completion = df.Start(Guid.NewGuid().ToString());

    foreach (var item in GetItems(limit))
    {
        df.Post(item.ToString());
    }

    completion.Complete();

    return completion;
}


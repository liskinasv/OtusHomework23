// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;



Console.WriteLine("Укажите путь к каталогу:");
string? path = Console.ReadLine();

if(Directory.Exists(path) == false)
{
    Console.WriteLine("Каталога не существует.");
    return;
}

Console.WriteLine("Введите 'n' для прекращения поиска.\n");


using (CancellationTokenSource tokenSource = new CancellationTokenSource())
{
    CancellationToken token = tokenSource.Token;

    Task task = CountSpacesParallelAsync(path, token);


    char ch = Console.ReadKey().KeyChar;
    if (ch == 'N' || ch == 'n')
    {
        tokenSource.Cancel();

        Console.ReadKey();
    } 

}





async Task CountSpacesParallelAsync(string path, CancellationToken token)
{
    Stopwatch sw = Stopwatch.StartNew();

    int totalSpaces = 0;

    string[] files = Directory.GetFiles(path);

    Task<int>[] tasks = new Task<int>[files.Length];


    for (int i = 0; i < tasks.Length; i++)
    {
        string filePath = files[i];

        tasks[i] = Task.Run(() => CountSpacesInFile(filePath, token), token);
    }

    try
    {
        await Task.WhenAll(tasks.ToArray());

        for (int i = 0; i < tasks.Length; i++)
        {
            totalSpaces += tasks[i].Result;
        }

        Console.WriteLine();
        Console.WriteLine($"Всего файлов: {files.Length}");
        Console.WriteLine($"Общее количество пробелов в файлах: {totalSpaces}");

    }
    catch (OperationCanceledException ex)
    {
        Console.WriteLine();
        Console.WriteLine(ex.Message);
        Console.WriteLine("Операция отменена.");
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine(ex.Message);
    }
    finally
    {
        Console.WriteLine();
        sw.Stop();
        Console.WriteLine($"Время выполнения: {sw.ElapsedMilliseconds} мс");
    }
}



int CountSpacesInFile(string filePath, CancellationToken token)
{
    using (StreamReader reader = new StreamReader(filePath))
    {
        int spaces = 0;
        string? line = "";
        int lineCounter = 0;

        try
        {
            while ((line = reader.ReadLine()) != null)
            {
                token.ThrowIfCancellationRequested();

                spaces += line.Split(' ').Length - 1;
                lineCounter++;
            }

            Console.WriteLine($"{filePath}.  Пробелов в файле : {spaces}");
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException($"В файле {filePath}  на строке {lineCounter} поступил запрос на отмену.");
            
        }
        catch (Exception)
        {
            throw;
        }

        return spaces;
    }
}


async Task CountSpacesInThreeFilesAsync(string file1, string file2, string file3, CancellationToken token)
{
    int totalSpaces = 0;

    Task<int> task1 = Task.Run(() => CountSpacesInFile(file1, token));
    Task<int> task2 = Task.Run(() => CountSpacesInFile(file2, token));
    Task<int> task3 = Task.Run(() => CountSpacesInFile(file3, token));

    try
    {
        await Task.WhenAll(task1, task2, task3);

        totalSpaces += task1.Result;
        totalSpaces += task2.Result;
        totalSpaces += task3.Result;


        Console.WriteLine();
        Console.WriteLine($"Общее количество пробелов в файлах: {totalSpaces}");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine();
        Console.WriteLine("Операция отменена.");

    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine(ex.Message);
    }

}







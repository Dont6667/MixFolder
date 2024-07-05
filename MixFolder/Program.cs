using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Пожалуйста, укажите путь к целевой папке.");
            return;
        }

        string directoryPath = args[0];

        try
        {
            // Проверяем существование указанной папки
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Папка '{directoryPath}' не существует.");
                return;
            }

            // Показываем размер папки до очистки
            long initialSize = GetDirectorySize(directoryPath);
            Console.WriteLine($"Размер папки до очистки: {FormatBytes(initialSize)}");

            // Очищаем папку
            CleanDirectory(directoryPath, DateTime.Now);

            // Показываем количество удаленных файлов и освобожденное место
            CleanupStats cleanupStats = CalculateCleanupStats(directoryPath);
            Console.WriteLine($"Удалено файлов: {cleanupStats.DeletedFilesCount}");
            Console.WriteLine($"Освобождено места: {FormatBytes(cleanupStats.TotalSavedSpace)}");

            // Показываем размер папки после очистки
            long finalSize = GetDirectorySize(directoryPath);
            Console.WriteLine($"Размер папки после очистки: {FormatBytes(finalSize)}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Ошибка доступа: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
    }

    static long GetDirectorySize(string directoryPath)
    {
        long directorySize = 0;

        // Перебираем все файлы в текущей директории
        foreach (string file in Directory.EnumerateFiles(directoryPath))
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file);
                directorySize += fileInfo.Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось получить информацию о файле '{file}': {ex.Message}");
            }
        }

        // Рекурсивно вызываем этот метод для подпапок
        foreach (string subDirectory in Directory.EnumerateDirectories(directoryPath))
        {
            try
            {
                directorySize += GetDirectorySize(subDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось получить размер папки '{subDirectory}': {ex.Message}");
            }
        }

        return directorySize;
    }

    static void CleanDirectory(string directoryPath, DateTime currentTime)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

        // Перебираем все файлы в текущей директории
        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            try
            {
                // Если файл не использовался более 30 минут, удаляем его
                if ((currentTime - file.LastAccessTime) > TimeSpan.FromMinutes(30))
                {
                    file.Delete();
                    Console.WriteLine($"Удален файл: {file.FullName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось удалить файл '{file.FullName}': {ex.Message}");
            }
        }

        // Рекурсивно вызываем этот метод для подпапок
        foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
        {
            CleanDirectory(subDirectory.FullName, currentTime);
        }
    }

    static CleanupStats CalculateCleanupStats(string directoryPath)
    {
        CleanupStats stats = new CleanupStats();

        // Подсчитываем количество удаленных файлов и освобожденное место
        stats = CalculateDirectoryCleanupStats(directoryPath, stats);

        return stats;
    }

    static CleanupStats CalculateDirectoryCleanupStats(string directoryPath, CleanupStats stats)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

        // Перебираем все файлы в текущей директории
        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            try
            {
                // Увеличиваем счетчик удаленных файлов
                stats.DeletedFilesCount++;

                // Увеличиваем счетчик освобожденного места
                stats.TotalSavedSpace += file.Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось получить информацию о файле '{file.FullName}': {ex.Message}");
            }
        }

        // Рекурсивно вызываем этот метод для подпапок
        foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
        {
            stats = CalculateDirectoryCleanupStats(subDirectory.FullName, stats);
        }

        return stats;
    }

    static string FormatBytes(long bytes)
    {
        const int scale = 1024;
        string[] orders = { "TB", "GB", "MB", "KB", "Bytes" };

        double max = Math.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
                return string.Format("{0:##.##} {1}", bytes / max, order);

            max /= scale;
        }

        return "0 Bytes";
    }
}

class CleanupStats
{
    public int DeletedFilesCount { get; set; }
    public long TotalSavedSpace { get; set; }

    public CleanupStats()
    {
        DeletedFilesCount = 0;
        TotalSavedSpace = 0;
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    static readonly string[] IgnoreFolders = { };
    static readonly string[] IgnoreFileExtensions = { ".png", ".jpg", ".gif" };
    static readonly string[] FoldersToDelete = { ".vs", "obj", "bin" };

    static void Main(string[] args)
    {
        Console.WriteLine("Enter the directory path:");
        string directoryPath = Console.ReadLine();

        Console.WriteLine("Enter the string to search for:");
        string searchString = Console.ReadLine();

        Console.WriteLine("Enter the string to replace it with:");
        string replaceString = Console.ReadLine();

        try
        {
            DeleteSpecifiedFolders(directoryPath);
            ProcessDirectory(directoryPath, searchString, replaceString);
            Console.WriteLine("Operation completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void DeleteSpecifiedFolders(string directoryPath)
    {
        foreach (var folder in FoldersToDelete)
        {
            string[] matchingFolders = Directory.GetDirectories(directoryPath, folder, SearchOption.AllDirectories);
            foreach (var folderPath in matchingFolders)
            {
                try
                {
                    Directory.Delete(folderPath, true);
                    Console.WriteLine($"Deleted folder: {folderPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete folder {folderPath}: {ex.Message}");
                }
            }
        }
    }

    static void ProcessDirectory(string directoryPath, string searchString, string replaceString)
    {
        // Rename directories first (bottom-up to avoid issues with changing paths)
        foreach (var dir in Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories).Reverse())
        {
            string newDirName = Path.GetFileName(dir).Replace(searchString, replaceString);
            if (newDirName != Path.GetFileName(dir))
            {
                string newDirPath = Path.Combine(Path.GetDirectoryName(dir), newDirName);
                Directory.Move(dir, newDirPath);
                Console.WriteLine($"Renamed directory: {dir} to {newDirPath}");
            }
        }

        // Now process files
        foreach (string filePath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
        {
            if (ShouldProcessFile(filePath))
            {
                ProcessFile(filePath, searchString, replaceString);
            }
        }
    }

    static bool ShouldProcessFile(string filePath)
    {
        if (IgnoreFolders.Any(folder => filePath.Contains($"{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}")))
        {
            return false;
        }

        string extension = Path.GetExtension(filePath).ToLower();
        if (IgnoreFileExtensions.Contains(extension))
        {
            return false;
        }

        return true;
    }

    static void ProcessFile(string filePath, string searchString, string replaceString)
    {
        // Rename file if necessary
        string directory = Path.GetDirectoryName(filePath);
        string fileName = Path.GetFileName(filePath);
        string newFileName = fileName.Replace(searchString, replaceString);

        if (newFileName != fileName)
        {
            string newFilePath = Path.Combine(directory, newFileName);
            File.Move(filePath, newFilePath);
            Console.WriteLine($"Renamed file: {filePath} to {newFilePath}");
            filePath = newFilePath; // Update filePath for content processing
        }

        // Process file content
        string content = File.ReadAllText(filePath);
        if (content.Contains(searchString))
        {
            content = content.Replace(searchString, replaceString);
            File.WriteAllText(filePath, content);
            Console.WriteLine($"Processed content: {filePath}");
        }
    }
}

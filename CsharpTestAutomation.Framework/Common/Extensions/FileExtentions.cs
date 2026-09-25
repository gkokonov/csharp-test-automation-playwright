using System.Reflection;

namespace CsharpTestAutomation.Framework.Common.Extensions;

/// <summary>
/// Contains Files related helper methods
/// </summary>
public static class FileExtentions
{
    /// <summary>
    /// Gets the full path to a test data file. The file can be located anywhere under the TestData directory
    /// using a path constructed from the provided subfolders.
    /// </summary>
    /// <param name="fileName">The full name of the requested file, i.e. test.xml</param>
    /// <param name="subFolders">The list of subfolders under TestData to construct the path (e.g., ["Resources", "Sample", "Json"])</param>
    /// <returns>The full path to the file as string</returns>
    public static string GetTestDataFilePath(string fileName, params string[] subFolders)
    {
        var assemblyDir = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));

        // Build the test data path
        var pathParts = new List<string> { "TestData" };

        // Add all non-null and non-empty subfolders
        if (subFolders != null)
        {
            pathParts.AddRange(subFolders.Where(folder => !string.IsNullOrEmpty(folder)));
        }

        pathParts.Add(fileName);

        var testDataFilePath = Path.Combine(assemblyDir ?? throw new InvalidOperationException("Assembly dir is empty!"),
                                            Path.Combine(pathParts.ToArray()));

        return testDataFilePath;
    }

    /// <summary>
    /// Gets the content as string of a test data file. The file can be located anywhere under the TestData directory
    /// using a path constructed from the provided subfolders.
    /// </summary>
    /// <param name="fileName">The full name of the requested file, i.e. test.xml</param>
    /// <param name="subFolders">The list of subfolders under TestData to construct the path (e.g., ["Resources", "Sample", "Json"])</param>
    /// <returns>Content of the specified file as string</returns>
    public static string GetTestDataFileContent(string fileName, params string[] subFolders)
    {
        var testDataFilePath = GetTestDataFilePath(fileName, subFolders);
        return File.ReadAllText(testDataFilePath);
    }
}

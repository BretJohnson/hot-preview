using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer;

[McpServerToolType]
public class FileSystemTools
{

    /// <summary>
    /// Reads a file from the specified path and returns its content.
    /// </summary>
    /// <param name="filePath">The full path to the file to be read.</param>
    /// <returns>A string containing the file content.</returns>
    [McpServerTool(Name = "read_file")]
    [Description("Reads a file from the specified path on user system")]
    public string ReadFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path {filePath}";
            }

            string content = File.ReadAllText(filePath);
            return content;
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }

    /// <summary>
    /// Reads a binary file from the specified path and returns its content as a Base64 string.
    /// </summary>
    /// <param name="filePath">The full path to the file to be read.</param>
    /// <returns>A Base64 string representing the file content.</returns>
    [McpServerTool(Name = "read_binary_file")]
    [Description("Reads a binary file from the specified path and returns Base64 encoded content on user system")]
    public string ReadBinaryFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path {filePath}";
            }

            byte[] bytes = File.ReadAllBytes(filePath);
            string base64Content = Convert.ToBase64String(bytes);
            return base64Content;
        }
        catch (Exception ex)
        {
            return $"Error reading binary file: {ex.Message}";
        }
    }

    /// <summary>
    /// Writes text content to a file at the specified path.
    /// </summary>
    /// <param name="filePath">The full path to the file to be written.</param>
    /// <param name="content">The text content to write to the file.</param>
    /// <returns>A status message indicating success or failure.</returns>
    [McpServerTool(Name = "write_file")]
    [Description("Writes text content to a file at the specified path on user system")]
    public string WriteFile(string filePath, string content)
    {
        try
        {
            File.WriteAllText(filePath, content);
            return $"Successfully wrote content to {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error writing to file: {ex.Message}";
        }
    }

    /// <summary>
    /// Writes binary content (provided as Base64 string) to a file at the specified path.
    /// </summary>
    /// <param name="filePath">The full path to the file to be written.</param>
    /// <param name="base64Content">The Base64 encoded content to write to the file.</param>
    /// <returns>A status message indicating success or failure.</returns>
    [McpServerTool(Name = "write_binary_file")]
    [Description("Writes binary content (provided as Base64 string) to a file on user system")]
    public string WriteBinaryFile(string filePath, string base64Content)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(base64Content);
            File.WriteAllBytes(filePath, bytes);
            return $"Successfully wrote binary content to {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error writing binary file: {ex.Message}";
        }
    }

    /// <summary>
    /// Reads specific lines from a text file.
    /// </summary>
    /// <param name="filePath">The full path to the file to be read.</param>
    /// <param name="startLine">The line number to start reading from (1-based).</param>
    /// <param name="endLine">The line number to end reading at (inclusive, 1-based).</param>
    /// <returns>A string containing the specified lines from the file.</returns>
    [McpServerTool(Name = "read_file_lines")]
    [Description("Reads specific lines from a text file on user system.")]
    public string ReadFileLines(string filePath, int startLine, int endLine)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path {filePath}";
            }

            if (startLine < 1)
            {
                return "Error: Start line must be at least 1.";
            }

            if (endLine < startLine)
            {
                return "Error: End line must be greater than or equal to start line.";
            }

            string[] allLines = File.ReadAllLines(filePath);

            if (startLine > allLines.Length)
            {
                return $"Error: Start line {startLine} exceeds file length of {allLines.Length} lines.";
            }

            int actualEndLine = Math.Min(endLine, allLines.Length);
            int linesToRead = actualEndLine - startLine + 1;

            string[] selectedLines = new string[linesToRead];
            Array.Copy(allLines, startLine - 1, selectedLines, 0, linesToRead);

            return string.Join(Environment.NewLine, selectedLines);
        }
        catch (Exception ex)
        {
            return $"Error reading file lines: {ex.Message}";
        }
    }

    /// <summary>
    /// Writes text content to specific lines in a file, replacing existing content at those lines.
    /// </summary>
    /// <param name="filePath">The full path to the file to be modified.</param>
    /// <param name="content">The text content to write to the file.</param>
    /// <param name="startLine">The line number to start writing from (1-based).</param>
    /// <returns>A status message indicating success or failure.</returns>
    [McpServerTool(Name = "write_file_lines")]
    [Description("Writes content to specific lines in a text file, replacing existing lines on user system.")]
    public string WriteFileLines(string filePath, string content, int startLine)
    {
        try
        {
            if (startLine < 1)
            {
                return "Error: Start line must be at least 1.";
            }

            string[] contentLines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> resultLines = new List<string>();

            if (File.Exists(filePath))
            {
                string[] existingLines = File.ReadAllLines(filePath);

                // Add lines before the start line
                for (int i = 0; i < Math.Min(startLine - 1, existingLines.Length); i++)
                {
                    resultLines.Add(existingLines[i]);
                }

                // Add new content
                resultLines.AddRange(contentLines);

                // Add lines after the inserted content if needed
                int lineAfterContent = startLine + contentLines.Length - 1;
                if (lineAfterContent < existingLines.Length)
                {
                    for (int i = lineAfterContent; i < existingLines.Length; i++)
                    {
                        resultLines.Add(existingLines[i]);
                    }
                }
            }
            else
            {
                // If file doesn't exist, pad with empty lines if needed
                for (int i = 1; i < startLine; i++)
                {
                    resultLines.Add(string.Empty);
                }

                resultLines.AddRange(contentLines);
            }

            File.WriteAllLines(filePath, resultLines);
            return $"Successfully wrote content to {filePath} starting at line {startLine}";
        }
        catch (Exception ex)
        {
            return $"Error writing to file lines: {ex.Message}";
        }
    }

    /// <summary>
    /// Inserts text content at specific lines in a file, pushing existing content down.
    /// </summary>
    /// <param name="filePath">The full path to the file to be modified.</param>
    /// <param name="content">The text content to insert into the file.</param>
    /// <param name="atLine">The line number to insert at (1-based).</param>
    /// <returns>A status message indicating success or failure.</returns>
    [McpServerTool(Name = "insert_file_lines")]
    [Description("Inserts content at a specific line in a text file, pushing existing content down on user system.")]
    public string InsertFileLines(string filePath, string content, int atLine)
    {
        try
        {
            if (atLine < 1)
            {
                return "Error: Insert line must be at least 1.";
            }

            string[] contentLines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> resultLines = new List<string>();

            if (File.Exists(filePath))
            {
                string[] existingLines = File.ReadAllLines(filePath);

                // Add lines before the insertion point
                for (int i = 0; i < Math.Min(atLine - 1, existingLines.Length); i++)
                {
                    resultLines.Add(existingLines[i]);
                }

                // Add new content
                resultLines.AddRange(contentLines);

                // Add lines after the insertion point
                for (int i = atLine - 1; i < existingLines.Length; i++)
                {
                    resultLines.Add(existingLines[i]);
                }
            }
            else
            {
                // If file doesn't exist, pad with empty lines if needed
                for (int i = 1; i < atLine; i++)
                {
                    resultLines.Add(string.Empty);
                }

                resultLines.AddRange(contentLines);
            }

            File.WriteAllLines(filePath, resultLines);
            return $"Successfully inserted content to {filePath} at line {atLine}";
        }
        catch (Exception ex)
        {
            return $"Error inserting into file: {ex.Message}";
        }
    }

    /// <summary>
    /// Formats a file size in bytes to a human-readable string.
    /// </summary>
    /// <param name="bytes">The file size in bytes.</param>
    /// <returns>A human-readable file size string.</returns>
    private string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;

        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:n1} {suffixes[counter]}";
    }

    /// <summary>
    /// Lists files and directories at the specified path.
    /// </summary>
    /// <param name="directoryPath">The path to list files and directories from.</param>
    /// <returns>A formatted string containing the directory listing.</returns>
    [McpServerTool(Name = "list_directory")]
    [Description("Lists files and directories at the specified path on user system.")]
    public string ListDirectory(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                return $"Error: Directory not found at path {directoryPath}";
            }

            string[] files = Directory.GetFiles(directoryPath);
            string[] directories = Directory.GetDirectories(directoryPath);

            string output = $"# Directory Listing for {directoryPath}\n\n";

            output += "## Directories\n\n";
            foreach (string dir in directories)
            {
                output += $"- {Path.GetFileName(dir)}/\n";
            }

            output += "\n## Files\n\n";
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                output += $"- {Path.GetFileName(file)} ({FormatFileSize(fileInfo.Length)})\n";
            }

            return output;
        }
        catch (Exception ex)
        {
            return $"Error listing directory: {ex.Message}";
        }
    }

}

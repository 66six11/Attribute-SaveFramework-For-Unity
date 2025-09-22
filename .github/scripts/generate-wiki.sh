#!/bin/bash

# Generate Wiki Documentation from XML Comments
# This script parses C# files for XML documentation and generates markdown wiki pages

set -e

echo "Starting wiki generation from XML documentation..."

# Create wiki directory
mkdir -p wiki

# Create a simple .NET console project
echo "Creating .NET project for wiki generator..."
mkdir -p temp_generator
cd temp_generator

# Create project file
cat > WikiGenerator.csproj << 'EOF2'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
EOF2

# Create a simple .NET console project
echo "Creating .NET project for wiki generator..."
mkdir -p temp_generator
cd temp_generator

# Create project file
cat > WikiGenerator.csproj << 'EOF2'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
EOF2

# Move the generator code to Program.cs
cat > Program.cs << 'EOF'
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class WikiGenerator
{
    private static readonly Regex ClassPattern = new Regex(@"^\s*(?:public|internal|private|protected)?\s*(?:static|abstract|sealed)?\s*(?:class|interface|struct|enum)\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Multiline);
    private static readonly Regex NamespacePattern = new Regex(@"^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)", RegexOptions.Multiline);
    private static readonly Regex XmlDocPattern = new Regex(@"^\s*///\s*(.*?)$", RegexOptions.Multiline);
    private static readonly Regex SummaryPattern = new Regex(@"<summary>\s*(.*?)\s*</summary>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex ParamPattern = new Regex(@"<param\s+name=""([^""]*)""\s*>\s*(.*?)\s*</param>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex ReturnsPattern = new Regex(@"<returns>\s*(.*?)\s*</returns>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex RemarksPattern = new Regex(@"<remarks>\s*(.*?)\s*</remarks>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex ExamplePattern = new Regex(@"<example>\s*(.*?)\s*</example>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

    public static void Main(string[] args)
    {
        var sourceDir = "../../SaveFramework";
        var outputDir = "../../wiki";
        
        if (!Directory.Exists(sourceDir))
        {
            Console.WriteLine($"Source directory '{sourceDir}' not found!");
            return;
        }

        Console.WriteLine($"Scanning {sourceDir} for C# files with XML documentation...");
        
        var csFiles = Directory.GetFiles(sourceDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Generated") && !f.Contains(".meta"))
            .ToList();

        Console.WriteLine($"Found {csFiles.Count} C# files to process");

        var documentation = new Dictionary<string, List<ClassDocumentation>>();

        foreach (var file in csFiles)
        {
            ProcessFile(file, documentation);
        }

        // Create output directory
        Directory.CreateDirectory(outputDir);

        // Generate wiki pages
        GenerateWikiPages(documentation, outputDir);
        
        Console.WriteLine("Wiki generation completed!");
    }

    private static void ProcessFile(string filePath, Dictionary<string, List<ClassDocumentation>> documentation)
    {
        var content = File.ReadAllText(filePath);
        var namespaceName = ExtractNamespace(content);
        var classes = ExtractClasses(content);

        if (!string.IsNullOrEmpty(namespaceName) && classes.Any())
        {
            if (!documentation.ContainsKey(namespaceName))
                documentation[namespaceName] = new List<ClassDocumentation>();

            documentation[namespaceName].AddRange(classes);
            Console.WriteLine($"  Processed: {Path.GetFileName(filePath)} ({classes.Count} classes)");
        }
    }

    private static string ExtractNamespace(string content)
    {
        var match = NamespacePattern.Match(content);
        return match.Success ? match.Groups[1].Value : "Global";
    }

    private static List<ClassDocumentation> ExtractClasses(string content)
    {
        var classes = new List<ClassDocumentation>();
        var lines = content.Split('\n');
        
        for (int i = 0; i < lines.Length; i++)
        {
            var classMatch = ClassPattern.Match(lines[i]);
            if (classMatch.Success)
            {
                var className = classMatch.Groups[1].Value;
                var xmlDoc = ExtractXmlDocForClass(lines, i);
                
                if (!string.IsNullOrEmpty(xmlDoc))
                {
                    classes.Add(new ClassDocumentation
                    {
                        Name = className,
                        XmlDocumentation = xmlDoc,
                        Summary = ExtractSummary(xmlDoc),
                        Parameters = ExtractParameters(xmlDoc),
                        Returns = ExtractReturns(xmlDoc),
                        Remarks = ExtractRemarks(xmlDoc),
                        Example = ExtractExample(xmlDoc)
                    });
                }
            }
        }

        return classes;
    }

    private static string ExtractXmlDocForClass(string[] lines, int classLineIndex)
    {
        var xmlLines = new List<string>();
        
        // Look backwards for XML documentation
        for (int i = classLineIndex - 1; i >= 0; i--)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("///"))
            {
                // Remove the /// prefix and add to collection
                var cleanLine = line.Substring(3).Trim();
                xmlLines.Insert(0, cleanLine);
            }
            else if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("["))
            {
                break; // Stop if we hit non-XML doc content
            }
        }

        return string.Join("\n", xmlLines);
    }

    private static string ExtractSummary(string xmlDoc)
    {
        var match = SummaryPattern.Match(xmlDoc);
        if (match.Success)
        {
            var summary = match.Groups[1].Value.Trim();
            // Clean up any remaining /// markers
            summary = Regex.Replace(summary, @"^\s*///\s*", "", RegexOptions.Multiline);
            return summary.Trim();
        }
        return "";
    }

    private static List<ParameterDoc> ExtractParameters(string xmlDoc)
    {
        var parameters = new List<ParameterDoc>();
        var matches = ParamPattern.Matches(xmlDoc);
        
        foreach (Match match in matches)
        {
            parameters.Add(new ParameterDoc
            {
                Name = match.Groups[1].Value,
                Description = match.Groups[2].Value.Trim()
            });
        }

        return parameters;
    }

    private static string ExtractReturns(string xmlDoc)
    {
        var match = ReturnsPattern.Match(xmlDoc);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    private static string ExtractRemarks(string xmlDoc)
    {
        var match = RemarksPattern.Match(xmlDoc);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    private static string ExtractExample(string xmlDoc)
    {
        var match = ExamplePattern.Match(xmlDoc);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    private static void GenerateWikiPages(Dictionary<string, List<ClassDocumentation>> documentation, string outputDir)
    {
        // Generate main index page
        GenerateIndexPage(documentation, outputDir);

        // Generate individual namespace pages
        foreach (var ns in documentation)
        {
            GenerateNamespacePage(ns.Key, ns.Value, outputDir);
        }
    }

    private static void GenerateIndexPage(Dictionary<string, List<ClassDocumentation>> documentation, string outputDir)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# SaveFramework API Documentation");
        sb.AppendLine();
        sb.AppendLine("This documentation is automatically generated from XML comments in the source code.");
        sb.AppendLine();
        sb.AppendLine("## Namespaces");
        sb.AppendLine();

        foreach (var ns in documentation.OrderBy(x => x.Key))
        {
            var safeNamespace = ns.Key.Replace(".", "-");
            sb.AppendLine($"- [{ns.Key}]({safeNamespace}.md) - {ns.Value.Count} classes");
        }

        sb.AppendLine();
        sb.AppendLine($"*Last updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*");

        File.WriteAllText(Path.Combine(outputDir, "Home.md"), sb.ToString());
    }

    private static void GenerateNamespacePage(string namespaceName, List<ClassDocumentation> classes, string outputDir)
    {
        var sb = new StringBuilder();
        var safeNamespace = namespaceName.Replace(".", "-");
        
        sb.AppendLine($"# {namespaceName}");
        sb.AppendLine();

        foreach (var classDoc in classes.OrderBy(x => x.Name))
        {
            sb.AppendLine($"## {classDoc.Name}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(classDoc.Summary))
            {
                sb.AppendLine($"**Description:** {classDoc.Summary}");
                sb.AppendLine();
            }

            if (classDoc.Parameters.Any())
            {
                sb.AppendLine("**Parameters:**");
                foreach (var param in classDoc.Parameters)
                {
                    sb.AppendLine($"- `{param.Name}`: {param.Description}");
                }
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(classDoc.Returns))
            {
                sb.AppendLine($"**Returns:** {classDoc.Returns}");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(classDoc.Remarks))
            {
                sb.AppendLine($"**Remarks:** {classDoc.Remarks}");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(classDoc.Example))
            {
                sb.AppendLine("**Example:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(classDoc.Example);
                sb.AppendLine("```");
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }

        sb.AppendLine($"*Last updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*");

        File.WriteAllText(Path.Combine(outputDir, $"{safeNamespace}.md"), sb.ToString());
    }
}

public class ClassDocumentation
{
    public string Name { get; set; }
    public string XmlDocumentation { get; set; }
    public string Summary { get; set; }
    public List<ParameterDoc> Parameters { get; set; } = new List<ParameterDoc>();
    public string Returns { get; set; }
    public string Remarks { get; set; }
    public string Example { get; set; }
}

public class ParameterDoc
{
    public string Name { get; set; }
    public string Description { get; set; }
}
EOF

# Build and run
echo "Building and running wiki generator..."
dotnet run

# Go back and clean up
cd ..
rm -rf temp_generator

echo "Wiki generation completed successfully!"
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
    
    // Enhanced method pattern to capture return type and parameters with better generic support
    private static readonly Regex MethodPattern = new Regex(@"^\s*(?:public|internal|private|protected)?\s*(?:static|virtual|override|abstract)?\s*([^=\s]+(?:<[^>]*>)?(?:\[\])?)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(([^)]*)\)", RegexOptions.Multiline);
    
    // Enhanced property pattern to capture type and modifiers with better generic support
    private static readonly Regex PropertyPattern = new Regex(@"^\s*(?:public|internal|private|protected)?\s*(?:static|virtual|override|abstract)?\s*([^=\s]+(?:<[^>]*>)?(?:\[\])?)\s+([A-Za-z_][A-Za-z0-9_]*)\s*{\s*(?:get|set)", RegexOptions.Multiline);
    
    private static readonly Regex NamespacePattern = new Regex(@"^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)", RegexOptions.Multiline);
    private static readonly Regex XmlDocPattern = new Regex(@"^\s*///\s*(.*?)$", RegexOptions.Multiline);
    private static readonly Regex SummaryPattern = new Regex(@"<summary>\s*(.*?)\s*</summary>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex ParamPattern = new Regex(@"<param\s+name=""([^""]*)""\s*>\s*(.*?)\s*</param>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex ReturnsPattern = new Regex(@"<returns>\s*(.*?)\s*</returns>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex RemarksPattern = new Regex(@"<remarks>\s*(.*?)\s*</remarks>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex ExamplePattern = new Regex(@"<example>\s*(.*?)\s*</example>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    
    // Enhanced pattern to match parameter name and type from parameter list with better generic support
    private static readonly Regex ParameterTypePattern = new Regex(@"([^,\s]+(?:<[^>]*>)?(?:\[\])?)\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.IgnoreCase);

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
        var classes = ExtractClassesAndMembers(content);

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

    private static List<ClassDocumentation> ExtractClassesAndMembers(string content)
    {
        var classes = new List<ClassDocumentation>();
        var lines = content.Split('\n');
        
        for (int i = 0; i < lines.Length; i++)
        {
            var classMatch = ClassPattern.Match(lines[i]);
            if (classMatch.Success)
            {
                var className = classMatch.Groups[1].Value;
                var xmlDoc = ExtractXmlDocForItem(lines, i);
                
                if (!string.IsNullOrEmpty(xmlDoc))
                {
                    var classDoc = new ClassDocumentation
                    {
                        Name = className,
                        XmlDocumentation = xmlDoc,
                        Summary = ExtractSummary(xmlDoc),
                        Parameters = ExtractParameters(xmlDoc),
                        Returns = ExtractReturns(xmlDoc),
                        Remarks = ExtractRemarks(xmlDoc),
                        Example = ExtractExample(xmlDoc),
                        Methods = new List<MethodDocumentation>()
                    };

                    // Extract methods for this class
                    var methodsStartIndex = i + 1;
                    var classEndIndex = FindClassEndIndex(lines, i);
                    
                    for (int j = methodsStartIndex; j < classEndIndex && j < lines.Length; j++)
                    {
                        var methodMatch = MethodPattern.Match(lines[j]);
                        var propMatch = PropertyPattern.Match(lines[j]);
                        
                        if (methodMatch.Success)
                        {
                            var returnType = methodMatch.Groups[1].Value.Trim();
                            var methodName = methodMatch.Groups[2].Value;
                            var parametersString = methodMatch.Groups[3].Value;
                            var methodXmlDoc = ExtractXmlDocForItem(lines, j);
                            
                            if (!string.IsNullOrEmpty(methodXmlDoc))
                            {
                                var paramTypes = ExtractParameterTypes(parametersString);
                                
                                classDoc.Methods.Add(new MethodDocumentation
                                {
                                    Name = methodName,
                                    Type = "Method",
                                    ReturnType = returnType,
                                    ParameterTypes = paramTypes,
                                    Summary = ExtractSummary(methodXmlDoc),
                                    Parameters = ExtractParameters(methodXmlDoc),
                                    Returns = ExtractReturns(methodXmlDoc),
                                    Remarks = ExtractRemarks(methodXmlDoc),
                                    Example = ExtractExample(methodXmlDoc)
                                });
                            }
                        }
                        else if (propMatch.Success)
                        {
                            var propType = propMatch.Groups[1].Value.Trim();
                            var propName = propMatch.Groups[2].Value;
                            var propXmlDoc = ExtractXmlDocForItem(lines, j);
                            
                            if (!string.IsNullOrEmpty(propXmlDoc))
                            {
                                classDoc.Methods.Add(new MethodDocumentation
                                {
                                    Name = propName,
                                    Type = "Property",
                                    ReturnType = propType,
                                    ParameterTypes = new List<ParameterTypeInfo>(),
                                    Summary = ExtractSummary(propXmlDoc),
                                    Parameters = ExtractParameters(propXmlDoc),
                                    Returns = ExtractReturns(propXmlDoc),
                                    Remarks = ExtractRemarks(propXmlDoc),
                                    Example = ExtractExample(propXmlDoc)
                                });
                            }
                        }
                    }

                    classes.Add(classDoc);
                }
            }
        }

        return classes;
    }

    private static int FindClassEndIndex(string[] lines, int classStartIndex)
    {
        int braceCount = 0;
        bool foundOpenBrace = false;
        
        for (int i = classStartIndex; i < lines.Length; i++)
        {
            var line = lines[i];
            foreach (char c in line)
            {
                if (c == '{')
                {
                    braceCount++;
                    foundOpenBrace = true;
                }
                else if (c == '}')
                {
                    braceCount--;
                    if (foundOpenBrace && braceCount == 0)
                    {
                        return i;
                    }
                }
            }
        }
        
        return lines.Length;
    }

    private static string ExtractXmlDocForItem(string[] lines, int itemLineIndex)
    {
        var xmlLines = new List<string>();
        
        // Look backwards for XML documentation
        for (int i = itemLineIndex - 1; i >= 0; i--)
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
            // Remove any remaining parameter tags from the summary
            summary = Regex.Replace(summary, @"<param[^>]*>.*?</param>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
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

    private static List<ParameterTypeInfo> ExtractParameterTypes(string parametersString)
    {
        var paramTypes = new List<ParameterTypeInfo>();
        
        if (string.IsNullOrWhiteSpace(parametersString))
            return paramTypes;
            
        // Handle complex parameter parsing for generic types
        var parameters = SplitParameters(parametersString);
        foreach (var param in parameters)
        {
            var trimmedParam = param.Trim();
            if (string.IsNullOrEmpty(trimmedParam))
                continue;
                
            // Find the last space to separate type from name
            var lastSpaceIndex = trimmedParam.LastIndexOf(' ');
            if (lastSpaceIndex > 0)
            {
                var type = trimmedParam.Substring(0, lastSpaceIndex).Trim();
                var name = trimmedParam.Substring(lastSpaceIndex + 1).Trim();
                
                // Clean up any default values
                var equalIndex = name.IndexOf('=');
                if (equalIndex >= 0)
                {
                    name = name.Substring(0, equalIndex).Trim();
                }
                
                paramTypes.Add(new ParameterTypeInfo
                {
                    Type = type,
                    Name = name
                });
            }
        }
        
        return paramTypes;
    }
    
    private static List<string> SplitParameters(string parametersString)
    {
        var parameters = new List<string>();
        var currentParam = new StringBuilder();
        var angleDepth = 0;
        
        for (int i = 0; i < parametersString.Length; i++)
        {
            var c = parametersString[i];
            
            if (c == '<')
                angleDepth++;
            else if (c == '>')
                angleDepth--;
            else if (c == ',' && angleDepth == 0)
            {
                parameters.Add(currentParam.ToString());
                currentParam.Clear();
                continue;
            }
            
            currentParam.Append(c);
        }
        
        if (currentParam.Length > 0)
            parameters.Add(currentParam.ToString());
            
        return parameters;
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
        
        // Add navigation back to home
        sb.AppendLine("[← Back to Home](Home.md)");
        sb.AppendLine();
        
        // Add table of contents
        if (classes.Any())
        {
            sb.AppendLine("## Table of Contents");
            sb.AppendLine();
            foreach (var classDoc in classes.OrderBy(x => x.Name))
            {
                sb.AppendLine($"- [{classDoc.Name}](#{classDoc.Name.ToLower()})");
                
                if (classDoc.Methods.Any())
                {
                    foreach (var method in classDoc.Methods.Take(5).OrderBy(m => m.Name)) // Show first 5 methods
                    {
                        var anchor = $"{classDoc.Name.ToLower()}-{method.Name.ToLower()}-{method.Type.ToLower()}".Replace(" ", "-");
                        sb.AppendLine($"  - [{method.Name} ({method.Type})](#{anchor})");
                    }
                    if (classDoc.Methods.Count > 5)
                    {
                        sb.AppendLine($"  - ... and {classDoc.Methods.Count - 5} more");
                    }
                }
            }
            sb.AppendLine();
        }

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

            // Add methods and properties
            if (classDoc.Methods.Any())
            {
                sb.AppendLine("### Methods and Properties");
                sb.AppendLine();

                foreach (var method in classDoc.Methods.OrderBy(m => m.Type).ThenBy(m => m.Name))
                {
                    var anchor = $"{classDoc.Name.ToLower()}-{method.Name.ToLower()}-{method.Type.ToLower()}".Replace(" ", "-");
                    sb.AppendLine($"#### {method.Name} ({method.Type}) {{#{anchor}}}");
                    
                    // Add method signature with proper formatting and indentation
                    if (!string.IsNullOrEmpty(method.ReturnType) && method.Type == "Method")
                    {
                        var signature = new StringBuilder();
                        signature.AppendLine("```csharp");
                        signature.Append($"{method.ReturnType} {method.Name}(");
                        
                        if (method.ParameterTypes.Any())
                        {
                            if (method.ParameterTypes.Count == 1)
                            {
                                // Single parameter on same line
                                var param = method.ParameterTypes[0];
                                signature.Append($"{param.Type} {param.Name}");
                            }
                            else
                            {
                                // Multiple parameters with proper indentation
                                signature.AppendLine();
                                for (int i = 0; i < method.ParameterTypes.Count; i++)
                                {
                                    var param = method.ParameterTypes[i];
                                    var comma = i < method.ParameterTypes.Count - 1 ? "," : "";
                                    signature.AppendLine($"    {param.Type} {param.Name}{comma}");
                                }
                                signature.Append("");
                            }
                        }
                        
                        signature.AppendLine(")");
                        signature.AppendLine("```");
                        sb.AppendLine(signature.ToString());
                        sb.AppendLine();
                    }
                    else if (!string.IsNullOrEmpty(method.ReturnType) && method.Type == "Property")
                    {
                        sb.AppendLine("```csharp");
                        sb.AppendLine($"{method.ReturnType} {method.Name} {{ get; set; }}");
                        sb.AppendLine("```");
                        sb.AppendLine();
                    }
                    
                    if (!string.IsNullOrEmpty(method.Summary))
                    {
                        sb.AppendLine($"**Description:** {method.Summary}");
                        sb.AppendLine();
                    }

                    if (method.Parameters.Any())
                    {
                        sb.AppendLine("**Parameters:**");
                        foreach (var param in method.Parameters)
                        {
                            // Try to find type information for this parameter
                            var paramTypeInfo = method.ParameterTypes.FirstOrDefault(p => p.Name == param.Name);
                            var typeInfo = paramTypeInfo != null ? $" (`{paramTypeInfo.Type}`)" : "";
                            sb.AppendLine($"- `{param.Name}`{typeInfo}: {param.Description}");
                        }
                        sb.AppendLine();
                    }

                    if (!string.IsNullOrEmpty(method.Returns))
                    {
                        var returnTypeInfo = !string.IsNullOrEmpty(method.ReturnType) ? $" (`{method.ReturnType}`)" : "";
                        sb.AppendLine($"**Returns**{returnTypeInfo}: {method.Returns}");
                        sb.AppendLine();
                    }

                    if (!string.IsNullOrEmpty(method.Remarks))
                    {
                        sb.AppendLine($"**Remarks:** {method.Remarks}");
                        sb.AppendLine();
                    }

                    if (!string.IsNullOrEmpty(method.Example))
                    {
                        sb.AppendLine("**Example:**");
                        sb.AppendLine("```csharp");
                        
                        // Format example code with proper indentation
                        var exampleLines = method.Example.Split('\n');
                        foreach (var line in exampleLines)
                        {
                            // Clean up any existing indentation and add consistent indentation
                            var trimmedLine = line.Trim();
                            if (!string.IsNullOrEmpty(trimmedLine))
                            {
                                sb.AppendLine(trimmedLine);
                            }
                            else
                            {
                                sb.AppendLine();
                            }
                        }
                        
                        sb.AppendLine("```");
                        sb.AppendLine();
                    }
                }
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
    public List<MethodDocumentation> Methods { get; set; } = new List<MethodDocumentation>();
}

public class MethodDocumentation
{
    public string Name { get; set; }
    public string Type { get; set; } // "Method" or "Property"
    public string ReturnType { get; set; }
    public List<ParameterTypeInfo> ParameterTypes { get; set; } = new List<ParameterTypeInfo>();
    public string Summary { get; set; }
    public List<ParameterDoc> Parameters { get; set; } = new List<ParameterDoc>();
    public string Returns { get; set; }
    public string Remarks { get; set; }
    public string Example { get; set; }
}

public class ParameterTypeInfo
{
    public string Type { get; set; }
    public string Name { get; set; }
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
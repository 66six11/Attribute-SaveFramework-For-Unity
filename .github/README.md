# Wiki Generation Workflow

This directory contains scripts and workflows for automatically generating wiki documentation from XML comments in the C# source code.

## Files

- `generate-wiki.yml` - GitHub Actions workflow that triggers on pushes to main branch or manual dispatch
- `scripts/generate-wiki.sh` - Shell script that creates and runs a .NET console application to parse XML documentation

## How it works

1. The workflow scans all `.cs` files in the `SaveFramework` directory
2. Extracts XML documentation comments (/// <summary>, /// <param>, etc.)
3. Groups documentation by namespace and class
4. Generates markdown wiki pages for each namespace
5. Creates a main `Home.md` index page linking to all namespaces

## Generated Wiki Structure

```
wiki/
├── Home.md                                    # Main index page
├── SaveFramework-Components.md                # SaveId component docs
├── SaveFramework-Editor.md                    # Editor tools docs
├── SaveFramework-Runtime-Core.md              # Core framework classes
├── SaveFramework-Runtime-Core-Conversion.md   # Type conversion system
└── SaveFramework-Sample.md                    # Sample code docs
```

## Triggering the Workflow

The workflow runs automatically when:
- Code changes are pushed to the main branch in `SaveFramework/**/*.cs` files
- Manually triggered via GitHub Actions interface

## Manual Execution

To run the wiki generation locally:

```bash
cd /path/to/repository
./.github/scripts/generate-wiki.sh
```

This will create a `wiki/` directory with all generated documentation.
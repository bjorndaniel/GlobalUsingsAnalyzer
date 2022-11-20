# Global usings analyzer

A C# analyzer that will suggest moving usings to a single project file. Defaults to GlobalUsings.cs

To change the name of the file (defaults to GlobalUsings.cs) add the following .editorconfig setting:

**dotnet_diagnostic.GlobalUsingsAnalyzer.global_usings_filename = FILENAME.cs**

## Release Notes

### 1.1

- Do not report issues when C# version is lower than C# 10 (first version to support Global Usings) 

### 1.0

Initial release
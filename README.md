# Global usings analyzer

A C# analyzer that will suggest moving usings to a single project file. Defaults to GlobalUsings.cs

To change the name of the file (defaults to GlobalUsings.cs) add the following .editorconfig setting:

**dotnet_diagnostic.GlobalUsingsAnalyzer0001.filename = FILENAME.cs**

To disable the analyzer for a project/solution add the following .edtiorconfig setting:

**dotnet_diagnostic.GlobalUsingsAnalyzer0002.enabled = false**

To change the severity of the diagnostic per project/solution add the following .edtiorconfig setting:

**dotnet_diagnostic.GlobalUsingsAnalyzer0003.severity = error** (can be info, warning, error or hidden)

**Note: The diagnostics must be placed under the [*.cs] entry to work**

Also available as a NuGet package for per-project installs: [GlobalUsingsAnalyzer](https://www.nuget.org/packages/GlobalUsingsAnalyzer)

## Release Notes

### 1.1

- Changed name of filename setting to dotnet_diagnostic.GlobalUsingsAnalyzer0001.filename
- Do not report issues when C# version is lower than C# 10 (first version to support Global Usings) 
- Added new setting dotnet_diagnostic.GlobalUsingsAnalyzer0002.enabled used to disable the analyzer for a specific project/solution
- Added new setting dotnet_diagnostic.GlobalUsingsAnalyzer0003.diagnostic_severity used to control the severity of the diagnostic. Can be one of the following: error, info, warning, hidden

### 1.0

Initial release

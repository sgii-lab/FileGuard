# FileGuard

**A lightweight file integrity monitoring CLI built with C# and .NET 8.**

FileGuard monitors files and directories for unexpected changes using SHA-256 cryptographic hashes. It creates a baseline of file states and compares subsequent scans against that baseline to identify modified, added or removed files.

Designed as a lightweight command-line utility, FileGuard demonstrates practical application of file system operations, cryptographic hashing, parallel processing, command-line argument parsing and automated testing.

---

## Features

- **SHA-256 integrity verification** — calculates cryptographic hashes to identify file modifications.
- **Baseline management** — creates a reference snapshot of files for subsequent integrity checks.
- **Change detection** — identifies added, modified and removed files.
- **Parallel scanning** — uses `Parallel.ForEachAsync` to process files concurrently.
- **Directory scanning** — recursively examines files within a target directory.
- **Glob exclusions** — supports exclusion patterns to omit selected files and directories.
- **Watch mode** — supports repeated integrity monitoring.
- **JSON output** — provides machine-readable output for automation and integration.
- **Symbolic link handling** — skips symbolic links.
- **Error handling** — handles inaccessible files and reports scanning warnings.
- **Exit codes** — provides status codes suitable for shell scripts and automation.

## Technology stack

| Technology | Purpose |
|---|---|
| C# | Primary programming language |
| .NET 8 | Application framework |
| SHA-256 | File integrity verification |
| `System.IO` | File system operations |
| `Parallel.ForEachAsync` | Concurrent file processing |
| xUnit | Automated testing |
| GitHub Actions | Continuous integration |

FileGuard's core functionality uses the .NET framework without requiring additional external runtime dependencies.

---

## Project architecture

The application separates command-line handling, file scanning, integrity comparison and application execution into distinct components.

```text
FileGuard/
│
├── FileGuard.sln
│
├── README.md
│
├── src/
│   └── FileGuard/
│       ├── Cli/
│       ├── Core/
│       ├── Program.cs
│       └── FileGuard.csproj
│
└── tests/
└── FileGuard.Tests/
├── AppTests.cs
├── ArgParserTests.cs
├── ComparerTests.cs
├── GlobTests.cs
├── ScannerTests.cs
├── TempDir.cs
└── FileGuard.Tests.csproj
```

### Component responsibilities

- **Cli:** Handles command-line arguments, commands and application options.
- **Core:** Contains the underlying file scanning, hashing and comparison functionality.
- **Program.cs:** Application entry point.
- **FileGuard.Tests:** Contains automated tests for application behaviour and core functionality.
- **FileGuard.sln:** Groups the application and test projects into one .NET solution.

---

## Requirements

- Linux, Windows or macOS.
- .NET 8 SDK.
- Terminal or command-line interface.

Verify your .NET installation:

```bash
dotnet --version
dotnet --list-sdks
```

---

## Installation and setup

### 1. Clone the repository

```bash
git clone https://github.com/sgii-lab/FileGuard.git
```

### 2. Enter the project directory

```bash
cd FileGuard
```

All commands in this README assume you are executing them from the repository root, where `FileGuard.sln` is located.

### 3. Restore dependencies

```bash
dotnet restore FileGuard.sln
```

### 4. Build the solution

```bash
dotnet build FileGuard.sln
```

For an optimized Release build:

```bash
dotnet build FileGuard.sln -c Release
```

### 5. Run automated tests

```bash
dotnet test FileGuard.sln
```

Run the tests using the Release configuration:

```bash
dotnet test FileGuard.sln -c Release
```

### 6. Run FileGuard

Display the available commands and options:

```bash
dotnet run --project src/FileGuard -- --help
```

**Note:** The solution file is used to build and test the complete project. The application itself is launched through its application project, `src/FileGuard`.

---

## Usage

FileGuard provides three primary commands:

| Command | Description |
|---|---|
| `init` | Creates an integrity baseline. |
| `check` | Compares the current file state against the baseline. |
| `watch` | Performs repeated integrity monitoring. |

The examples below assume the command syntax is:

```bash
dotnet run --project src/FileGuard -- <command> <directory>
```

### Initialize a baseline

Create a baseline for a directory:

```bash
dotnet run --project src/FileGuard -- init /path/to/directory
```

The baseline establishes the reference file state used for future comparisons.

### Check file integrity

Compare the current directory state against the baseline:

```bash
dotnet run --project src/FileGuard -- check /path/to/directory
```

FileGuard reports detected changes.

### Watch a directory

Start repeated integrity monitoring:

```bash
dotnet run --project src/FileGuard -- watch /path/to/directory
```

Use `--help` to inspect the available command-specific options.

---

## Practical demonstration

The following example demonstrates the basic integrity-monitoring workflow on Linux.

### Step 1: Create a sample directory

```bash
mkdir -p ~/fileguard-demo

echo "Original content" > ~/fileguard-demo/example.txt
```

### Step 2: Initialize the baseline

```bash
dotnet run --project src/FileGuard -- init ~/fileguard-demo
```

### Step 3: Modify the file

```bash
echo "Modified content" >> ~/fileguard-demo/example.txt
```

### Step 4: Check for changes

```bash
dotnet run --project src/FileGuard -- check ~/fileguard-demo
```

FileGuard should identify the modified file.

---

## Command-line options

FileGuard supports additional options for controlling output and scanning behaviour.

| Option | Description |
|---|---|
| `--json` | Produces machine-readable JSON output. |
| Exclusion patterns | Exclude matching files or directories from scanning. |
| `--help` | Displays available commands and options. |

Consult the command help for the exact supported syntax:

```bash
dotnet run --project src/FileGuard -- --help
```

---

## Exit codes

FileGuard uses exit codes to communicate the result of an operation.

| Exit code | Meaning |
|---|---|
| `0` | No detected integrity changes. |
| `1` | Integrity changes detected. |
| `2` | Application or execution error. |

These codes allow FileGuard to be integrated into shell scripts and automated workflows.

---

## JSON output

FileGuard supports JSON output through the `--json` option.

Example invocation:

```bash
dotnet run --project src/FileGuard -- check /path/to/directory --json
```

JSON output makes it possible for other programs to consume integrity-check results without parsing human-readable terminal output.

---

## Testing

FileGuard includes an automated test project covering important application components.

The test suite includes:

- Application behaviour.
- Command-line argument parsing.
- File comparison.
- Glob pattern handling.
- File scanning.

Run the complete test suite from the repository root:

```bash
dotnet test FileGuard.sln
```

For a Release configuration:

```bash
dotnet test FileGuard.sln -c Release
```

---

## Publishing

FileGuard can be published as a Release build.

From the repository root:

```bash
dotnet publish src/FileGuard/FileGuard.csproj -c Release -o ./publish
```

Run the published application:

```bash
dotnet ./publish/fileguard.dll --help
```

The published output directory contains the application and its required runtime files.

---

## Security considerations

FileGuard is designed to help identify unexpected file modifications.

Important considerations:

- SHA-256 provides a cryptographic fingerprint of file contents.
- A baseline must be protected against unauthorized modification.
- FileGuard does not prevent files from being changed.
- A compromised system may also compromise a locally stored baseline.
- Symbolic links are skipped to avoid following linked filesystem paths.
- Files that cannot be accessed may produce warnings and incomplete scan results.

For stronger integrity assurance, store protected baseline copies separately from the monitored directory.

---

## Limitations

FileGuard is an integrity-monitoring utility, not a complete endpoint detection or antivirus solution.

It does not:

- Prevent unauthorized file modifications.
- Automatically identify malware.
- Guarantee that a system is uncompromised.
- Replace operating-system security controls.
- Protect a baseline stored on a compromised system.

Its purpose is to provide a lightweight mechanism for detecting and reporting file integrity changes.

---

## Development

To contribute or modify the project:

1. Fork the repository.
2. Clone your fork.
3. Create a development branch.
4. Implement your changes.
5. Add or update automated tests.
6. Build the solution.
7. Run the test suite.
8. Submit a pull request.

Recommended verification commands:

```bash
dotnet restore FileGuard.sln
dotnet build FileGuard.sln -c Release
dotnet test FileGuard.sln -c Release
```

---

## Roadmap

Potential future improvements include:

- More detailed integrity reports.
- Configurable scanning profiles.
- Improved baseline management.
- Additional output formats.
- Performance benchmarking.
- More comprehensive automated tests.
- Improved monitoring and reporting capabilities.

---

## License

See the repository's `LICENSE` file for licensing information.

---

## Author

**Godfrey Jr Maliki**

GitHub: https://github.com/sgii-lab

**FileGuard — Know when your files change.**

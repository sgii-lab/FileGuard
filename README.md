# FileGuard

![CI](https://github.com/sgii-lab/FileGuard/actions/workflows/ci.yml/badge.svg?branch=main)

**A lightweight, dependency-free file integrity monitoring tool built with C# and .NET 8.**

FileGuard is a command-line utility that creates cryptographic snapshots of directory contents and compares subsequent scans against a previously recorded baseline.

It identifies files that have been **added, modified, or deleted**, using SHA-256 cryptographic hashes to detect content changes.

Designed for developers, system administrators, and security-conscious users, FileGuard provides a straightforward way to monitor configuration directories, verify deployment contents, inspect backup consistency, and identify unexpected file modifications.

The application uses parallel file hashing to improve scanning performance, supports configurable exclusion patterns, and provides machine-readable JSON output for integration with scripts and automated workflows.

---

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Use Cases](#use-cases)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [How It Works](#how-it-works)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Command Reference](#command-reference)
- [Usage Examples](#usage-examples)
- [Exit Codes](#exit-codes)
- [Exclusion Patterns](#exclusion-patterns)
- [JSON Output](#json-output)
- [Continuous Monitoring](#continuous-monitoring)
- [Automation and Scripting](#automation-and-scripting)
- [Baseline Storage](#baseline-storage)
- [Performance and Resource Usage](#performance-and-resource-usage)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Security Considerations](#security-considerations)
- [Limitations](#limitations)
- [Future Improvements](#future-improvements)
- [License](#license)
- [Author](#author)

---

## Overview

File integrity monitoring is the process of establishing a known state of files and detecting unexpected changes to that state over time.

FileGuard implements this process through a baseline-and-comparison model.

Instead of relying on file modification timestamps, FileGuard calculates a SHA-256 hash for each scanned file. Subsequent scans calculate new hashes and compare them with the recorded baseline.

This allows FileGuard to detect content changes even when file timestamps are unreliable or have been preserved.

### Core capabilities

| Capability | Description |
|---|---|
| Baseline creation | Generate a cryptographic snapshot of a directory. |
| Change detection | Identify added, modified, and deleted files. |
| Cryptographic hashing | Use SHA-256 to verify file content. |
| Parallel processing | Hash multiple files concurrently. |
| Exclusions | Ignore files and directories using glob patterns. |
| Continuous monitoring | Repeat scans at configurable intervals. |
| JSON reporting | Produce structured output for external tools. |
| Automation support | Use predictable process exit codes in scripts and CI. |

FileGuard is designed to be small, portable, and straightforward to operate from a terminal.

---

## Key Features

### 1. SHA-256 File Integrity Verification

Every scanned file is processed using the SHA-256 cryptographic hashing algorithm.

The resulting hash represents the file's content and is used to determine whether the content has changed since the baseline was created.

FileGuard streams file contents through the hashing process rather than loading entire files into memory.

This makes the approach suitable for scanning directories containing files of different sizes.

### 2. Parallel File Processing

File hashing is parallelized using .NET's `Parallel.ForEachAsync`.

This allows multiple files to be processed concurrently, taking advantage of available CPU resources.

The implementation is intended to improve scanning performance when working with directories containing many files.

### 3. Three Operating Modes

FileGuard provides three primary commands:

| Command | Purpose |
|---|---|
| `init` | Create a baseline from a directory. |
| `check` | Compare the current directory state against a baseline. |
| `watch` | Repeatedly scan a directory and report changes. |

### 4. File Change Classification

FileGuard identifies three categories of filesystem changes:

| Symbol | Classification | Description |
|---|---|---|
| `[+]` | Added | A file exists in the current scan but not in the baseline. |
| `[~]` | Modified | A file exists in both scans but has a different SHA-256 hash. |
| `[-]` | Deleted | A file exists in the baseline but is missing from the current scan. |

### 5. Configurable Exclusions

Glob patterns allow users to exclude files and directories from scanning.

Examples:

```bash
-e "*.log"
-e ".git/**"
-e "bin/**"
```

Multiple exclusion patterns can be supplied in a single command.

### 6. Automation-Friendly Exit Codes

FileGuard returns predictable exit codes to distinguish successful scans, detected changes, and execution errors.

This makes it suitable for shell scripts, scheduled jobs, and CI pipelines.

### 7. JSON Output

The `--json` option provides machine-readable results.

JSON output makes it possible to integrate FileGuard with other command-line utilities, monitoring systems, and automation scripts.

### 8. Dependency-Free Core

The application is implemented using C# and .NET 8 without requiring additional third-party runtime packages for its core scanning functionality.

### 9. Global .NET Tool Support

FileGuard can be packaged and installed as a global .NET tool, making the `fileguard` command available from the terminal.

### 10. Automated Testing

The repository includes unit and end-to-end tests, with GitHub Actions providing automated CI execution.

---

## Use Cases

FileGuard can be used in several practical scenarios.

### System Configuration Monitoring

Create a baseline of selected configuration directories and periodically check for unexpected content changes.

Example:

```bash
fileguard init /etc -b /var/lib/fileguard/etc-baseline.json
```

### Deployment Verification

Record the expected contents of a deployment directory and compare them against the current deployment.

This can help identify unexpected file additions, deletions, or modifications.

### Backup Verification

Compare a directory against a previously recorded baseline to identify content differences.

This is useful when checking whether files have changed between backup operations.

### Development Environment Monitoring

Monitor important project directories while excluding temporary files, build outputs, and version-control metadata.

Example:

```bash
fileguard init ~/projects/myapp \
-e ".git/**" \
-e "bin/**" \
-e "obj/**"
```

### Security Auditing

Use periodic integrity checks to identify unexpected file-content changes in selected directories.

FileGuard can contribute to a broader security monitoring workflow, although it does not independently determine whether a detected change is malicious.

---

## Technology Stack

| Category | Technology | Purpose |
|---|---|---|
| Language | C# | Application implementation |
| Framework | .NET 8 | Runtime and application framework |
| Cryptography | SHA-256 | File-content integrity verification |
| Parallel Processing | `Parallel.ForEachAsync` | Concurrent file hashing |
| Serialization | System.Text.Json | Baseline and JSON serialization |
| CLI | Custom command-line interface | Argument parsing and command execution |
| Testing | xUnit | Automated testing |
| CI/CD | GitHub Actions | Automated build and test workflows |

---

## Architecture

FileGuard separates command-line handling from the core file-scanning and comparison functionality.

### High-level execution flow

```text
USER
|
v
COMMAND LINE
|
v
CLI HANDLER
|
v
COMMAND ROUTING
|
+--------+--------+
|        |        |
v        v        v
INIT    CHECK    WATCH
|        |        |
+--------+--------+
|
v
FILE SCANNER
|
v
SHA-256 HASHING
|
v
BASELINE DATA
|
v
CHANGE COMPARER
|
v
REPORTER
|
+--------+--------+
|                 |
v                 v
TERMINAL           JSON
|                 |
+--------+--------+
|
v
EXIT CODE
```

### Component responsibilities

| Component | Responsibility |
|---|---|
| CLI | Processes arguments and dispatches commands. |
| Scanner | Recursively enumerates files and calculates hashes. |
| Hasher | Streams file content through SHA-256. |
| Baseline storage | Serializes and retrieves recorded file metadata. |
| Comparer | Determines which files have changed. |
| Glob matcher | Evaluates configured exclusion patterns. |
| Reporter | Formats results for terminal or JSON output. |

---

## How It Works

FileGuard uses a three-stage integrity monitoring process.

### Stage 1: Directory Scanning

The scanner recursively enumerates files under the supplied directory.

For each eligible file, FileGuard calculates:

- Relative file path.
- SHA-256 content hash.
- File size.

Files are processed concurrently using `Parallel.ForEachAsync`.

File contents are streamed through the hashing operation to avoid loading entire files into memory.

Symbolic links are skipped, and unreadable files are handled with warnings.

### Stage 2: Baseline Creation

The scan results are serialized into a human-readable JSON baseline file.

Conceptually, a baseline contains records similar to:

```json
{
  "path": "config/app.conf",
  "hash": "example-sha256-hash",
  "size": 1024
}
```

The baseline represents the directory state at the time of initialization.

### Stage 3: Baseline Comparison

When `check` is executed, FileGuard performs another scan and compares the resulting file records with the saved baseline.

Files are classified according to their relative paths and content hashes.

The comparison produces:

- Added files.
- Modified files.
- Deleted files.

The results are then displayed in the terminal or serialized as JSON.

**Important:** A detected modification means that file content differs from the baseline. It does not automatically mean that the change was unauthorized or malicious.

---

## Installation

### Prerequisites

The following is required to build and run FileGuard from source:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

Verify your installation:

```bash
dotnet --version
git --version
```

### Clone the repository

```bash
git clone https://github.com/sgii-lab/FileGuard.git
```

Navigate into the project:

```bash
cd FileGuard
```

Restore dependencies:

```bash
dotnet restore
```

Build the application:

```bash
dotnet build
```

---

## Quick Start

The following example demonstrates the basic FileGuard workflow.

### 1. Create a baseline

Select a directory to monitor:

```bash
dotnet run --project src/FileGuard -- init ~/projects
```

This creates a baseline using the default filename:

```text
fileguard-baseline.json
```

### 2. Make some changes

After creating the baseline, modify files in the monitored directory.

For example:

- Add a new file.
- Edit an existing configuration file.
- Delete an existing document.

### 3. Check for changes

```bash
dotnet run --project src/FileGuard -- check ~/projects
```

Example output:

```text
3 change(s) detected:

[~] modified src/app.conf
[-] deleted  notes/old.txt
[+] added    scripts/new.sh
```

The output identifies the relative path and classification of each detected change.

### 4. Use exclusion patterns

To ignore version-control metadata and log files:

```bash
dotnet run --project src/FileGuard -- init ~/projects \
-e ".git/**" \
-e "*.log"
```

Use the same exclusions when performing subsequent checks to maintain a consistent scanning scope.

---

## Command Reference

General syntax:

```text
fileguard <command> <path> [options]
```

### Available commands

| Command | Description |
|---|---|
| `init` | Scan a directory and save a new baseline. |
| `check` | Scan a directory and compare it against the baseline. |
| `watch` | Repeatedly scan a directory and report changes. |

### Available options

| Option | Description | Default |
|---|---|---|
| `-b, --baseline` | Specify the baseline JSON file. | `fileguard-baseline.json` |
| `-e, --exclude` | Exclude files matching a glob pattern. Repeatable. | None |
| `-i, --interval` | Set the number of seconds between watch scans. | `30` |
| `--json` | Produce machine-readable JSON output. | Disabled |
| `-h, --help` | Display command help. | N/A |

### Display help

```bash
fileguard --help
```

Command-specific help:

```bash
fileguard init --help
fileguard check --help
fileguard watch --help
```

---

## Usage Examples

### Create a baseline with a custom filename

```bash
fileguard init ~/projects \
-b ~/fileguard-projects.json
```

### Check against a custom baseline

```bash
fileguard check ~/projects \
-b ~/fileguard-projects.json
```

### Monitor the Linux configuration directory

```bash
sudo fileguard init /etc \
-b /var/lib/fileguard/etc-baseline.json
```

Subsequent checks:

```bash
sudo fileguard check /etc \
-b /var/lib/fileguard/etc-baseline.json
```

Use appropriate permissions to access the monitored directory and baseline.

### Exclude multiple directories and file types

```bash
fileguard init ~/projects \
-e ".git/**" \
-e "bin/**" \
-e "obj/**" \
-e "*.log"
```

### Check with JSON output

```bash
fileguard check ~/projects --json
```

### Monitor a directory every 10 seconds

```bash
fileguard watch ~/projects --interval 10
```

---

## Exit Codes

FileGuard uses process exit codes to communicate scan results.

| Exit Code | Meaning | Description |
|---|---|---|
| `0` | Clean | No changes detected. |
| `1` | Changes detected | One or more files were added, modified, or deleted. |
| `2` | Error | Invalid arguments, missing baseline, or an unrecoverable execution error. |

These exit codes make FileGuard suitable for automated workflows.

### Example: Shell automation

```bash
fileguard check /etc \
-b /var/lib/fileguard/etc-baseline.json

case $? in
0)
echo "No changes detected."
;;
1)
echo "File integrity changes detected!"
;;
2)
echo "FileGuard encountered an error."
;;
esac
```

Always distinguish a detected change from an execution failure.

---

## Exclusion Patterns

FileGuard supports glob-based exclusion patterns.

Exclusions allow users to omit files and directories that are not relevant to the monitoring operation.

### Supported pattern examples

| Pattern | Intended match |
|---|---|
| `*.log` | Log files in any folder. |
| `bin/**` | Contents of a top-level `bin` directory. |
| `docs/*.md` | Markdown files directly inside `docs`. |
| `file?.txt` | Single-character wildcard, such as `file1.txt`. |
| `.git/**` | Contents of a top-level `.git` directory. |

### Multiple exclusions

Use repeated `-e` arguments:

```bash
fileguard init ~/projects \
-e "*.log" \
-e ".git/**" \
-e "bin/**"
```

**Note:** Glob matching behaviour depends on the application's matcher implementation. Patterns should be tested against the intended directory structure before being used in security-sensitive monitoring.

For consistent results, use the same exclusion patterns for baseline creation and subsequent checks.

---

## JSON Output

FileGuard supports JSON output through the `--json` option.

Example:

```bash
fileguard check ~/projects --json
```

JSON output is useful when integrating FileGuard with:

- Shell scripts.
- Python automation.
- CI pipelines.
- Monitoring applications.
- Log-processing tools.

Example conceptual result:

```json
{
  "changes": [
  {
    "type": "Modified",
    "path": "src/app.conf"
  },
  {
    "type": "Added",
    "path": "scripts/new.sh"
  }
  ]
}
```

*The example illustrates the intended structure. Refer to the actual implementation for the exact JSON schema.*

---

## Continuous Monitoring

The `watch` command repeatedly scans a directory at a configurable interval.

Example:

```bash
fileguard watch /etc --interval 10
```

This performs recurring scans every 10 seconds.

### Monitoring behaviour

1. The initial scan establishes the comparison state for the watch session.
2. Subsequent scans are compared against the previous scan.
3. Newly detected changes are reported.
4. The process continues until manually stopped.

The watch command uses polling rather than operating-system filesystem event notifications.

Stop monitoring with:

```text
Ctrl + C
```

**Important:** Watch mode is intended for ongoing observation during the current process session. It should not be treated as a persistent monitoring service unless separately configured to run as one.

---

## Global .NET Tool Installation

FileGuard can be packaged and installed as a global .NET tool.

This makes the `fileguard` command available without repeatedly specifying the project path.

### 1. Create the NuGet package

From the repository root:

```bash
dotnet pack src/FileGuard -c Release
```

### 2. Install the tool locally from the generated package

```bash
dotnet tool install --global \
--add-source src/FileGuard/nupkg \
FileGuard
```

### 3. Verify installation

```bash
fileguard --help
```

### PATH configuration

Ensure the global .NET tools directory is included in your PATH:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

To make this permanent for Bash:

```bash
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
source ~/.bashrc
```

---

## Standalone Linux Binary

FileGuard can also be published as a self-contained executable for Linux x64.

This allows the application to run on a compatible target machine without requiring the .NET runtime to be installed separately.

### Publish

```bash
dotnet publish src/FileGuard \
-c Release \
-r linux-x64 \
--self-contained true \
-p:PublishSingleFile=true \
-o dist
```

### Execute

```bash
./dist/fileguard --help
```

The generated binary targets Linux x64. Other architectures and operating systems require their respective runtime identifiers.

---

## Baseline Storage

The baseline is the reference point against which subsequent scans are compared.

By default, FileGuard uses:

```text
fileguard-baseline.json
```

A custom location can be specified:

```bash
fileguard init ~/projects \
-b /var/lib/fileguard/projects.json
```

Subsequent checks must use the same baseline:

```bash
fileguard check ~/projects \
-b /var/lib/fileguard/projects.json
```

### Baseline security

A baseline is only useful if its integrity can be trusted.

If an attacker can modify both the monitored files and the baseline, they may be able to conceal changes.

For security-sensitive monitoring:

- Store baselines outside the monitored directory.
- Restrict write permissions on baseline files.
- Use a separate account or protected storage location where appropriate.
- Keep trusted baseline copies in a location with independent access controls.
- Consider cryptographic baseline signing as a future enhancement.

---

## Performance and Resource Usage

FileGuard uses parallel hashing to improve the speed of directory scans.

Its implementation uses:

- Asynchronous file processing.
- `Parallel.ForEachAsync`.
- SHA-256 hashing.
- Stream-based file reading.
- File metadata collection.

### Performance considerations

Actual scanning performance depends on:

- Number of files.
- Total directory size.
- Individual file sizes.
- Storage device performance.
- CPU availability.
- Filesystem permissions.
- Concurrent system activity.

Parallel hashing can improve throughput, particularly when scanning many files, but performance is not guaranteed to scale linearly with CPU core count.

FileGuard also needs to read the contents of files to calculate their hashes, meaning large directories may require significant disk I/O.

---

## Testing

The repository includes unit tests and end-to-end command-line tests.

Test project:

```text
tests/FileGuard.Tests/
```

### Run all tests

```bash
dotnet test
```

### Build the application

```bash
dotnet build
```

### Run tests in Release configuration

```bash
dotnet test -c Release
```

### Areas covered by the testing approach

Important behaviours to validate include:

- Baseline creation.
- SHA-256 file hashing.
- Added-file detection.
- Modified-file detection.
- Deleted-file detection.
- Exclusion pattern handling.
- Baseline serialization and deserialization.
- Invalid command arguments.
- Exit code behaviour.
- CLI execution.

---

## Project Structure

```text
FileGuard/
│
├── src/
│   └── FileGuard/
│       │
│       ├── Cli/
│       │   ├── Argument parsing
│       │   ├── Command dispatch
│       │   └── Help text
│       │
│       ├── Core/
│       │   ├── File scanner
│       │   ├── SHA-256 hashing
│       │   ├── Baseline storage
│       │   ├── Change comparer
│       │   ├── Glob matching
│       │   └── Reporting
│       │
│       └── Program.cs
│
├── tests/
│   └── FileGuard.Tests/
│       │
│       ├── Unit tests
│       └── End-to-end CLI tests
│
├── .github/
│   └── workflows/
│       └── ci.yml
│
├── .gitignore
└── README.md
```

---

## Security Considerations

FileGuard is a file-content integrity monitoring utility, not a complete intrusion detection system.

Its security value depends on the integrity of the baseline, the reliability of the scanning process, and the permissions under which it operates.

### Implemented protections

| Protection | Purpose |
|---|---|
| SHA-256 hashing | Detect changes to file contents. |
| Symbolic link skipping | Avoid following symbolic links during scanning. |
| Unreadable-file warnings | Report files that cannot be processed. |
| Baseline self-exclusion | Prevent the baseline from being included in its own monitored snapshot. |
| Exit-code reporting | Allow automation to distinguish clean scans, changes, and errors. |

### Operational recommendations

For sensitive directories:

1. Create the baseline from a trusted system state.
2. Store the baseline outside the monitored directory.
3. Restrict baseline modification permissions.
4. Run scans using an account with appropriate read access.
5. Investigate unexpected changes rather than automatically assuming they are malicious.
6. Protect the executable and its configuration against unauthorized modification.

---

## Limitations

The current implementation has several intentional limitations.

| Limitation | Explanation |
|---|---|
| Content-based detection | Detects file-content changes, not permission or ownership changes. |
| No timestamp monitoring | Does not independently track modification or access timestamps. |
| Polling-based watch mode | Re-scans directories at configured intervals. |
| Unsigned baselines | Baseline files are not cryptographically signed. |
| Symbolic links skipped | Symbolic-link targets are not monitored. |
| Read permission required | Files that cannot be read cannot be reliably hashed. |
| No automatic alert delivery | Changes are reported through the CLI rather than email or webhooks. |

FileGuard should be used as one component of a broader system-monitoring and security strategy.

---

## Future Improvements

The following are potential extensions for future versions.

### Integrity and Security

- HMAC-signed baselines.
- Cryptographic baseline verification.
- File permission monitoring.
- File ownership monitoring.
- Optional timestamp tracking.
- More detailed unreadable-file reporting.

### Monitoring

- Event-driven monitoring with `FileSystemWatcher`.
- Configurable monitoring profiles.
- Persistent monitoring service.
- Email notifications.
- Webhook integrations.
- Structured monitoring logs.

### Performance

- Configurable maximum parallelism.
- Improved large-directory scanning.
- More detailed scan statistics.
- Incremental scanning optimizations.

### Reporting and Integration

- More detailed JSON reports.
- CSV report export.
- Summary statistics.
- Integration with external monitoring platforms.
- Historical change reports.

---

## License

This project is distributed under the MIT License.

See the [LICENSE](LICENSE) file for details.

---

## Author

**Godfrey Junior Maliki**

GitHub: [@sgii-lab](https://github.com/sgii-lab)

Repository: [FileGuard](https://github.com/sgii-lab/FileGuard)

---

*FileGuard is a portfolio project demonstrating practical application of C#, .NET 8, cryptographic hashing, asynchronous programming, parallel processing, filesystem operations, command-line application design, and automated software testing.*

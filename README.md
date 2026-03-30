
# TeamTools Linter CommandLine

[![License MIT](https://gist.githubusercontent.com/IVNSTN/905fc514e3cea426d51efae6b98ca4d5/raw/License-MIT-purple.svg)](./LICENSE)
[![Coverage](https://gist.githubusercontent.com/IVNSTN/905fc514e3cea426d51efae6b98ca4d5/raw/code-coverage.svg)](https://github.com/IVNSTN/TeamTools.Linter.CommandLine/actions)

[\[English en-us\]](./README.md) [\[Русский ru-ru\]](./README.ru-ru.md)

A command-line utility for code linting with plugins support.

## Plugins

For example, [IVNSTN/TeamTools.Linter.TSQL](https://github.com/IVNSTN/TeamTools.Linter.TSQL) for static analysis of **T‑SQL** code.
The list of plugins must be specified in the [configuration file](./TeamTools.Linter.CommandLine/DefaultConfig.json).

## Parameters

| Parameter | Shortcut | Description |
|-----------|----------|-------------|
| `--config` | `-c` | Path to the configuration file |
| `--dir` | `-d` | Path to a directory containing files to be linted |
| `--file` | `-f` | Path to a single file to be linted |
| `--filelist` | `-l` | Path to a text file listing one or more full paths to files to be linted |
| `--output` | `-o` | Path to the output file for results, or `console` to print findings to the terminal |
| `--format` | `-m` | Output format: `json`, `sonar`, or `text`. This parameter is only considered if `output` specifies a file path |
| `--severity` | `-s` | Minimum severity level of findings to include in results. Default is `info` (includes all errors, warnings, hints, and info messages). Use `warning` to include only errors and warnings, or `error` for only explicit errors. Severity levels for each rule are defined in the plugin configuration |
| `--basepath` | `-r` | Base path for files. If specified, relative paths (with this base) will be used in logs and output instead of absolute paths |
| `--verbose` | `-v` | Print detailed progress information to the console |
| `--with-version` | `-n` | Print the version number before outputting results. Unlike `--version`, this allows linting to proceed while also showing the current version in the log |
| `--diff` | — | Lint all files differing from the `master` branch. Works only when running on files in a Git repository (Git must be in `PATH`). Alternatively, compute the file list manually, save it to a text file, and pass its path via `--filelist` |
| `--quiet` | — | Do not return a non‑zero exit code if linting findings are detected |
| `--version` | — | Print the utility version without performing other operations |
| `--help` | — | Display the list of parameters |

## Usage Examples

### Linting diff for a directory

```cmd
.\TeamTools.Linter.CommandLine.exe --dir "c:\source\my_project" --diff
```

The diff is calculated via Git against the main branch; only modified files are scanned. The main branch name can be specified in the configuration file.

### Linting a specific file

```cmd
.\TeamTools.Linter.CommandLine.exe --file "c:\source\my_project\Stored procedures\dbo.my_proc.sql"
```

### Linting all files in a directory excluding info messages

```cmd
.\TeamTools.Linter.CommandLine.exe --dir "c:\source\my_project" --severity warning
```

## Integration

The utility can be used directly via the terminal or integrated into various tools.

### SSMS External Tool

Lint the file open in the current SSMS tab using a custom menu item.
To create a new menu item:

1. Go to **Tools → External Tools**
2. Add a new item and configure as follows:

```ini
Command           = <path to exe>\TeamTools.Linter.CommandLine.exe
Arguments         = --file $(ItemPath) --with-version
Initial directory = (leave empty)
Use output window = check this box
```

- In **Arguments**, use the above line.
- In **Initial directory**, enter the path to the linter executable directory (same as **Command**, but without the `.exe` filename).
- Check **Use output window**; leave other options unchecked.

Now you can lint the open file: right‑click the tab header and select your custom **External Tools** menu item.

### Visual Studio External Tool

Configuration for linting a specific file is similar to SSMS. Below is an example for finding stoppers in the entire diff against the main branch in the current repository:

```ini
Command           = <path to exe>\TeamTools.Linter.CommandLine.exe
Arguments         = --diff --severity warning --with-version
Initial directory = $(SolutionDir)
Use output window = check this box
```

### SourceTree Custom Action

1. Open **Tools → Options** in SourceTree.
2. Select the **Custom actions** tab and add a new item.
3. Enter the full path to the linter executable in **Script to run**.
4. Enter the following in **Parameter**:

```cmd
--file "$REPO\$FILE" --severity warning --verbose
```

Now you can lint a selected file directly from the **SourceTree** interface.

### GIT Hook

Automatically lint changed files before pushing or committing. Add a script call to the appropriate Git event. Example script below (accepts one parameter: the full path to the folder containing `TeamTools.Linter.CommandLine.exe`).

To match CI pipeline behavior, use `--severity` to limit findings.

```sh
#!/bin/bash

linter_folder="$1"
echo "linter: $linter_folder"

repo_path="$(git rev-parse --show-toplevel)"
echo "repository: $repo_path"

"$linter_folder/TeamTools.Linter.CommandLine.exe" \
    --config "$linter_folder/DefaultConfig.json" \
    --dir "$repo_path" \
    --basepath "$repo_path" \
    --output console \
    --severity warning \
    --diff \
    --withversion

last_exit_code=$?

if [ $last_exit_code -ne 0 ]; then
    echo "======="
    echo "Linting failed. See errors and warnings above."
    echo "All stoppers must be fixed before pushing the branch to the server."
    echo "======="

    exit $last_exit_code
fi
```

### CI Pipeline

Integrate into your build pipeline by constructing a console call with required parameters and adding it as a pipeline step.

To prevent findings from failing the build (e.g., if using SonarQube Quality Gate), add `--quiet` to ensure the exit code is always `0`.

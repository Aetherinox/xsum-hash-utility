<p align="center"><img src="Docs/images/banner.png" width="860"></p>
<h1 align="center"><b>xSum Hash Utility</b></h1>

<div align="center">

![Version](https://img.shields.io/github/v/tag/Aetherinox/xsum-shahash-utility?logo=GitHub&label=version&color=ba5225) ![Downloads](https://img.shields.io/github/downloads/Aetherinox/xsum-shahash-utility/total) ![Repo Size](https://img.shields.io/github/repo-size/Aetherinox/xsum-shahash-utility?label=size&color=59702a) ![Last Commit)](https://img.shields.io/github/last-commit/Aetherinox/xsum-shahash-utility?color=b43bcc)

</div>

<br />

---

<br />


- [About](#about)
  - [Supported Algorithms](#supported-algorithms)
  - [What Is A Hash Digest](#what-is-a-hash-digest)
- [Arguments](#arguments)
  - [Main Arguments](#main-arguments)
  - [Sub Arguments](#sub-arguments)
- [Syntax](#syntax)
    - [`--generate`](#--generate)
    - [`--target`](#--target)
    - [`--digest`](#--digest)
    - [`--algorithm`](#--algorithm)
    - [`--output`](#--output)
    - [`--overwrite`](#--overwrite)
    - [`--progress`](#--progress)
    - [`--lowercase`](#--lowercase)
    - [`--clipboard`](#--clipboard)
    - [`--exclude`](#--exclude)
- [Features](#features)
  - [Help Menu](#help-menu)
  - [Target Types](#target-types)
  - [Benchmark](#benchmark)
    - [Standard Benchmark](#standard-benchmark)
    - [Algorithm Stress Test Benchmark](#algorithm-stress-test-benchmark)

<br />

---

<br />

> [!NOTE]
> This utility is currently in development. Not all features are complete. If you see some aspect of the code that could be improved, you're more than welcome to commit. All code will be reviewed before it is accepted.

<br />

---

<br />

# About

This utility allows you to generate, sign, and validate checksums.

While there are numerous apps available for doing things such as this; I need a tool that was very specific, and allowed for this to all be done very easily without complicated excessive options.

<br />

## Supported Algorithms
Currently, the following algorithms are suppported. More are planned for later.

- MD5
- SHA-1
- SHA-256
- SHA-384
- SHA-512

<br />

## What Is A Hash Digest
When using `--generate`, a hash digest will be created which contains a list of all the files you have targeted.  When that digest is generated, it will be created as a file named depending on what hash algorithm you specified.

You may either specify your own name for the hash digest, or let the utility pick a name.

If you generated a hash digest using `SHA512` and did not specify  `--digest <FILE>`, your hash digest will be generated as `SHA512.txt`.

When you open the hash digest in a text editor, you will see something similar to the following:

```
d63ba16a664619c2dc4eb2aeef2a2e64cbc7931b831e0adf1c2275ee08e8fd47  example_file_1.txt
dfb8dacbd53eac730814ef2e9f74a47efabe5cb2a5e458bcad6380ae4c1f1f59  example_file_2.txt
9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08  sample_zip_1.zip
60303ae22b998861bce3b28f33eec1be758a213c86c93c076dbe9f558c11c752  README.md
```

<br />

---

<br />

# Arguments
The following is a description of all the arguments associated with this utility.

<br />

## Main Arguments
The following arguments are considered the **primary** arguments of this utility.

<br />

| Argument | Description |
| --- | --- |
| `-g, --generate` | Generate a new hash digest from the `--target <FILE>` |
| `-v, --verify` | Verifies an existing hash digest and compares its contents with `--target <FILE>` |
| `-s, --sign` | Signs an existing hash digest using GPG |

<br />

## Sub Arguments
Along with the main features listed above, the following sub-arguments can be used to configure out xsum works for you:

<br />

| Argument | Description |
| --- | --- |
| ![Target Badge](https://img.shields.io/badge/Target-%E2%A0%80%E2%A0%80--t%2C%20----target%E2%A0%80%E2%A0%80-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=42) | Target folder or file to generate / verify hash for |
| ![Digest Badge](https://img.shields.io/badge/Digest-%E2%A0%80%E2%A0%80--d%2C%20----digest%E2%A0%80%E2%A0%80-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=44) | Hash digest which contains list of generated hashes |
| ![Algo Badge](https://img.shields.io/badge/Algorithm---a%2C%20----algorithm-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=18) | Algorithm used to verify `--digest` |
| ![Output Badge](https://img.shields.io/badge/Output-%E2%A0%80%E2%A0%80--o%2C%20----output%E2%A0%80%E2%A0%80-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=42) | Output file for results verified |
| ![Overwrite Badge](https://img.shields.io/badge/Overwrite---w%2C%20----overwrite-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=17) | Overwrite results to `--output` instead of append |
| ![Progress Badge](https://img.shields.io/badge/Progress-%E2%A0%80--p%2C%20----progress%E2%A0%80-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=25) | Displays each file being checked and additional info |
| ![Lowercase Badge](https://img.shields.io/badge/Lowercase---l%2C%20----lowercase%E2%A0%80-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=15) | Match and output hash value(s) in lower case instead of upper case |
| ![Clipboard Badge](https://img.shields.io/badge/Clipboard-%E2%A0%80--c%2C%20----clipboard-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=20) | Copies the output hash value to clipboard. |
| ![Exclude Badge](https://img.shields.io/badge/Exclude-%E2%A0%80%E2%A0%80--e%2C%20----exclude%E2%A0%80-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=34) | Filter out files to not be hashed with `-generate` and `-verify` |
| ![Clearsign Badge](https://img.shields.io/badge/Clearsign---r%2C%20----clearsign%E2%A0%80-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=21) | Sign the hash digest using GPG with a clear signature |
| ![DetachSign Badge](https://img.shields.io/badge/DetachSign---n%2C%20----detachsign-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=10) | Sign the hash digest using GPG with a detached signature |
| ![Benchmark Badge](https://img.shields.io/badge/Benchmark---b%2C%20----benchmark-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=15) | Performs benchmarks on a specified algorithm or all. |
| ![Iterations Badge](https://img.shields.io/badge/Iterations---i%2C%20----iterations-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=14) | Number of iterations to perform using `--benchmark` |
| ![Buffer Badge](https://img.shields.io/badge/Buffer-%E2%A0%80%E2%A0%80--f%2C%20----buffer%E2%A0%80%E2%A0%80-%23fc0352?style=for-the-badge&labelColor=%23990836&logo=data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7&logoWidth=44) | Buffer size to use when using `--benchmark` |

<br />

---

<br />

# Syntax
This utility attempts to be semi "smart" in the aspect that you can execute commands in various different ways. The reason for this is because out of the other hashing tools out there, we try to use a similar syntax for this utilities' commands so that there's very little in means of transitioning, and to make it more comfortable.

<br />

### `--generate`
Compute hash for folder, files, or strings and generate new hash digest

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

The `--generate` argument allows you to create a new hash digest from the target folder. This argument requires you to specify a target file or folder using one of the following ways:

<br />

| Expects | Description |
| --- | --- |
| `--generate Path\To\Folder` | Folder to generate hash digest for |
| `--generate --target Path\To\Folder` | Same as above
| `--generate Path\To\File.xxx` | File to generate hash digest for |
| `--generate --target Path\To\File.xxx` | Same as above

<br />

The following commands all do the same action:

```C#
xsum --generate --target "X:\Path\To\ExampleFile.zip" --algo sha256 --digest SHA256.sig
xsum --generate "X:\Path\To\ExampleFile.zip" --algo sha256 --digest SHA256.sig
```

```C#
xsum --verify --target "X:\Path\To\ExampleFile.zip" --algo sha256 --digest SHA256.sig
xsum --verify "X:\Path\To\ExampleFile.zip" --algo sha256 --digest SHA256.sig
```

</details>

<br />

---

<br />

### `--target`
Target folder or file to generate / verify hash for

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

The **Target** is the file or folder you wish to either generate a hash for, or verify an existing hash.

This can be defined either by specifying the file or folder directly after using `--target`, or directly after the main command.

<br />

| Expects | Description |
| --- | --- |
| `--target Path\To\FILE.xxx` | File to create / verify a digest for |
| `--target Path\To\FOLDER` | Folder to create / verify a digest for. All files in folder will be added to digest. |
| `--target STRING` | Creates an SHA hash for a string and prints to console. |

<br />

The following commands all do the same action:

```C#
xsum --verify --target "X:\Path\To\ExampleFile.zip" --algo sha256 --digest SHA256.sig
xsum --generate --target "X:\Path\To\ExampleFile.zip" --algo sha256 --digest SHA256.sig
```

```C#
xsum --verify "X:\Path\To\ExampleFile.zip" --algo sha256 --digest SHA256.sig
xsum --generate "X:\Path\To\ExampleFile.zip" --algo sha256 --digest SHA256.sig
```

</details>

<br />

---

<br />

### `--digest`
Hash digest which contains list of generated hashes

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

The `--digest <FILE>` argument tells xsum either where your current digest is if you're using `-verify`, or where you want a new digest to be created if you are using `--generate`.

<br />

| Expects | Description |
| --- | --- |
| `--digest Path\To\FILE.xxx` | The file to use as the hash digest for verifying |

<br />

If you use `--generate` and do not define a hash digest, one will be generated and placed in the folder where the hashes were made, using the structure `[AlgorithmName].txt`

If you execute:
```C#
xsum --generate "X:\Path\To\ExampleFile.zip" --algo SHA512
```

The hash digest will be saved as `SHA512.txt`.

<br />

</details>

<br />

---

<br />

### `--algorithm`
Algorithm used to verify `--digest`

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

The `--algorithm <HASH>` argument specifies which algorithm to use for generation or verification.

<br />

Available Algorithms:

| Algorithm | Command |
| --- | --- |
| `--algorithm md5` | MD5 |
| `--algorithm sha1` | SHA-1 |
| `--algorithm sha256` | SHA-256 |
| `--algorithm sha384` | SHA-384 |
| `--algorithm sha512` | SHA-512 |

<br />

</details>

<br />

---

<br />

### `--output`
Output file which displays detailed information about each task.

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

The `--output` argument is an optional parameter which allows you to define a file where the results of your generated or verified results will be placed.

The information provided by `--output` is different than the output to the hash digest using `--digest`.

The hash digest contains a list of each target file and its corresponding hash. However, the output file will display step-by-step of what is going on when the hashes are generated.

<br />

| Expects | Description |
| --- | --- |
| `--output Path\To\Output.txt` | The file to output results to |

<br />

</details>

<br />

---

<br />

### `--overwrite`
Overwrite results to `--output` instead of append

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

When used in combination with `--output`, this argument will force the utility to overwrite any existing output files. If you do not specify `--overwrite`, then your current task's results will be appended to any existing result files that may have been generated from previous tasks.

<br />

</details>

<br />

---

<br />

### `--progress`
Displays in-depth information about the utility's progress during a task, as well as the checksum for each file being processed.

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

The `--progress` argument allows you to see a more detailed report about what xSum is doing.

Without this argument, you will see a simple message stating whether or not your hash digest was successfully verified, or that your new digest has been created.

However, when using this argument, you will see a larger collection of messages.

<br />

<p align="center"><sub>--progress enabled</sub></p>

<p align="center"><img style="width: 85%;text-align: center;border: 1px solid #353535;" src="Docs/images/3.png"></p>

<br />

<p align="center"><sub>--progress disabled</sub></p>

<p align="center"><img style="width: 85%;text-align: center;border: 1px solid #353535;" src="Docs/images/4.png"></p>

<br />

</details>

<br />

---

<br />

### `--lowercase`
Match and output hash value(s) in lower case instead of upper case.

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

During normal operation when this utility is processing files, the hash that this utility generates is in upper-case characters. If your hash matches the one in the digest but has different casing, then the verification will fail. The casing of your digest hash, and the hash that the utility generates must be the exact same.

Without the argument `--lowercase`, the following verification will **fail**:

```
d63ba16a664619c2dc4eb2aeef2a2e64cbc7931b831e0adf1c2275ee08e8fd47  filename.zip
```

```
D63BA16A664619C2DC4EB2AEEF2A2E64CBC7931B831E0ADF1C2275EE08E8FD47
```

<br />

This argument will transform all hashes to lowercase, both the hash produced by the utility, and the hashes in your digest, which will allow them to match as long as the hash characters are the same.

<br />

</details>

<br />

---

<br />

### `--clipboard`
Copies the output hash value to clipboard.

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

When the argument `--clipboard` is specified, the results of a task will be copied to your clipboard.

<br />

> [!NOTE]
> When targeting a folder to generate a hash, the hash of the folder itself will be copied to your clipboard, not the individual files.

<br />

</details>

<br />

---

<br />

### `--exclude`
The `--exclude` argument gives you the ability to filter out files that should not be included when your dash digest is created.

<br />

<details>
<summary><sub>Read More</sub></summary>

<br />

It allows for wildcard patterns so that you can match multiple files with one rule.

<br />

```
d63ba16a664619c2dc4eb2aeef2a2e64cbc7931b831e0adf1c2275ee08e8fd47  example_file_1.txt
dfb8dacbd53eac730814ef2e9f74a47efabe5cb2a5e458bcad6380ae4c1f1f59  example_file_2.txt
9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08  sample_zip_1.zip
60303ae22b998861bce3b28f33eec1be758a213c86c93c076dbe9f558c11c752  README.md
```

<br />

As an example, you could exclude the files `example_file_1.txt` and `example_file_2.txt` from being hashed by utilizing the `*` wildcard:

<br />

```shell
--exclude *.txt
```

<br />

For multiple exclusions, append additional `--exclude` to the end of your command:

```shell
--exclude *.txt --exclude *D*
```

<br />

With the above rules in place, any file ending with `.txt` will be excluded, and the rule `*D*` will exclude the file `README.md` since it has a `D` in the name.

```
9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08  sample_zip_1.zip
```

</details>

<br />

---

<br />

# Features
The following features are highlighted to explain them in better detail:

<br />

## Help Menu
We've attempted to give the user as much info as possible to make using the program easier. You can access the main help / commands menu by typing:

```shell
xsum
xsum /?
xsum ?
```

<br />

If you need help on a specific command, type the argument like you normally would, but append a question mark `?` at the end, such as:
```shell
xsum --benchmark ?
```

<br />

<p align="center"><sub>Help menu for the command `benchmark`)</sub></p>

<p align="center"><img style="width: 85%;text-align: center;border: 1px solid #353535;" src="Docs/images/7.png"></p>

<br />

---

<br />

## Target Types
This utility handles various different types of input:
- Files
- Folder
- Strings

If your specified input is not detected as a valid file or folder, the utility will switch over to **String Mode**, which means that it will take the string you have provided, and return a hash for that string.

<br />

<p align="center"><sub>Standard string hashing (sha256)</sub></p>

<p align="center"><img style="width: 85%;text-align: center;border: 1px solid #353535;" src="Docs/images/5.png"></p>

<br />

<p align="center"><sub>Standard string hashing (sha1)</sub></p>

<p align="center"><img style="width: 85%;text-align: center;border: 1px solid #353535;" src="Docs/images/6.png"></p>

<br />

---

<br />

## Benchmark
xSum includes a **Benchmark** feature, which allows you to test the performance of the various different hashes.

<br />

The benchmark functionality includes a few different modes:

| Mode | Description | Example Command |
| --- | --- | --- |
| **Standard Benchmark** | Run each algorithm through a series of performance tests | `xsum.exe --benchmark --buffer 32000000 --iterations 50` |
| **Algo Stress Test** | Stress the processing speeds of one particular algorithm. | `xsum --benchmark --algo md5 --iterations 100000` |

<br />

---

<br />

### Standard Benchmark
This will test each algorithm by throwing a series of hashing tasks at it. The test will perform `2 Rounds` for each algorithm in order to ensure everything is properly warmed up.

<br />
<br />

Each algorithm will be assessed using 4 methods:

| Name | Method | Notes |
| --- | --- | --- |
| Unmanaged | `SHA*Cng` | |
| Managed | `SHA*Managed` | Not available for `MD5` |
| Crypto Service Provider (CSP) | `SHA*CryptoServiceProvider` | |
| Create | `SHA*.Create` | |

<br />
<br />

This benchmark accepts two arguments:
| Argument | Default | Min Value | Max Value | Description |
| --- | --- | --- | --- | --- |
| `--buffer` | `32000000` | `5242880` (5MB) | `512000000` (512MB) | Size of bytes to use in test |
| `--iterations` | `50` | `1` | `500000` | Number of iterations for iteration test |

<br />
<br />

```C#
xsum.exe --benchmark --buffer 32000000 --iterations 50
```

<br />
<br />

<p align="center"><img style="width: 85%;text-align: center;border: 1px solid #353535;" src="Docs/images/1.png"></p>

<br />

### Algorithm Stress Test Benchmark
This test focuses on one specified algorithm when performing a series of tasks. The test will generate a large group of text and then hash that text in groups; progressively sampling larger sizes.

<br />
<br />

This benchmark accepts two arguments:
| Argument | Default | Min Value | Max Value | Description |
| --- | --- | --- | --- | --- |
| `--algo` | `sha1` | - | - | The algorithm to perform the test for. |
| `--iterations` | `100000` | `1` | `500000` | Number of characters to use in sample. |

<br />
<br />

```C#
xsum.exe --benchmark --algo sha256 --iterations 100000
```

<br />
<br />

<p align="center"><img style="width: 85%;text-align: center;border: 1px solid #353535;" src="Docs/images/2.png"></p>


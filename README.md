<p align="center"><img src="Docs/images/banner.png" width="860"></p>
<h1 align="center"><b>xSum Hash Utility</b></h1>

<div align="center">

![Version](https://img.shields.io/github/v/tag/Aetherinox/xsum-shahash-utility?logo=GitHub&label=version&color=ba5225) ![Downloads](https://img.shields.io/github/downloads/Aetherinox/xsum-shahash-utility/total) ![Repo Size](https://img.shields.io/github/repo-size/Aetherinox/xsum-shahash-utility?label=size&color=59702a) ![Last Commit)](https://img.shields.io/github/last-commit/Aetherinox/xsum-shahash-utility?color=b43bcc)

</div>

<br />

---

<br />


- [About](#about)
  - [What Is A Hash Digest](#what-is-a-hash-digest)
- [Arguments](#arguments)
  - [Main Arguments](#main-arguments)
  - [Sub Arguments](#sub-arguments)
- [Syntax](#syntax)
    - [`--target`](#--target)
    - [`--digest`](#--digest)
    - [`--algorithm`](#--algorithm)
    - [`--output`](#--output)
    - [`--overwrite`](#--overwrite)
    - [`--progress`](#--progress)
    - [`--lowercase`](#--lowercase)
- [Features](#features)
  - [Benchmark](#benchmark)
    - [Standard Benchmark](#standard-benchmark)
    - [Algorithm Stress Test Benchmark](#algorithm-stress-test-benchmark)


<br />

---

<br />

# About

This utility allows you to generate, sign, and validate checksums.

While there are numerous apps available for doing things such as this; I need a tool that was very specific, and allowed for this to all be done very easily without complicated excessive options.

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
| `-t, --target` | Target folder or file to generate / verify hash for |
| `-d, --digest` | Hash digest which contains list of generated hashes |
| `-a, --algorithm` | Algorithm used to verify `--digest` |
| `-o, --output` | Output file for results verified |
| `-w, --overwrite` | Overwrite results to `--output` instead of append |
| `-p, --progress` | Displays each file being checked and additional info |
| `-l, --lowercase` | Match and output hash value(s) in lower case instead of upper case |
| `-c, --clipboard` | Copies the output hash value to clipboard. |
| `-b, --benchmark` | Performs benchmarks on a specified algorithm or all. |
| `-i, --iterations` | Number of iterations to perform using `--benchmark` |
| `-b, --buffer` | Buffer size to use when using `--benchmark` |

<br />

---

<br />

# Syntax
This utility attempts to be semi "smart" in the aspect that you can execute commands in various different ways. The reason for this is because out of the various other hashing tools out there, we try to use a similar syntax for this utilities' commands so that there's very little in means of transitioning, and to make it more comfortable.

<br />

### `--target`
Target folder or file to generate / verify hash for

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

---

<br />

</details>

<br />

### `--algorithm`
Algorithm used to verify --digest

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
Output file for results verified

<details>
<summary><sub>Read More</sub></summary>

<br />

The `--output` argument is an optional parameter which allows you to define a file where the results of your generated or verified results will be placed.

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

<details>
<summary><sub>Read More</sub></summary>

<br />

During normal operation when this utility is processing files, the hash that this utility generates is in upper-case characters. If your hash matches the one in the digest but has different casing, then the verification will fail. The casing of your digest hash, and the hash that the utility generates must be the exact same.

Without this argument enabled, the following verification will **fail**:

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

# Features
The following features are highlighted to explain them in better detail:

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


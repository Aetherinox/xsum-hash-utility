<p align="center"><img src="Docs/images/banner.png" width="860"></p>
<h1 align="center"><b>xSum Hash Utility</b></h1>

<div align="center">

![Version](https://img.shields.io/github/v/tag/Aetherinox/xsum-shahash-utility?logo=GitHub&label=version&color=ba5225) ![Downloads](https://img.shields.io/github/downloads/Aetherinox/xsum-shahash-utility/total) ![Repo Size](https://img.shields.io/github/repo-size/Aetherinox/xsum-shahash-utility?label=size&color=59702a) ![Last Commit)](https://img.shields.io/github/last-commit/Aetherinox/xsum-shahash-utility?color=b43bcc)

</div>

<br />

---

<br />


- [About](#about)
  - [Main Arguments](#main-arguments)
  - [Sub Arguments](#sub-arguments)
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

## Main Arguments
xSum mains the following main features:

<br />

| Argument | Description |
| --- | --- |
| `-a, --generate` | Generate a new hash digest from the `--target <FILE>` |
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


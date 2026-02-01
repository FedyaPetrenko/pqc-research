# PQC Research App

A comprehensive post-quantum cryptography (PQC) research application targeting **.NET 10** that benchmarks and compares emerging quantum-resistant algorithms with classical cryptographic methods.

## Overview

This solution contains **2 integrated applications** that explore the performance characteristics and network overhead of post-quantum cryptography algorithms introduced natively in **.NET 10**:

### Applications

#### 1. **PqcResearchApp** (Main Console Application)
The primary research application that performs two types of analysis:

- **Artifact Size Analysis**: Measures network overhead by comparing cryptographic artifact sizes (public keys, private keys, ciphertexts, signatures) across algorithms
- **Performance Benchmarks**: Executes detailed speed and memory consumption benchmarks using BenchmarkDotNet

#### 2. **Benchmarks Project**
Dedicated benchmarking suite using BenchmarkDotNet for rigorous performance testing and profiling.

---

## Algorithms Evaluated

### Classical (Comparison Baseline)
- **RSA-4096**: Asymmetric encryption
- **ECC P-256**: Key encapsulation
- **ECC P-384**: Digital signatures

### Post-Quantum (Native .NET 10 - Windows 11 24H2+)
- **ML-KEM-768** (Kyber): Key encapsulation mechanism
- **ML-DSA-65** (Dilithium): Digital signature algorithm

---

## Key Features

✅ **Native .NET 10 Support**: Leverages new `System.Security.Cryptography` PQC APIs  
✅ **Comprehensive Benchmarking**: Speed, memory allocation, and throughput analysis  
✅ **Artifact Size Comparison**: Real-world network overhead measurements  
✅ **Platform Validation**: Automatic PQC hardware support detection  

---

## Project Structure

---

## Requirements

- **.NET 10** or later
- **Windows 11 24H2** or later (for PQC hardware support)
- Visual Studio 2026 or compatible IDE

> **Note**: PQC algorithms require specific OS/hardware support. The application will throw `PlatformNotSupportedException` if running on unsupported platforms.

---

## Getting Started

### 1. Clone the Repository

### 2. Build the Solution

### 3. Run the Application

### Expected Output
The application executes in two phases:

**Phase 1: Artifact Size Analysis**

**Phase 2: Performance Benchmarks**
- Throughput (ops/sec)
- Memory allocations
- Latency comparisons

---

## Research Findings

This project demonstrates:

1. **Transition Feasibility**: PQC algorithms are now natively supported in .NET 10
2. **Performance Characteristics**: ML-KEM and ML-DSA benchmarks vs. classical methods
3. **Resource Trade-offs**: Artifact size increases and memory implications

---

## Technical Details

### Key Services

| Service | Purpose | Notes |
|---------|---------|-------|
| `MlKemService` | Key encapsulation | Generates shared secrets; requires platform support |
| `MlDsaService` | Digital signatures | Message signing and verification |
| `RsaService` | Classical baseline | RSA-4096 for comparison |
| `EccKemService` | ECC encapsulation | P-256 curve baseline |

### Artifact Export
All cryptographic materials can be exported in standard formats:
- **Public Keys**: SubjectPublicKeyInfo (SPKI)
- **Private Keys**: PKCS#8 format
- **Ciphertexts/Signatures**: Raw byte arrays

---

## Performance Metrics

The benchmarks measure:
- **Latency**: Time per operation (ms/op)
- **Throughput**: Operations per second
- **Memory**: Allocated bytes and GC pressure
- **Gen 2 Collections**: Garbage collection efficiency

---

## References

- [NIST PQC Standardization](https://csrc.nist.gov/projects/post-quantum-cryptography)
- [ML-KEM (Kyber) Specification](https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.203.pdf)
- [ML-DSA (Dilithium) Specification](https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.204.pdf)
- [.NET 10 Cryptography APIs](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography)

---

**Last Updated**: February 2026  
**Target Framework**: .NET 10
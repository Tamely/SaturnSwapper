# Workflow Guide

This repo can be built for the following platforms, using the provided setup and the following instructions. Before attempting to clone or build, please check these requirements:

## Build Requirements

Currently, the only requirement is the ability to run on Windows 10/11 x64. Later, we plan on adding Mobile and MacOS support.

Additionally, keep in mind that cloning this repo takes roughly 1GB of disk space. This will definitely be increase over time, so consider this to be a minimum for working with this repository.

## Concepts

The repository can be built from a regular, non-administrator command prompt, from the root of the repo, as follows:

For Windows:
```cmd
dotnet build
```

This builds the project (in the default debug configuration) and outputs the result to the SaturnClient/bin/Debug directory.

## How to download and compile the code

1. Install [Git](https://gitforwindows.org/)
2. Clone the repo:
```cmd
git clone https://github.com/Tamely/SaturnSwapper -b master --recursive
```
3. Navigate to the SaturnSwapper directory:
```cmd
cd SaturnSwapper
```
4. Build the project:
```cmd
dotnet build
```
5. Run the executable from the SaturnClient/bin/Debug directory

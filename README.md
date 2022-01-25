# SaturnSwapper
Most advanced Fortnite Skin Swapper to this date. UI made in HTML and Backend made in C#

## Prerequisites
- [.NET 6 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.0-windows-x64-installer)
- [.NET 6 SDK (if you are building yourself)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.101-windows-x64-installer)
- [WebView2 Runtime (comes with Windows 11 and you need the x64 Evergreen Bootstrapper)](https://developer.microsoft.com/en-us/microsoft-edge/webview2/#download-section)

## Usage
<details>
<summary>Building from source</summary>

The source on the swapper is 100% complete which means you can build it without any extra steps. To do this:
1. Turn off your antivirus because the swapper is flagged as a virus due to false positives with WebView2 (You have the source so you know it isn't a virus).
2. Clone the repository
    ```
    git clone https://github.com/Tamely/SaturnSwapper
    cd SaturnSwapper
    ```
3. Remove 'SaturnBot' project from the .sln
    Delete the lines:
    ```
    Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SaturnBot", "SaturnBot\SaturnBot.csproj", "{9B658498-B58B-4DB5-B275-4BFB59FAD1AD}"
    EndProject
    ```
3. Build the solution
    ``` 
    dotnet build
    ```
4. Make sure you have all the prerequisites installed.
5. Navigate to Saturn.Client\bin\Debug\net6.0-windows\Saturn.exe and open it.
6. Get your key to access the swapper [here](https://linkvertise.com/88495/saturn-swapper-key/) (they expire every 48 hours).
7. You're all set!
</details>

<details>
<summary>Downloading (prefered method)</summary>

1. Head to the [releases](https://github.com/Tamely/SaturnSwapper/releases) tab on the right side of the Saturn GitHub page.
2. Under the topmost release, click the file labled "Saturn.zip"
3. Turn off your antivirus because the swapper is flagged as a virus due to false positives with WebView2 (It's open source so you know it isn't a virus).
4. Make sure you have all the prerequisites installed.
5. Launch Saturn.exe
6. Get your key to access the swapper [here](https://linkvertise.com/88495/saturn-swapper-key/) (they expire every 48 hours).
7. You're all set!
</details>

## Contributing

Contributions are always welcome as it helps keep the project working!
- To make a contribution, fork the repository, make your change to the code, then submit a pull request on this repo.
- Let me (Tamely#6469) know when you make a pull request so I can give you the contributor role on Saturn's Discord server.

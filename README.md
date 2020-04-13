# Angular Language Service for Visual Studio

This is an extension to Visual Studio to provide IntelliSense for Angular templates.

# Build

Prerequisites:
- NuGet - [Direct Installation Link](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe)
- Visual Studio 2019 with the "Visual Studio extension development" workload selected
- npm

Open a Developer Command Prompt for your Visual Studio install (note that there is a Developer Command Prompt for each of your Visual Studio installations). You can find it by searching the start menu. Install the NuGet packages for the solution and run msbuild (npm packages for typescript and the angular language service plugin will be installed automatically):

```
> nuget restore
> msbuild AngularLanguageService.sln
```

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

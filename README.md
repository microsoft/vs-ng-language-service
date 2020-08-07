# Angular Language Service for Visual Studio

![ALS](https://media.giphy.com/media/Xxob53hbQT0MPaeDqF/giphy.gif)

This is an extension made to bring in the Angular Language Service to Angular templates being created in Visual Studio

**Minimum version of Visual Studio Needed:** 16.5.0

Download the extension [here](https://marketplace.visualstudio.com/items?itemName=TypeScriptTeam.AngularLanguageService&ssr=false#review-details)


## Features

* Completions List
* AOT Diagnostic messages
* Quick Info

**Coming Soon:** Go to Definition

## Usage

1. Download the Extension from the Visual Studio Marketplace.

2. Open a Visual Studio solution containing an Angular Project

3. Open a .ts or html Angular file you should see the following in the Output Window:

![Output Window](https://uzpxja.sn.files.1drv.com/y4m3m3SBmJRyfKCfXs_KhtEHNFw7eXHwFBMbqVDfTmL6ZbHREv_brszarakz90TN7ilTgh4wmV-rxW_5uZ9fkwOdo1ISMm-oEzENEnx-SSMhE6ehQZnDqDvB8hVkjZfLCBH6dx4HqPaEqLVj1GJCsmdFY2YCbWKv80ON5qKYTB9D3GDmdqXFddN3sKlcC1F5oF-lbE5pnDWWA-Lqe0oD7ZZ7w?width=512&height=164&cropmode=none)

# Local Build

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

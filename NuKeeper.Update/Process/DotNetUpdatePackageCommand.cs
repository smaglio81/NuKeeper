using System;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public class DotNetUpdatePackageCommand : IDotNetUpdatePackageCommand
    {
        private readonly IExternalProcess _externalProcess;

        public DotNetUpdatePackageCommand(IExternalProcess externalProcess)
        {
            _externalProcess = externalProcess;
        }

        public async Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            if (currentPackage == null)
            {
                throw new ArgumentNullException(nameof(currentPackage));
            }

            if (packageSource == null)
            {
                throw new ArgumentNullException(nameof(packageSource));
            }

            if (allSources == null)
            {
                throw new ArgumentNullException(nameof(allSources));
            }

            var projectPath = currentPackage.Path.Info.DirectoryName;
            var projectFileNameCommandLine = ArgumentEscaperWrapper.EscapeAndConcatenate(new string[] { currentPackage.Path.Info.Name });
            var sourceUrl = UriEscapedForArgument(packageSource.SourceUri);

            // ARPRDEVOPS-847 & ARPRDEVOPS-848 - The new dotnet.exe (tested on 9) has a real issue parsing local paths
            //  (ie. "C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\"). I felt like the best solution was to
            //  remove the articulated list of sources and just let the system use the default nuget.config for the current user.
            //var sources = allSources.CommandLine("-s");
            //var restoreCommand = $"restore {projectFileNameCommandLine} {sources}";

            var restoreCommand = $"restore {projectFileNameCommandLine}";
            await _externalProcess.Run(projectPath, "dotnet", restoreCommand, true);

            if (currentPackage.Path.PackageReferenceType == PackageReferenceType.ProjectFileOldStyle)
            {
                var removeCommand = $"remove {projectFileNameCommandLine} package {currentPackage.Id}";
                await _externalProcess.Run(projectPath, "dotnet", removeCommand, true);
            }

            var addCommand = $"add {projectFileNameCommandLine} package {currentPackage.Id} -v {newVersion} -s {sourceUrl} --no-restore";
            await _externalProcess.Run(projectPath, "dotnet", addCommand, true);
        }

        private static string UriEscapedForArgument(Uri uri)
        {
            if (uri == null)
            {
                return string.Empty;
            }

            return ArgumentEscaperWrapper.EscapeAndConcatenate(new string[] { uri.ToString() });
        }
    }
}

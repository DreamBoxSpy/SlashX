using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Repositories;
using NuGet.RuntimeModel;
using NuGet.Versioning;
using SlashX.Exceptions;
using SlashX.Services.Interfaces;
using SlashX.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace SlashX.Services
{
    internal class LibraryManager : ILibraryManager
    {
        public string LibrariesRoot { get; } 
        public string PackagesRoot { get; }

        private readonly ILogger logger;
        private readonly NuGetv3LocalRepository localRepository;
        private readonly PackageSourceProvider packageSourceProvider;
        private readonly ISettings nugetSettings;
        private readonly SourceRepository[] sourceRepositories;
        private readonly NuGetFramework appFramework;
        private readonly SourceCacheContext sourceCacheContext;
        private readonly NuGet.Common.ILogger nugetLogger;

        private class NuGetLogger(ILogger logger) : NuGet.Common.LoggerBase
        {
            public override void Log(NuGet.Common.ILogMessage message)
            {
                logger.Log(message.Level switch
                {
                    NuGet.Common.LogLevel.Error => LogLevel.Error,
                    NuGet.Common.LogLevel.Warning => LogLevel.Warning,
                    NuGet.Common.LogLevel.Information => LogLevel.Information,
                    NuGet.Common.LogLevel.Minimal => LogLevel.Information,
                    NuGet.Common.LogLevel.Debug => LogLevel.Debug,
                    NuGet.Common.LogLevel.Verbose => LogLevel.Trace,
                    _ => LogLevel.Trace
                }, message.Message);
            }

            public override Task LogAsync(NuGet.Common.ILogMessage message)
            {
                Log(message);
                return Task.CompletedTask;
            }
        }

        public LibraryManager(
            ILogger<LibraryManager> logger,
            IOptions<SlashXConfiguration> conf
        )
        {
            var dataRoot = conf.Value.DataRoot;
            LibrariesRoot = Path.Combine(dataRoot, "libraries");
            PackagesRoot = Path.Combine(dataRoot, "packages");

            Directory.CreateDirectory(LibrariesRoot);
            Directory.CreateDirectory(PackagesRoot);

            Environment.SetEnvironmentVariable("NUGET_PACKAGES", LibrariesRoot + '\\');

            nugetSettings = Settings.LoadDefaultSettings(AppDomain.CurrentDomain.BaseDirectory);

            this.logger = logger;

            packageSourceProvider = new PackageSourceProvider(nugetSettings);

            localRepository = new(PackagesRoot);
            

            sourceRepositories = [ 
                Repository.Factory.GetCoreV3(PackagesRoot, FeedType.FileSystemV2),
                ..packageSourceProvider.LoadPackageSources()
                .Select(x => Repository.Factory.GetCoreV3(x))
                ];

            appFramework = NuGetFramework.Parse(
                typeof(LibraryManager).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName
                );
            sourceCacheContext = new();

            nugetLogger = new NuGetLogger(logger);
        }

        private bool ResolvePackage(string packageId, VersionRange? version, [NotNullWhen(true)] out string? root)
        {
            if(version == null)
            {
                version = new(floatRange: new FloatRange(NuGetVersionFloatBehavior.AbsoluteLatest));
            }

            var packageRoot = Path.Combine(LibrariesRoot, packageId.ToLower());
            if (!Directory.Exists(packageRoot))
            {
                root = null;
                return false;
            }

            NuGetVersion? bestVersion = null;

            foreach(var v in Directory.EnumerateDirectories(packageRoot, "*", SearchOption.TopDirectoryOnly))
            {
                using var reader = new PackageFolderReader(v);
                var ver = reader.NuspecReader.GetVersion();

                if(bestVersion == null && version.Satisfies(ver))
                {
                    bestVersion = ver;
                }
                else if(version.IsBetter(bestVersion, ver))
                {
                    bestVersion = ver;
                }
            }

            if(bestVersion == null)
            {
                root = null;
                return false;
            }

            root = Path.Combine(packageRoot, bestVersion.ToString());
            return Directory.Exists(root);
        }

        private async Task CleanupUnusedFiles(string packageDir, CancellationToken cancellationToken)
        {
            foreach (var v in Directory.EnumerateFiles(packageDir, "*.nupkg", SearchOption.TopDirectoryOnly))
            {
                File.Delete(v);
            }
            foreach (var v in Directory.EnumerateFiles(packageDir, "*.a", SearchOption.AllDirectories))
            {
                File.Delete(v);
            }

            using var reader = new PackageFolderReader(packageDir);

            var reducer = new FrameworkReducer();
            var bestFramework = reducer.GetNearest(appFramework, await reader.GetSupportedFrameworksAsync(cancellationToken));

           
            // Libraries
            foreach (var group in await reader.GetLibItemsAsync(cancellationToken))
            {
                if (group.TargetFramework == bestFramework)
                {
                    continue;
                }
                foreach(var v in group.Items)
                {
                    var path = Path.Combine(packageDir, v);
                    if(File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }

            // Runtimes
            var runtimePath = Path.Combine(packageDir, "runtimes");
            if(Directory.Exists(runtimePath))
            {
                var graph = new RuntimeGraph([new RuntimeDescription(RuntimeInformation.RuntimeIdentifier)]);
                foreach(var v in Directory.EnumerateDirectories(runtimePath, "*", SearchOption.TopDirectoryOnly))
                {
                    var id = Path.GetFileName(v).ToLower();
                    if(!graph.AreCompatible(RuntimeInformation.RuntimeIdentifier, id))
                    {
                        Directory.Delete(v, true);
                        continue;
                    }

                    foreach(var f in Directory.EnumerateFiles(v, "*.pdb", SearchOption.AllDirectories))
                    {
                        File.Delete(f);
                    }
                }
            }
        }

        private bool CanSkip(string packageId)
        {
            return packageId.StartsWith("slashx.", StringComparison.OrdinalIgnoreCase) ||
                        packageId.Equals("slashx", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> DownloadPackage(string packageId, VersionRange version, CancellationToken cancellationToken)
        {
            NuGetVersion? bestVersion = null;

            foreach (var v in localRepository.FindPackagesById(packageId))
            {
                var ver = v.Version;

                if (bestVersion == null && version.Satisfies(ver))
                {
                    bestVersion = ver;
                }
                else if (version.IsBetter(bestVersion, ver))
                {
                    bestVersion = ver;
                }
            }
            
            if(bestVersion != null)
            {
                var package = localRepository.FindPackage(packageId, bestVersion);
                return package.ExpandedPath;
            }

            foreach (var v in sourceRepositories)
            {
                var resource = await v.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
                foreach (var ver in await resource.GetAllVersionsAsync(packageId, sourceCacheContext, nugetLogger, cancellationToken))
                {
                    if (bestVersion == null && version.Satisfies(ver))
                    {
                        bestVersion = ver;
                    }
                    else if (version.IsBetter(bestVersion, ver))
                    {
                        bestVersion = ver;
                    }
                }
                if (bestVersion == null)
                {
                    continue;
                }

                var packageFolder = Path.Combine(LibrariesRoot, packageId.ToLower(), bestVersion.ToString());

                var downloadResource = await v.GetResourceAsync<DownloadResource>(cancellationToken);

                {
                    using var result = await downloadResource.GetDownloadResourceResultAsync(new(packageId, bestVersion), new(sourceCacheContext), LibrariesRoot,
                        nugetLogger, cancellationToken);

                    if(!Directory.Exists(packageFolder))
                    {
                        foreach(var f in await result.PackageReader.GetFilesAsync(cancellationToken))
                        {
                            if(f.EndsWith('/') || f.EndsWith('\\'))
                            {
                                continue;
                            }

                            using var pfs = await result.PackageReader.GetStreamAsync(f, cancellationToken);

                            var path = Path.Combine(packageFolder, f);

                            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                            using var fs = File.OpenWrite(path);
                            await pfs.CopyToAsync(fs);
                        }
                    }
                }

                await CleanupUnusedFiles(packageFolder, cancellationToken);

                return packageFolder;
            }

            throw new FileNotFoundException($"The specified NuGet package cannot be found: {packageId} {version}");

        }

        public Task FixDependencies(string packageId, CancellationToken cancellation)
        {

            string packageName;
            string? packageVersion;

            var splitIdx = packageId.IndexOf('-');
            if (splitIdx == -1)
            {
                packageName = packageId;
                packageVersion = null;
            }
            else
            {
                packageName = packageId[..splitIdx];
                packageVersion = packageId[(splitIdx + 1)..];
            }
            return FixDependencies(packageName, packageVersion, cancellation);
        }
        public Task FixDependencies(string packageId, string? version, CancellationToken cancellationToken)
        {
            VersionRange ver = VersionRange.AllStable;

            if(!string.IsNullOrEmpty(version) && 
                VersionRange.TryParse(version, out var parsedVer))
            {
                ver = parsedVer;
            }

            return FixDependencies(packageId, 
                ver
                , cancellationToken);
        }

        public async Task FixDependencies(string packageId, VersionRange version, CancellationToken cancellationToken)
        {
            if (!ResolvePackage(packageId, version, out var packageFolder))
            {
                // Download package
                try
                {
                    packageFolder = await DownloadPackage(packageId, version, cancellationToken);
                }
                catch(FileNotFoundException)
                {
                    if (CanSkip(packageId))
                    {
                        return;
                    }
                    throw;
                }
            }
            var reader = new PackageFolderReader(packageFolder);

            var reducer = new FrameworkReducer();
            var bestFramework = reducer.GetNearest(appFramework, await reader.GetSupportedFrameworksAsync(cancellationToken));
            
            foreach(var dep in await reader.GetPackageDependenciesAsync(cancellationToken))
            {
                if(dep.TargetFramework != bestFramework)
                {
                    continue;
                }

                foreach(var v in dep.Packages)
                {
                    await FixDependencies(v.Id, v.VersionRange, cancellationToken);
                }
            }
        }

        private async Task ResolvePackages(Dictionary<string, VersionRange> libraries, IEnumerable<string> packageId, 
            CancellationToken cancellationToken)
        {
            foreach(var v in packageId)
            {
                string packageName;
                string? packageVersion;

                var splitIdx = v.IndexOf('-');
                if(splitIdx == -1)
                {
                    packageName = v;
                    packageVersion = null;
                }
                else
                {
                    packageName = v[..splitIdx];
                    packageVersion = v[(splitIdx + 1)..];
                }

                VersionRange versionRange = VersionRange.AllStable;

                if (!string.IsNullOrEmpty(packageVersion))
                {
                    if (NuGetVersion.TryParse(packageVersion, out var ngVer))
                    {
                        versionRange = new(ngVer, null);
                    }
                    else if(VersionRange.TryParse(packageVersion, out var ngVerRange))
                    {
                        versionRange = ngVerRange;
                    }
                }

                if (!ResolvePackage(packageName, versionRange, out var packageFolder))
                {
                    if(CanSkip(packageName))
                    {
                        continue;
                    }
                    throw new FileNotFoundException($"The specified NuGet package cannot be found: {v}");
                }

                using var reader = new PackageFolderReader(packageFolder);

                libraries[packageName] = new(reader.NuspecReader.GetVersion());

                var reducer = new FrameworkReducer();
                var bestFramework = reducer.GetNearest(appFramework, await reader.GetSupportedFrameworksAsync(cancellationToken));

                foreach (var dep in await reader.GetPackageDependenciesAsync(cancellationToken))
                {
                    if (dep.TargetFramework != bestFramework)
                    {
                        continue;
                    }

                    foreach (var p in dep.Packages)
                    {
                        if(libraries.TryGetValue(p.Id, out var packageVer))
                        {
                            var oldVer = packageVer;
                            packageVer = NuGetVersionUtils.Intersect(packageVer, p.VersionRange) ?? 
                                throw new NuGetVersionConflictException($"NuGet version conflict: Package '{p.Id}' requires version ranges " +
                                    $"'{oldVer.ToNormalizedString()}' and '{p.VersionRange.ToNormalizedString()}' " +
                                    $"that have no intersection.");
                        }
                        else
                        {
                            packageVer = p.VersionRange;
                        }
                        libraries[p.Id] = packageVer;

                        await ResolvePackages(libraries, [$"{p.Id}-{p.VersionRange}"], cancellationToken);
                    }
                    
                }
            }
        }

        async Task<IEnumerable<string>> ILibraryManager.ResolveLibraries(CancellationToken cancellationToken, IEnumerable<string> packageId)
        {
            List<string> libs = [];
            Dictionary<string, VersionRange> libPackages = [];
            await ResolvePackages(libPackages, packageId, cancellationToken); 
            
            foreach((var id, var versionRange) in libPackages)
            {
                if (!ResolvePackage(id, versionRange, out var packageFolder))
                {
                    if (CanSkip(id))
                    {
                        continue;
                    }
                
                    throw new FileNotFoundException($"The specified NuGet package cannot be found: {id}");
                }
                using var reader = new PackageFolderReader(packageFolder);

                var reducer = new FrameworkReducer();
                var bestFramework = reducer.GetNearest(appFramework, await reader.GetSupportedFrameworksAsync(cancellationToken));

                foreach(var group in await reader.GetLibItemsAsync(cancellationToken))
                {
                    if(group.TargetFramework != bestFramework)
                    {
                        continue;
                    }
                    foreach(var v in group.Items)
                    {
                        if(!".dll".Equals(Path.GetExtension(v), StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        var path = Path.Combine(packageFolder, v);
                        libs.Add(path);
                    }
                }

                var runtimesPath = Path.Combine(packageFolder, "runtimes");
                if(Directory.Exists(runtimesPath))
                {
                    var dir = Directory.EnumerateDirectories(runtimesPath, "*", SearchOption.TopDirectoryOnly).FirstOrDefault();

                    if(dir != null)
                    {
                        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            libs.AddRange(Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories));
                        }
                        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            libs.AddRange(Directory.EnumerateFiles(dir, "*.so", SearchOption.AllDirectories));
                        }
                        else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            libs.AddRange(Directory.EnumerateFiles(dir, "*.dylib", SearchOption.AllDirectories));
                        }
                    }
                }
            }

            return libs;
        }

    }
}

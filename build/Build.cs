using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.PathConstruction;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target BuildDebugModules => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetPack(s => s.SetProject(RootDirectory)
                                        .SetConfiguration(Configuration));
        });

    Target BuildDebugCore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s => s.SetProjectFile(RootDirectory));
        });

    Target Publish => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s => s.SetProjectFile(RootDirectory)
                                       .SetConfiguration("Release"));
            DotNetTasks.DotNetPack(s => s.SetProject(RootDirectory)
                                        .SetConfiguration("Release"));
            DotNetTasks.DotNetPublish(s => s.SetProject(Path.Combine(RootDirectory, "SlashX.Shell"))
                                         .SetConfiguration("Release"));
        });



    Target Compile => _ => _
        .DependsOn(BuildDebugModules)
        .DependsOn(BuildDebugCore);

}

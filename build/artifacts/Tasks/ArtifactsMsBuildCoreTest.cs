using Common.Utilities;

namespace Artifacts.Tasks;

[TaskName(nameof(ArtifactsMsBuildCoreTest))]
[TaskDescription("Tests the msbuild package in docker container")]
[TaskArgument(Arguments.DockerRegistry, Constants.DockerHub, Constants.GitHub)]
[TaskArgument(Arguments.DockerDotnetVersion, Constants.Version60, Constants.Version70)]
[TaskArgument(Arguments.DockerDistro, Constants.AlpineLatest, Constants.DebianLatest, Constants.UbuntuLatest)]
[IsDependentOn(typeof(ArtifactsPrepare))]
public class ArtifactsMsBuildCoreTest : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        var shouldRun = true;
        shouldRun &= context.ShouldRun(context.IsDockerOnLinux, $"{nameof(ArtifactsMsBuildCoreTest)} works only on Docker on Linux agents.");

        return shouldRun;
    }

    public override void Run(BuildContext context)
    {
        if (context.Version == null)
            return;
        var rootPrefix = string.Empty;
        var version = context.Version.NugetVersion;

        foreach (var dockerImage in context.Images)
        {
            if (context.SkipImageForArtifacts(dockerImage)) continue;

            var framework = dockerImage.TargetFramework;

            var targetFramework = framework switch
            {
                Constants.Version60 or Constants.Version70 => $"net{framework}",
                _ => framework
            };

            var cmd = $"{rootPrefix}/scripts/test-msbuild-task.sh --version {version} --nugetPath {rootPrefix}/nuget --repoPath {rootPrefix}/repo/tests/integration --targetframework {targetFramework}";

            context.DockerTestArtifact(dockerImage, cmd);
        }
    }
}

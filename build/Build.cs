using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.Tools.Docker.DockerTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.BuildDocker);

    [GitVersion(Framework = "netcoreapp3.1")] readonly GitVersion GitVersion;

    [Parameter] readonly string DockerRegistryUsername;
    [Parameter] readonly string DockerRegistryPassword;
    [Parameter] readonly string DockerImageName = "mssql-tmpfs";
    [Parameter] readonly string DockerOrganization = "dangl";

    Target BuildDocker => _ => _
        .Executes(() =>
        {
            DockerBuild(c => c
                .SetFile(RootDirectory / "Dockerfile")
                .SetTag($"{DockerImageName}:dev")
                .SetPath(".")
                .SetPull(true)
                .SetProcessWorkingDirectory(RootDirectory));
        });

    Target PushDocker => _ => _
        .DependsOn(BuildDocker)
        .Requires(() => DockerRegistryUsername)
        .Requires(() => DockerRegistryPassword)
        .Executes(() =>
        {
            DockerLogin(x => x
                .SetUsername(DockerRegistryUsername)
                .SetPassword(DockerRegistryPassword)
                .DisableProcessLogOutput());

            PushDockerWithTagAsync("dev");

            if (GitVersion.BranchName.Equals("master") || GitVersion.BranchName.Equals("origin/master"))
            {
                PushDockerWithTagAsync("latest");
                PushDockerWithTagAsync(GitVersion.SemVer);
            }
        });

    private void PushDockerWithTagAsync(string tag)
    {
        DockerTag(c => c
            .SetSourceImage($"{DockerImageName}:dev")
            .SetTargetImage($"{DockerOrganization}/{DockerImageName}:{tag}"));
        DockerPush(c => c
            .SetName($"{DockerOrganization}/{DockerImageName}:{tag}"));
    }
}

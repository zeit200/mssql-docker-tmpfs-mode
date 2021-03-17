# mssql-docker-tmpfs
[Forked from t-oster/mssql-docker-zfs](https://github.com/t-oster/mssql-docker-zfs)
Hack to run the mssql-docker containers with a tmpfs filesystem mounted as data dir

## C# Example

```csharp
var sqlContainer = await dockerClient
    .Containers
    .CreateContainerAsync(new CreateContainerParameters
    {
        Name = SQLSERVER_CONTAINER_NAME_PREFIX + Guid.NewGuid(),
        Image = "dangl/mssql-tmpfs:latest",
        Env = new List<string>
        {
            "ACCEPT_EULA=Y",
            $"SA_PASSWORD={SQLSERVER_SA_PASSWORD}"
        },
        HostConfig = new HostConfig
        {
            Tmpfs = new Dictionary<string, string>
            {
                {"/var/opt/mssql/data", "" },
                {"/var/opt/mssql/log", "" },
                {"/var/opt/mssql/secrets", "" }
            },
            Mounts = new List<Mount>
            {
                new Mount
                {
                    Type = "tmpfs",
                    TmpfsOptions = new TmpfsOptions
                    {
                        SizeBytes = 1_000_000_000_000
                    },
                    Target = "/var/opt/mssql/data"
                },
                new Mount
                {
                    Type = "tmpfs",
                    TmpfsOptions = new TmpfsOptions
                    {
                        SizeBytes = 1_000_000_000_000
                    },
                    Target = "/var/opt/mssql/log"
                },
                new Mount
                {
                    Type = "tmpfs",
                    TmpfsOptions = new TmpfsOptions
                    {
                        SizeBytes = 1_000_000_000_000
                    },
                    Target = "/var/opt/mssql/secrets"
                }
            },
            PortBindings = new Dictionary<string, IList<PortBinding>>
            {
                {
                    "1433/tcp",
                    new PortBinding[]
                    {
                        new PortBinding
                        {
                            HostPort = freePort
                        }
                    }
                }
            }
        }
  });
```

## Original Readme

**UPDATE: Since the release of ZFS 0.8 this is no longer necessary. ZFS 0.8 supports O_DIRECT and thus mssql runs fine without any modifications. This repository is only kept for archiving purposes.**

This is a way to mask the O_DIRECT flag of the open function, which the mssql-server uses and which causes the mssql container to not run on zfs.
Look at https://github.com/Microsoft/mssql-docker/issues/13 for more details. Thanks to @Mic92 for the hints and code.

To use it, just copy the nodirect_open.so to your container or link it like shown in the docker-compose.yml and add it to the LD_PRELOAD environment variable
(also like shown in the docker-compose.yml).
Alternatively just use the Dockerfile to create your own mssql-container which already contains the hack.

NO WARRANTY WHATSOEVER

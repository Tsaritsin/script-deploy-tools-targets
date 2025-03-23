# script-deploy-tools-targets
Targets for library script-deploy-tools:
- ScriptDeployTools.Targets.SqlServer - ![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Tsaritsin/script-deploy-tools-targets/tagged.yml) - deploy scripts to SQL Server 



## IsInitializeTarget
This scripts can deploy when target is not created (e.g. script to create database).

When database is not exists target auto change database to [master]

## Used
```csharp
var deployService = new DeployBuilder()
    .AddLogger(loggerFactory.CreateLogger<IDeploymentService>())
    .AddOptions(new DeploymentOptions
    {
        InsertMigrationScript = scripts.FirstOrDefault(x =>
            x is { IsService: true, ScriptKey: "INSERT_MIGRATION" })
    })
    .FromEmbeddedResources(options =>
    {
        options.Assemblies = [typeof(DeployHelper).Assembly];
    })
    .ToSqlServer(options =>
    {
        options.ConnectionString = connectionString;
        options.GetDeployedInfoScript = scripts.FirstOrDefault(x =>
            x is { IsService: true, ScriptKey: "GET_DEPLOYED_SCRIPTS" });
    })
    .Build();

await deployService.Deploy(scripts, cancellationToken);
```
A more detailed example is available [in this repository](https://github.com/Tsaritsin/script-deploy-tools/tree/main/Samples/SqlServerDeploy).

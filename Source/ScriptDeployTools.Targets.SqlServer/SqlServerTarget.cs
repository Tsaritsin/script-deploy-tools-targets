using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ScriptDeployTools.Targets.SqlServer;

internal class SqlServerTarget(
    ILogger logger,
    SqlServerTargetOptions options) : IDeployTarget
{
    #region Fields

    private readonly IDictionary<string, IDeployedInfo> _deployedScripts = new Dictionary<string, IDeployedInfo>();

    private bool _deployedScriptsIsLoaded;

    /// <summary>
    /// Already validated that database is existing
    /// </summary>
    private bool _databaseExists;

    #endregion

    #region Methods

    private static string SetParameters(string scriptContent,
                                        IDictionary<string, string?> parameters)
    {
        return parameters.Aggregate(
            scriptContent,
            (current, parameter) => current.Replace($"$({parameter.Key})", parameter.Value));
    }

    private async ValueTask<bool> DatabaseExists(CancellationToken cancellationToken)
    {
        if (_databaseExists)
            return true;

        logger.LogDebug("Connecting to database: {connectionString}", options.ConnectionString);

        await using var connection = new SqlConnection(options.ConnectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);

            logger.LogDebug("Database is exists");

            _databaseExists = true;

            return true;
        }
        catch (SqlException)
        {
            logger.LogDebug("Connecting failed, database is not exists");

            return false;
        }
    }

    private IDeployedInfo? ReturnDeployedInfo(string scriptKey)
    {
        var result = _deployedScripts.TryGetValue(scriptKey, out var info)
            ? info
            : null;

        if (result is not null)
            logger.LogDebug("Script {ScriptKey} is deployed", scriptKey);

        return result;
    }

    #endregion

    #region Implementation IDeployTarget

    public async ValueTask<IDeployedInfo?> GetDeployedInfo(string scriptKey,
                                                           CancellationToken cancellationToken)
    {
        var databaseExists = await DatabaseExists(cancellationToken);

        if (!databaseExists)
            return null;

        if (_deployedScriptsIsLoaded)
        {
            return ReturnDeployedInfo(scriptKey);
        }

        logger.LogDebug("Get applied migrations");

        await using var connection = new SqlConnection(options.ConnectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = options.GetDeployedInfoScript!.Content!;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var key = reader.FromDb<string>(nameof(IDeployedInfo.ScriptKey));
            var hash = reader.FromDb<string>(nameof(IDeployedInfo.ContentsHash));

            if (string.IsNullOrWhiteSpace(key))
            {
                logger.LogWarning("Script key is empty");
                continue;
            }

            _deployedScripts[key] = new DeployedInfo(key) { ContentsHash = hash };
        }

        logger.LogDebug("Found {CountAppliedMigrations} applied migrations", _deployedScripts.Count);

        _deployedScriptsIsLoaded = true;

        return ReturnDeployedInfo(scriptKey);
    }

    public async Task DeployScript(IScript script,
                                   CancellationToken cancellationToken)
    {
        logger.LogDebug("Deploy {Script}", script.ScriptKey);

        var databaseExists = await DatabaseExists(cancellationToken);

        var canDeploy = databaseExists ||
                        script.IsInitializeTarget;

        if (!canDeploy)
        {
            logger.LogError("Database is not exists, skip deploy {Script}", script.ScriptKey);
            return;
        }

        string connectionString;

        if (!databaseExists)
        {
            var builder = new SqlConnectionStringBuilder(options.ConnectionString)
            {
                InitialCatalog = "master"
            };

            connectionString = builder.ConnectionString;
        }
        else
        {
            connectionString = options.ConnectionString!;
        }

        await using var connection = new SqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = SetParameters(script.Content!, script.ScriptParameters);

        await command.ExecuteNonQueryAsync(cancellationToken);

        var canCheckDeployedInfo = script is { IsInitializeTarget: false, IsService: false };

        if (canCheckDeployedInfo)
        {
            _deployedScripts[script.ScriptKey] = new DeployedInfo(script.ScriptKey);
        }
    }

    #endregion
}

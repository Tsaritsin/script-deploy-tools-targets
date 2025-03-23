namespace ScriptDeployTools.Targets.SqlServer;

/// <summary>
/// Represents the configuration options for targeting a SQL Server in the deployment process.
/// </summary>
public record SqlServerTargetOptions
{
    /// <summary>
    /// Gets or sets the connection string used for connecting to the SQL Server.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the script used for retrieving information about deployed scripts.
    /// This script is executed against the SQL Server to identify which scripts have been applied.
    /// </summary>
    public IScript? GetDeployedInfoScript { get; set; }
}

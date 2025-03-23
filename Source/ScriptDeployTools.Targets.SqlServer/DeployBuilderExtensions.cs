namespace ScriptDeployTools.Targets.SqlServer;

/// <summary>
/// Provides extension methods for configuring deployment builders to target SQL Server.
/// </summary>
public static class DeployBuilderExtensions
{
    /// <summary>
    /// Configures the deployment builder to target a SQL Server database.
    /// </summary>
    /// <param name="builder">The deployment builder to configure.</param>
    /// <param name="applyOptions">An action to configure the SQL Server target options.</param>
    /// <returns>The configured deployment builder.</returns>
    public static IDeployBuilder ToSqlServer(this IDeployBuilder builder,
                                             Action<SqlServerTargetOptions> applyOptions)
    {
        var options = new SqlServerTargetOptions();

        applyOptions(options);

        ArgumentNullException.ThrowIfNull(options.ConnectionString);
        ArgumentNullException.ThrowIfNull(options.GetDeployedInfoScript);

        builder.Target = new SqlServerTarget(
            builder.Logger ?? throw new ArgumentNullException(nameof(builder.Logger)),
            options);

        return builder;
    }
}

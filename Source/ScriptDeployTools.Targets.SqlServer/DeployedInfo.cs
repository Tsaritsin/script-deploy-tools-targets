namespace ScriptDeployTools.Targets.SqlServer;

/// <summary>
///     Represents a script that has been deployed, with metadata about its name and contents.
/// </summary>
public record DeployedInfo(
    string ScriptKey)
    : IDeployedInfo
{
    /// <summary>
    ///     Hash of script content, used to verify integrity.
    /// </summary>
    public string? ContentsHash { get; set; }
}

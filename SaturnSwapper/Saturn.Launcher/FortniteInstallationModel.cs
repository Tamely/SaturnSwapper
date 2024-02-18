namespace Saturn.Launcher;

public class InstallationList
{
    public string InstallLocation { get; set; }
    public string NamespaceId { get; set; }
    public string ItemId { get; set; }
    public string ArtifactId { get; set; }
    public string AppVersion { get; set; }
    public string AppName { get; set; }
}

public class FortniteInstallationModel
{
    public List<InstallationList> InstallationList { get; set; }
}
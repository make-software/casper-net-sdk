namespace Casper.Network.SDK.Types
{
    /// <summary>
    /// The state of the reactor.
    /// </summary>
    public enum ReactorState
    {
        Initialize,
        CatchUp,
        Upgrading,
        KeepUp,
        Validate,
        ShutdownForUpgrade
    }
}

namespace Shop.Ordering;

/// <summary>
/// Configures the timing of saga, reminder, and retry behavior.
/// </summary>
public sealed class ScenarioOptions
{
    public int SagaTimeoutSeconds { get; set; } = 30;
    public int ReminderSeconds { get; set; } = 10;
    public int RetryDelaySeconds { get; set; } = 2;
    public int BatchTimeoutSeconds { get; set; } = 2;
}

namespace Domain;

public static class GlobalHealthState
{
    public static volatile bool IsDefaultApiDown;
    public static volatile bool IsFallbackApiDown;

    public static volatile int MinTimeToWaitForDefaultApi;
    public static volatile int MinTimeToWaitForFallbackApi;
}

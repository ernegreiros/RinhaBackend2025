namespace Models;

public class HealthCheck
{
    public bool Failing { get; set; }
    public int MinResponseTime { get; set; }
}

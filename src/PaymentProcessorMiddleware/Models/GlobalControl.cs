namespace Models;

public class RouteGlobalControl
{
    public string Url { get; set; } = "http://default:8080";
    public string Service { get; set; } = "default";
    public bool Failing { get; set; } = false;
    public int MinResponseTime { get; set; } = 0;
}
public interface IRouteGlobalControlService
{
    RouteGlobalControl Get();
    void Update(RouteGlobalControl updated);
}

public class RouteGlobalControlService : IRouteGlobalControlService
{
    private readonly RouteGlobalControl _current = new();

    public RouteGlobalControl Get() => _current;

    public void Update(RouteGlobalControl newControl)
    {
        Console.WriteLine($"Update to {newControl.Service} | {newControl.Url} | {newControl.Failing} | {newControl.MinResponseTime}");
        _current.Url = newControl.Url;
        _current.Service = newControl.Service;
        _current.Failing = newControl.Failing;
        _current.MinResponseTime = newControl.MinResponseTime;
    }
}
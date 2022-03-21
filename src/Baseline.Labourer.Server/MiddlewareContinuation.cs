namespace Baseline.Labourer.Server;

/// <summary>
/// Response from certain middleware functions that define whether middlewares should continue executing or whether
/// they should end.
/// </summary>
public enum MiddlewareContinuation
{
    Continue,
    Abort
}
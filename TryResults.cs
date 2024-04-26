using System;
using System.Threading.Tasks;

namespace RelinkModOrganizer;

public class TryResult(bool success, string? errorMessage = null)
{
    public bool Success { get; } = success;
    public string? ErrorMessage { get; } = errorMessage;
}

public static class TryResults
{
    public static TryResult Ok() =>
        new(true);

    public static TryResult Error(string errorMessage) =>
        new(false, errorMessage);

    public static TryResult Except(this TryResult result, Action<string> action)
    {
        if (!result.Success &&
            !string.IsNullOrWhiteSpace(result.ErrorMessage))
            action(result.ErrorMessage);

        return result;
    }
}
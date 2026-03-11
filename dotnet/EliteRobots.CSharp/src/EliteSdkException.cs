namespace EliteRobots.CSharp;

public sealed class EliteSdkException : Exception
{
    public int StatusCode { get; }

    public EliteSdkException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

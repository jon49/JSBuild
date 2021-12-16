using System.Runtime.Serialization;

namespace JSBuild.Exceptions;

internal class StartProcessException : Exception
{
    public StartProcessException()
    {
    }

    public StartProcessException(string? message) : base(message)
    {
    }

    public StartProcessException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected StartProcessException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

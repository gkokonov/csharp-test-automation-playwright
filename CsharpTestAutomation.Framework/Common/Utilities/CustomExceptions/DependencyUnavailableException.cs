namespace CsharpTestAutomation.Framework.Common.Utilities.CustomExceptions;

[Serializable]
public class DependencyUnavailableException : Exception
{
    public DependencyUnavailableException()
    {
    }

    public DependencyUnavailableException(string message)
        : base(message)
    {
    }

    public DependencyUnavailableException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

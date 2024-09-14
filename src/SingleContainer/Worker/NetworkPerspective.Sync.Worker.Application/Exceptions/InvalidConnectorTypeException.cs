namespace NetworkPerspective.Sync.Worker.Application.Exceptions;

public class InvalidConnectorTypeException : ApplicationException
{
    public InvalidConnectorTypeException(string connectorType) : base($"Connector type '{connectorType}' is invalid")
    { }
}
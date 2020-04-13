namespace Goodday.Core.Domain
{
    public interface IMessage : ICanTurnIntoBytes
    {
        Header Header { get; }
    }
}
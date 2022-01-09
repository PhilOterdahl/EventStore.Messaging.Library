namespace EventStore.Library.Messaging.Command;

public interface ICommand : IMessage
{

}

public interface ICommand<T> : ICommand
{
}
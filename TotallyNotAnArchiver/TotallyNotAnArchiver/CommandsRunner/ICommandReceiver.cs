namespace TotallyNotAnArchiver.CommandsRunner
{
    public interface ICommandReceiver
    {
        void Receive(string[]command);
    }
}
using TotallyNotAnArchiver.CommandsRunner;

namespace TotallyNotAnArchiver
{
    class Program
    {
        static void Main(string[] args)
        {
            new CommandReceiver().Receive(args);
        }
    }
}
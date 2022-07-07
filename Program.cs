using System;
using Protocol;

namespace Project
{
    class Program
    {
        static System system;
        static NetworkManager networkManager;

        static void PrintUsageAndExit()
        {
            Console.WriteLine("Usage: dotnet run owner index");
            global::System.Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            if (args.Length != 2) PrintUsageAndExit();

            string owner = args[0];
            int index = int.Parse(args[1]);

            system = new System {
                HUB_HOST = "127.0.0.1",
                HUB_PORT = 5000,
                SELF_HOST = "127.0.0.1",
                SELF_PORT = 5010 + index,
                SELF_OWNER = owner,
                SELF_INDEX = index
            };
            system.EnsureAlgorithm("app");

            networkManager = new NetworkManager(system);

            networkManager.StartListener();
            registerToHub();
        }

        static void registerToHub()
        {
            var procRegistration = new ProcRegistration
            {
                Owner = system.SELF_OWNER,
                Index = system.SELF_INDEX
            };

            var message = new Message
            {
                Type = Message.Types.Type.ProcRegistration,
                MessageUuid = global::System.Guid.NewGuid().ToString(),
                ProcRegistration = procRegistration
            };

            networkManager.SendNetworkMessage(message, system.HUB_HOST, system.HUB_PORT);
        }
    }
}

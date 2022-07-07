using System;
using System.Collections.Generic;
using System.Linq;
using Protocol;

namespace Project
{
    class System
    {
        public string HUB_HOST { get; set; }
        public int HUB_PORT { get; set; }
        public string SELF_HOST { get; set; }
        public int SELF_PORT { get; set; }
        public string SELF_OWNER { get; set; }
        public int SELF_INDEX { get; set; }

        public string SystemId { get; set; }
        public List<ProcessId> processes = new List<ProcessId>();
        public List<ProcessId> Processes {
            get { lock(processes) { return processes; } }
            set { lock(processes) { processes.Clear(); processes.AddRange(value); SetCurrentProcess(); } }
        }

        public ProcessId CurrentProcess { get; private set;}
        public NetworkManager NetworkManager { get; set; }

        private Dictionary<string, Algorithm> Algorithms = new Dictionary<string, Algorithm>();
        private readonly object algorithmsLock = new object();

        public void RegisterMessage(Message message)
        {
            EnsureAlgorithm(message.ToAbstractionId).RegisterMessage(message);
        }

        public void ClearSystem()
        {
            NetworkManager.Running = false;

            foreach(var algorithm in Algorithms.Values)
            {
                algorithm.Running = false;
            }
            Algorithms.Clear();
        }

        public Algorithm EnsureAlgorithm(string abstractionId)
        {
            lock (Algorithms) {
                if (! Algorithms.ContainsKey(abstractionId)) {
                    var lastAbstraction = abstractionId.Split('.').Last();
                    var (name, _) = Util.SplitInstanceId(lastAbstraction);
                    switch (name)
                    {
                        case "app":
                            Algorithms[abstractionId] = new App(this, abstractionId);
                            break;
                        case "pl":
                            Algorithms[abstractionId] = new PerfectLink(this, abstractionId);
                            break;
                        case "beb":
                            Algorithms[abstractionId] = new BestEffortBroadcast(this, abstractionId);
                            break;
                        case "nnar":
                            Algorithms[abstractionId] = new NNAtomicRegister(this, abstractionId);
                            break;

                        default:
                            throw new Exception($"Could not register abstraction with id {lastAbstraction}!");
                    }
                }
                return Algorithms[abstractionId];
            }
        }

        private void SetCurrentProcess()
        {
            foreach (var process in processes) {
                if (process.Owner == SELF_OWNER &&
                    process.Index == SELF_INDEX) {
                        CurrentProcess = process;
                        return;
                    }
            }
        }

        internal ProcessId GetProcessByHostAndPort(string senderHost, int senderListeningPort)
        {
            if (senderHost == HUB_HOST && senderListeningPort == HUB_PORT) return null;

            foreach (var process in Processes)
                if (process.Host == senderHost && process.Port == senderListeningPort)
                    return process;

            throw new ArgumentException($"Could not find process with host {senderHost} and port {senderListeningPort}");
        }
    }
}
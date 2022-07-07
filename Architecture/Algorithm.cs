using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Protocol;

namespace Project
{
    abstract class Algorithm
    {
        public System System { get; set; }
        public string AbstractionId { get; set; }

        public bool Running { get; set; }
        private List<Message> messagesToHandle = new List<Message>();

        public Algorithm(System system, string abstractionId)
        {
            System = system;
            AbstractionId = abstractionId;
            StartHandlingEvents();
        }

        public void RegisterMessage(Message message)
        {
            if (Running)
                (new Thread(() => {
                    lock (messagesToHandle)
                    {
                        messagesToHandle.Insert(0, message);
                        Monitor.Pulse(messagesToHandle);
                    }
                })).Start();
        }

        private void StartHandlingEvents()
        {
            Running = true;
            (new Thread(() => {
                while (Running) {
                    // wait for a message/event to arrive
                    lock (messagesToHandle) {
                        var unhandledMessages = new List<Message>();

                        while (messagesToHandle.Count > 0) {
                            // handle the message
                            var message = messagesToHandle[0];
                            messagesToHandle.RemoveAt(0);

                            if (! HandleMessage(message)) {
                                unhandledMessages.Add(message);
                            }
                        };
                        while (unhandledMessages.Count > 0) {
                            var message = unhandledMessages.First();
                            unhandledMessages.RemoveAt(0);
                            messagesToHandle.Add(message);
                        }
                        Monitor.Wait(messagesToHandle);
                    }
                }
            })).Start();
        }

        abstract protected bool HandleMessage(Message message);
    }
}
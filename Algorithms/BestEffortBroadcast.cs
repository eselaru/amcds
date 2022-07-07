using System;
using Protocol;

namespace Project
{
    class BestEffortBroadcast : Algorithm
    {
        public BestEffortBroadcast(System system, string abstractionId) : base(system, abstractionId)
        {

        }

        protected override bool HandleMessage(Message message)
        {
            switch (message.Type) {
                case Message.Types.Type.BebBroadcast: {
                    foreach (var process in System.Processes)
                    {
                        var plSendMessage = new Message {
                            MessageUuid = Guid.NewGuid().ToString(),
                            ToAbstractionId = AbstractionId + ".pl",
                            Type = Message.Types.Type.PlSend,
                            PlSend = new PlSend {
                                Destination = process,
                                Message = message.BebBroadcast.Message
                            }
                        };

                        System.RegisterMessage(plSendMessage);
                    }
                    break;
                };

                case Message.Types.Type.PlDeliver: {
                    var bebDeliver = new Message {
                        Type = Message.Types.Type.BebDeliver,
                        ToAbstractionId = Util.ParentAbstractionId(AbstractionId),
                        SystemId = message.SystemId,
                        BebDeliver = new BebDeliver {
                            Message = message.PlDeliver.Message,
                            Sender = message.PlDeliver.Sender
                        }
                    };

                    System.RegisterMessage(bebDeliver);
                    break;
                };
            }

            return true;
        }
    }
}
using Protocol;

namespace Project
{
    class PerfectLink : Algorithm
    {
        public PerfectLink(System system, string abstractionId) : base(system, abstractionId)
        {

        }

        protected override bool HandleMessage(Message message)
        {
            switch (message.Type) {

                case Message.Types.Type.NetworkMessage: {
                    var networkMessage = message.NetworkMessage;

                    var plMessage = new Message {
                        SystemId = message.SystemId,
                        ToAbstractionId = Util.ParentAbstractionId(AbstractionId),
                        MessageUuid = message.MessageUuid,
                        Type = Message.Types.Type.PlDeliver,
                        PlDeliver = new PlDeliver {
                            Sender = System.GetProcessByHostAndPort(networkMessage.SenderHost, networkMessage.SenderListeningPort),
                            Message = networkMessage.Message
                        }
                    };

                    System.RegisterMessage(plMessage);
                    break;
                };

                case Message.Types.Type.PlSend: {
                    var networkMessage = new Message {
                        SystemId = System.SystemId,
                        ToAbstractionId = message.ToAbstractionId,
                        Type = Message.Types.Type.NetworkMessage,
                        NetworkMessage = new NetworkMessage {
                            SenderHost = System.SELF_HOST,
                            SenderListeningPort = System.SELF_PORT,
                            Message = message.PlSend.Message
                        }
                    };
                    System.NetworkManager.SendMessage(
                        networkMessage,
                        message.PlSend.Destination.Host,
                        message.PlSend.Destination.Port
                    );
                    break;
                };
            }

            return true;
        }
    }
}
using System;
using System.Linq;
using Protocol;

namespace Project
{
    class App : Algorithm
    {
        public static string InstanceName = "app";

        public App(System system, string abstractionId) : base(system, abstractionId)
        {

        }

        protected override bool HandleMessage(Message message)
        {
            switch (message.Type) {
                // messages directly from hub
                case Message.Types.Type.PlDeliver: {
                    var innerMessage = message.PlDeliver.Message;

                    switch (innerMessage.Type) {
                        case Message.Types.Type.ProcInitializeSystem:
                            System.Processes = innerMessage.ProcInitializeSystem.Processes.ToList();
                            System.SystemId = message.SystemId;
                            Console.WriteLine($"Starting system {message.SystemId} ...");
                            break;

                        case Message.Types.Type.ProcDestroySystem:
                            System.ClearSystem();
                            Console.WriteLine($"Stopping ...");
                            break;

                        case Message.Types.Type.AppBroadcast:
                            var valueMessage = new Message{
                                ToAbstractionId = "app",
                                Type = Message.Types.Type.AppValue,
                                AppValue = new AppValue {
                                    Value = innerMessage.AppBroadcast.Value
                                }
                            };

                            var bebMessage = new Message{
                                Type = Message.Types.Type.BebBroadcast,
                                ToAbstractionId = "app.beb",
                                BebBroadcast = new BebBroadcast {
                                    Message = valueMessage
                                }
                            };

                            System.RegisterMessage(bebMessage);
                            break;

                        case Message.Types.Type.AppWrite:
                            var nnarWrite = new Message {
                                Type = Message.Types.Type.NnarWrite,
                                ToAbstractionId = $"app.nnar[{innerMessage.AppWrite.Register}]",
                                NnarWrite = new NnarWrite {
                                    Value = innerMessage.AppWrite.Value
                                }
                            };

                            System.RegisterMessage(nnarWrite);
                            break;

                        case Message.Types.Type.AppRead: {
                            var nnarRead = new Message {
                                Type = Message.Types.Type.NnarRead,
                                ToAbstractionId = $"app.nnar[{innerMessage.AppRead.Register}]",
                                NnarRead = new NnarRead()
                            };

                            System.RegisterMessage(nnarRead);
                            break;
                        }
                    }
                    break;
                }

                case Message.Types.Type.BebDeliver: {
                    var networkMessage = new Message {
                        SystemId = System.SystemId,
                        Type = Message.Types.Type.NetworkMessage,
                        NetworkMessage = new NetworkMessage {
                            SenderHost = System.SELF_HOST,
                            SenderListeningPort = System.SELF_PORT,
                            Message = message.BebDeliver.Message
                        }
                    };

                    System.NetworkManager.SendMessage(
                        networkMessage,
                        System.HUB_HOST,
                        System.HUB_PORT
                    );
                    break;
                }

                case Message.Types.Type.NnarReadReturn: {
                    var (_, register) = Util.SplitInstanceId(message.FromAbstractionId.Split('.').ToList().Last());

                    var networkMessage = new Message {
                        SystemId = System.SystemId,
                        Type = Message.Types.Type.NetworkMessage,
                        NetworkMessage = new NetworkMessage {
                            SenderHost = System.SELF_HOST,
                            SenderListeningPort = System.SELF_PORT,
                            Message = new Message {
                                Type = Message.Types.Type.AppReadReturn,
                                ToAbstractionId = AbstractionId,
                                AppReadReturn = new AppReadReturn {
                                    Register = register,
                                    Value = message.NnarReadReturn.Value
                                }
                            }
                        }
                    };

                    System.NetworkManager.SendMessage(
                        networkMessage,
                        System.HUB_HOST,
                        System.HUB_PORT
                    );
                    break;
                }

                case Message.Types.Type.NnarWriteReturn: {
                    var (_, register) = Util.SplitInstanceId(message.FromAbstractionId.Split('.').ToList().Last());

                    var networkMessage = new Message {
                        SystemId = System.SystemId,
                        Type = Message.Types.Type.NetworkMessage,
                        NetworkMessage = new NetworkMessage {
                            SenderHost = System.SELF_HOST,
                            SenderListeningPort = System.SELF_PORT,
                            Message = new Message {
                                Type = Message.Types.Type.AppWriteReturn,
                                ToAbstractionId = AbstractionId,
                                AppWriteReturn = new AppWriteReturn {
                                    Register = register
                                }
                            }
                        }
                    };

                    System.NetworkManager.SendMessage(
                        networkMessage,
                        System.HUB_HOST,
                        System.HUB_PORT
                    );
                    break;
                }
            }

            return true;
        }

    }
}
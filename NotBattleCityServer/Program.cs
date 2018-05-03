using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;
using NotBattleCity;

namespace NotBattleCityServer
{
    class Program
    {
        static NetServer server;
        static List<NetPeer> clients;
        static readonly int port = 14242;

        static void Main(string[] args)
        {
            var config = new NetPeerConfiguration("hej") { Port = port };
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            server = new NetServer(config);
            server.Start();

            if (server.Status == NetPeerStatus.Running)
            {
                Console.WriteLine("Server is running on port " + config.Port);
            }
            else
            {
                Console.WriteLine("Server not started...");
            }
            clients = new List<NetPeer>();
            var idDict = new Dictionary<IPEndPoint, long>();

            NetIncomingMessage message;
            var stop = false;

            while (!stop)
            {
                while ((message = server.ReadMessage()) != null)
                {
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryRequest:
                            {
                                NetOutgoingMessage msg = server.CreateMessage();
                                msg.Write("lala");
                                msg.Write(clients.Count);

                                server.SendDiscoveryResponse(msg, message.SenderEndPoint);
                                server.FlushSendQueue();
                            }
                            break;
                        case NetIncomingMessageType.Data:
                            {
                                var data = NetCommand.ReadCommand(message);
                                if (idDict[message.SenderEndPoint] == 0)
                                {
                                    idDict[message.SenderEndPoint] = data.ID;
                                }
                                var thingy = $"{message.SenderEndPoint.Address}:{message.SenderEndPoint.Port}:{idDict[message.SenderEndPoint]} : {data}";
                                Console.WriteLine(thingy);
                                NetOutgoingMessage m = NetCommand.WriteCommand(server, data);

                                server.SendToAll(m, NetDeliveryMethod.ReliableOrdered);
                                server.FlushSendQueue();

                                break;
                            }
                        case NetIncomingMessageType.DebugMessage:
                            Console.WriteLine(message.ReadString());
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            Console.WriteLine(message.SenderConnection.Status);
                            if (message.SenderConnection.Status == NetConnectionStatus.Connected)
                            {
                                clients.Add(message.SenderConnection.Peer);
                                idDict.Add(message.SenderEndPoint, 0);
                                Console.WriteLine("{0}:{1}:{2} has connected.", message.SenderEndPoint.Address, message.SenderEndPoint.Port, idDict[message.SenderEndPoint]);
                                NetOutgoingMessage querryPlayerPos = NetCommand.WriteCommand(server, -1, Command.QuerryPlayerPosition, 0, 0);
                                server.SendToAll(querryPlayerPos, NetDeliveryMethod.ReliableOrdered);
                                server.FlushSendQueue();
                            }
                            if (message.SenderConnection.Status == NetConnectionStatus.Disconnected)
                            {
                                var id = idDict[message.SenderEndPoint];
                                idDict.Remove(message.SenderEndPoint);
                                clients.Remove(message.SenderConnection.Peer);
                                Console.WriteLine("{0}:{1}:{2} has disconnected.", message.SenderEndPoint.Address, message.SenderEndPoint.Port, id);
                                NetOutgoingMessage querryPlayerPos = NetCommand.WriteCommand(server, -1, Command.DestroyPlayer, id);
                                server.SendToAll(querryPlayerPos, NetDeliveryMethod.ReliableOrdered);
                                server.FlushSendQueue();
                            }
                            break;
                        default:
                            Console.WriteLine("Unhandled message type: {message.MessageType}");
                            break;
                    }
                    server.Recycle(message);
                }
            }

            Console.ReadKey();
        }
    }
}
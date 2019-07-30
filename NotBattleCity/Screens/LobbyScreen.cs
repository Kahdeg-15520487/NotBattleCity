using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Utility;
using Utility.ScreenManager;
using Utility.UI;

namespace NotBattleCity.Screens
{
    class LobbyScreen : Screen
    {
        struct ServerInfo
        {
            public string Name { get; set; }
            public IPEndPoint IPEndPoint { get; set; }

            public ServerInfo(NetIncomingMessage msg)
            {
                IPEndPoint = msg.SenderEndPoint;
                Name = msg.ReadString();
            }
        }

        Canvas canvas;
        Label label_selectedServer;
        List<Button> buttonList;

        List<ServerInfo> serverList;
        ServerInfo _selectedServer;

        NetClient client;
        public static readonly int port = 14242;

        public LobbyScreen(GraphicsDevice device) : base(device, "LobbyScreen")
        { }

        public override bool Init()
        {
            var config = new NetPeerConfiguration("hej");
            config.AutoFlushSendQueue = false;
            client = new NetClient(config);
            client.Start();

            client.DiscoverLocalPeers(port);

            serverList = new List<ServerInfo>();
            buttonList = new List<Button>();
            InitUI();

            return base.Init();
        }

        public void InitUI()
        {
            canvas = new Canvas();

            label_selectedServer = new Label("", new Point(400, 50), new Vector2(120, 40), CONTENT_MANAGER.Fonts["default"])
            {
                Origin = Vector2.Zero,
                ForegroundColor = Color.White
            };

            Button button_refreshServerList = new Button("Refresh", new Point(400, 10), new Vector2(80, 30), CONTENT_MANAGER.Fonts["default"]);
            button_refreshServerList.Origin = new Vector2(10, 3);
            button_refreshServerList.MouseClick += (o, e) =>
            {
                client.DiscoverLocalPeers(port);
            };

            InputBox inputBox_ip = new InputBox("", new Point(400, 100), new Vector2(200, 30), CONTENT_MANAGER.Fonts["default"], Color.White, Color.Transparent);

            Button button_connect = new Button("Connect", new Point(500, 10), new Vector2(80, 30), CONTENT_MANAGER.Fonts["default"]);
            button_connect.Origin = new Vector2(10, 3);
            button_connect.MouseClick += (o, e) =>
            {

                if (string.IsNullOrEmpty(label_selectedServer.Text))
                {
                    if (!string.IsNullOrEmpty(inputBox_ip.Text))
                    {
                        IPAddress ipaddr = default;
                        if (IPAddress.TryParse(inputBox_ip.Text, out ipaddr))
                        {
                            _selectedServer = new ServerInfo()
                            {
                                Name = "unknown",
                                IPEndPoint = new IPEndPoint(ipaddr, port)
                            };
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                GameScreen.serverEndpoint = _selectedServer.IPEndPoint;
                SCREEN_MANAGER.GotoScreen("GameScreen");
            };


            canvas.AddElement("label_selectedServer", label_selectedServer);
            canvas.AddElement("button_refreshServerList", button_refreshServerList);
            canvas.AddElement("button_connect", button_connect);
            canvas.AddElement("inputBox_ip", inputBox_ip);
        }

        void AddServerToList(ServerInfo serverInfo)
        {
            Button bt = new Button(serverInfo.Name, new Point(10, 40 * buttonList.Count), new Vector2(120, 30), CONTENT_MANAGER.Fonts["default"])
            {
                Origin = new Vector2(10, 0),
                ForegroundColor = Color.Black,
                BorderColor = Color.Black,
                MetaData = serverInfo
            };

            bt.MouseClick += (o, e) =>
            {
                _selectedServer = (ServerInfo)((Button)o).MetaData;
                label_selectedServer.Text = _selectedServer.Name;
            };

            canvas.AddElement(serverInfo.Name, bt);
        }

        public override void Update(GameTime gameTime)
        {
            canvas.Update(gameTime, CONTENT_MANAGER.CurrentInputState, CONTENT_MANAGER.LastInputState);

            var inQueue = new List<NetIncomingMessage>();
            var msgCount = client.ReadMessages(inQueue);

            foreach (var msg in inQueue)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        var serverInfo = new ServerInfo(msg);
                        if (!serverList.Contains(serverInfo))
                        {
                            serverList.Add(serverInfo);
                            Console.WriteLine("Found server at " + serverInfo.IPEndPoint + " name: " + serverInfo.Name);
                            AddServerToList(serverInfo);
                        }
                        break;
                }
            }

            client.Recycle(inQueue);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

            CONTENT_MANAGER.BeginSpriteBatch(spriteBatch);
            canvas.Draw(spriteBatch, gameTime);
            CONTENT_MANAGER.EndSpriteBatch(spriteBatch);
        }
    }
}

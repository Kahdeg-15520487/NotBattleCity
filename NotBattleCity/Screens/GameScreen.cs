using Humper;
using Lidgren.Network;
using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.ScreenManager;
using Utility.UI;

namespace NotBattleCity.Screens
{
    class GameScreen : Screen
    {
        DrawBatch drawBatch;

        Canvas canvas;

        Dictionary<long, Player> Players;

        World world;

        Map map;
        public static readonly long ID = RandomIdGenerator.Base36ToDecimalSystem(RandomIdGenerator.GetBase36(5));

        public static NetClient client;
        public static List<NetIncomingMessage> inQueue;
        public static List<NetOutgoingMessage> outQueue;
        public static IPEndPoint serverEndpoint;
        public static readonly int port = 14242;

        bool isDrawDebug = false;

        public GameScreen(GraphicsDevice device) : base(device, "GameScreen")
        {
        }

        public override bool Init()
        {
            var config = new NetPeerConfiguration("hej");
            config.AutoFlushSendQueue = false;
            client = new NetClient(config);
            client.Start();

            client.Connect(serverEndpoint);

            //client.DiscoverLocalPeers(port);

            //string ip = "localhost";
            //int port = 14242;
            //client.Connect(ip, port);

            InitUI();
            drawBatch = new DrawBatch(device);
            world = new World(600, 400);
            Players = new Dictionary<long, Player>();

            map = MapLoader.Load("Content/map/map.csv", world);

            MapRenderer.spritesheet = CONTENT_MANAGER.SpriteSheets["terrain"];

            return base.Init();
        }

        public void InitUI()
        {
            canvas = new Canvas();
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }

        public override void Update(GameTime gameTime)
        {
            inQueue = new List<NetIncomingMessage>();
            var msgCount = client.ReadMessages(inQueue);

            outQueue = new List<NetOutgoingMessage>();

            if (HelperFunction.IsKeyDown(Keys.Escape))
            {
                client.Disconnect("Smell ya later");
                CONTENT_MANAGER.GameInstance.Exit();
            }
            if (HelperFunction.IsKeyDown(Keys.P))
            {
                isDrawDebug = !isDrawDebug;
            }

            if (!Players.ContainsKey(ID) && HelperFunction.IsKeyPress(Keys.R))
            {
                QueueCommand(ID, Command.CreatePlayer, 112f, 112f);
            }

            foreach (var player in Players.Values)
            {
                player.Update(gameTime);
            }

            foreach (var msg in inQueue)
            {
                switch (msg.MessageType)
                {
                    //case NetIncomingMessageType.DiscoveryResponse:
                    //    Console.WriteLine("Found server at " + msg.SenderEndPoint + " name: " + msg.ReadString());
                    //    client.Connect(msg.SenderEndPoint);
                    //    break;
                    case NetIncomingMessageType.Data:
                        {
                            NetCommand netcmd = NetCommand.ReadCommand(msg);

                            switch (netcmd.Command)
                            {
                                case Command.CreatePlayer:
                                    Console.WriteLine(netcmd);
                                    SpawnPlayer(netcmd);
                                    break;

                                case Command.DestroyPlayer:
                                    if (Players.ContainsKey(netcmd.LL))
                                    {
                                        world.Remove(Players[netcmd.LL].collision);
                                        Players.Remove(netcmd.LL);
                                    }
                                    break;

                                case Command.QuerryPlayerPosition:
                                    AnswerPosition();
                                    break;

                                case Command.ChangePlayerColor:
                                case Command.MovePlayer:
                                case Command.RotatePlayer:
                                case Command.StopPlayer:
                                case Command.CreateBullet:
                                case Command.MoveBullet:
                                case Command.DestroyBullet:
                                    Players[netcmd.ID].ExecuteCommand(netcmd);
                                    break;

                                case Command.SetTerrain:
                                    SetTerrain(netcmd);
                                    break;

                            }
                        }
                        break;
                }
            }

            foreach (var msg in outQueue)
            {
                client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
            }
            client.FlushSendQueue();

            client.Recycle(inQueue);

            canvas.Update(gameTime, CONTENT_MANAGER.CurrentInputState, CONTENT_MANAGER.LastInputState);
        }

        public static void QueueCommand(long iD, Command cmd, int x, int y)
        {
            outQueue.Add(NetCommand.WriteCommand(client, ID, cmd, x, y));
        }

        public static void QueueCommand(long iD, Command cmd, int x, int y, int z)
        {
            outQueue.Add(NetCommand.WriteCommand(client, ID, cmd, x, y, z));
        }

        public static void QueueCommand(long iD, Command cmd, float x, float y)
        {
            outQueue.Add(NetCommand.WriteCommand(client, ID, cmd, x, y));
        }

        public static void QueueCommand(long iD, Command cmd, long ll)
        {
            outQueue.Add(NetCommand.WriteCommand(client, ID, cmd, ll));
        }

        private void SetTerrain(NetCommand netcmd)
        {
            var t = (Terrain)netcmd.I3;
            var mapcell = map[netcmd.I1, netcmd.I2];
            mapcell.Terrain = t;
            switch (t)
            {
                case Terrain.Void:
                    //remove collision
                    world.Remove(mapcell.collision);
                    break;

                case Terrain.BrickRight:
                case Terrain.BrickDown:
                case Terrain.BrickLeft:
                case Terrain.BrickUp:
                    //modify collision;
                    world.Remove(mapcell.collision);

                    int x = mapcell.Coordinate.X * 16;
                    int y = mapcell.Coordinate.Y * 16;
                    int w = 16;
                    int h = 16;
                    switch (t)
                    {
                        case Terrain.BrickRight:
                            x += 8;
                            w = 8;
                            break;
                        case Terrain.BrickDown:
                            y += 8;
                            h = 8;
                            break;
                        case Terrain.BrickLeft:
                            w = 8;
                            break;
                        case Terrain.BrickUp:
                            h = 8;
                            break;
                    }
                    mapcell.collision = world.Create(x, y, w, h).AddTags(t.GetCollisionTag());
                    mapcell.collision.Data = mapcell;
                    map[netcmd.I1, netcmd.I2] = mapcell;
                    break;
            }
        }

        private void AnswerPosition()
        {
            if (Players.ContainsKey(ID))
            {
                var pos = Players[ID].Position;
                QueueCommand(ID, Command.CreatePlayer, pos.X, pos.Y);
                int color = 0;
                switch (Players[ID].pallete)
                {
                    case "yellow":
                        color = 0;
                        break;
                    case "silver":
                        color = 1;
                        break;
                    case "green":
                        color = 2;
                        break;
                    case "red":
                        color = 3;
                        break;
                }
                QueueCommand(ID, Command.ChangePlayerColor, color, 0);
                QueueCommand(ID, Command.RotatePlayer, (int)Players[ID].direction, 0);
                QueueCommand(ID, Command.StopPlayer, 0);
            }
        }

        private void SpawnPlayer(NetCommand cmd)
        {
            if (Players.ContainsKey(cmd.ID))
            {
                return;
            }

            var player = new Player(new Point((int)cmd.X, (int)cmd.Y), world, cmd.ID);
            Players.Add(player.ID, player);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            CONTENT_MANAGER.BeginSpriteBatch(spriteBatch);

            canvas.Draw(spriteBatch, gameTime);

            MapRenderer.Draw(spriteBatch, map);
            foreach (var player in Players.Values)
            {
                player.Draw(gameTime, spriteBatch);
            }
            CONTENT_MANAGER.EndSpriteBatch(spriteBatch);

            if (isDrawDebug)
            {
                drawBatch.Begin();
                world.DrawDebug(0, 0, 800, 600, DrawCell, DrawBox, DrawString);
                drawBatch.End();
            }
        }

        private void DrawBox(IBox box)
        {
            if (box.Width == 0 || box.Height == 0)
            {
                return;
            }

            DrawRect(box.X, box.Y, box.Width, box.Height);
        }

        private void DrawRect(float x, float y, float width, float height)
        {
            var p0 = new Vector2(x, y);
            var p1 = new Vector2(x + width, y);
            var p2 = new Vector2(x + width, y + height);
            var p3 = new Vector2(x, y + height);

            drawBatch.DrawLine(Pen.Red, p0, p1);
            drawBatch.DrawLine(Pen.Red, p1, p2);
            drawBatch.DrawLine(Pen.Red, p2, p3);
            drawBatch.DrawLine(Pen.Red, p3, p0);
        }

        private void DrawCell(int x, int y, int w, int h, float alpha)
        {
            return;
        }

        private void DrawString(string message, int x, int y, float alpha)
        {
            return;
        }
    }
}


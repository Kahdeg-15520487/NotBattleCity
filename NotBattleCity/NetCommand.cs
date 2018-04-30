using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using NotBattleCity.Screens;

namespace NotBattleCity
{
    enum Command
    {
        CreatePlayer,
        ChangePlayerColor,
        MovePlayer,
        StopPlayer,
        RotatePlayer,
        DestroyPlayer,
        QuerryPlayerPosition,
        CreateBullet,
        MoveBullet,
        DestroyBullet,
        DestroyBrick,
        Disconnect
    }

    [StructLayout(LayoutKind.Explicit)]
    struct NetCommand
    {
        [FieldOffset(0)]
        public long ID;
        [FieldOffset(8)]
        public Command Command;
        [FieldOffset(8)]
        public int CommandI;
        [FieldOffset(12)]
        public float X;
        [FieldOffset(16)]
        public float Y;
        [FieldOffset(12)]
        public long LL;
        [FieldOffset(12)]
        public int I1;
        [FieldOffset(16)]
        public int I2;

        public static NetOutgoingMessage WriteCommand(NetCommand netcmd)
        {
            NetOutgoingMessage msg = GameScreen.client.CreateMessage();
            msg.Write(netcmd.ID);
            msg.Write(netcmd.CommandI);
            msg.Write(netcmd.LL);
            return msg;
        }

        public static NetOutgoingMessage WriteCommand(long id, Command cmd, float x, float y)
        {
            NetOutgoingMessage msg = GameScreen.client.CreateMessage();
            msg.Write(id);
            msg.Write((int)cmd);
            msg.Write(x);
            msg.Write(y);
            return msg;
        }

        public static NetOutgoingMessage WriteCommand(long id, Command cmd, int x, int y)
        {
            NetOutgoingMessage msg = GameScreen.client.CreateMessage();
            msg.Write(id);
            msg.Write((int)cmd);
            msg.Write(x);
            msg.Write(y);
            return msg;
        }

        public static NetOutgoingMessage WriteCommand(long id, Command cmd, long ll)
        {
            NetOutgoingMessage msg = GameScreen.client.CreateMessage();
            msg.Write(id);
            msg.Write((int)cmd);
            msg.Write(ll);
            return msg;
        }

        public static NetCommand ReadCommand(NetIncomingMessage msg)
        {
            return new NetCommand()
            {
                ID = msg.ReadInt64(),
                Command = (Command)msg.ReadInt32(),
                LL = msg.ReadInt64()
            };
        }

        public override string ToString()
        {
            switch (Command)
            {
                case Command.CreatePlayer:
                    return $"{ID} {Command} {X} {Y}";

                case Command.ChangePlayerColor:
                    return $"{ID} {Command} {I1}";

                case Command.MovePlayer:
                    return $"{ID} {Command} {X} {Y}";

                case Command.RotatePlayer:
                    return $"{ID} {Command} {I1}";

                case Command.DestroyPlayer:
                    return $"{ID} {Command} {LL}";

                case Command.CreateBullet:
                    return $"{ID} {Command} {X} {Y}";

                case Command.MoveBullet:
                    return $"{ID} {Command} {X} {Y}";

                case Command.DestroyBullet:
                    return $"{ID} {Command} {X} {Y}";

                default:
                    return $"{ID} {Command}";
            }
        }
    }
}

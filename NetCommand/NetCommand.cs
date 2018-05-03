using Lidgren.Network;
using System.Runtime.InteropServices;

namespace NotBattleCity
{
    public enum Command
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
        SetTerrain,
        Disconnect
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct NetCommand
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
        [FieldOffset(20)]
        public int I3;

        public static NetOutgoingMessage WriteCommand(NetPeer server, NetCommand netcmd)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write(netcmd.ID);
            msg.Write((int)netcmd.Command);
            msg.Write(netcmd.LL);
            msg.Write(netcmd.I3);
            return msg;
        }

        public static NetOutgoingMessage WriteCommand(NetPeer server, long id, Command cmd, float x, float y)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write(id);
            msg.Write((int)cmd);
            msg.Write(x);
            msg.Write(y);
            return msg;
        }

        public static NetOutgoingMessage WriteCommand(NetPeer server, long id, Command cmd, int x, int y)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write(id);
            msg.Write((int)cmd);
            msg.Write(x);
            msg.Write(y);
            return msg;
        }

        public static NetOutgoingMessage WriteCommand(NetPeer server, long id, Command cmd, int x, int y, int z)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write(id);
            msg.Write((int)cmd);
            msg.Write(x);
            msg.Write(y);
            msg.Write(z);
            return msg;
        }

        public static NetOutgoingMessage WriteCommand(NetPeer server, long id, Command cmd, long ll)
        {
            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write(id);
            msg.Write((int)cmd);
            msg.Write(ll);
            return msg;
        }

        public static NetCommand ReadCommand(NetIncomingMessage msg)
        {
            var ID = msg.ReadInt64();
            var Command = (Command)msg.ReadInt32();
            var LL = msg.ReadInt64();
            var I3 = Command == Command.SetTerrain ? msg.ReadInt32() : -1;

            return new NetCommand()
            {
                ID = ID,
                Command = Command,
                LL = LL,
                I3 = I3
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

                case Command.SetTerrain:
                    return $"{ID} {Command} {I1} {I2} {I3}";

                default:
                    return $"{ID} {Command}";
            }
        }
    }
}

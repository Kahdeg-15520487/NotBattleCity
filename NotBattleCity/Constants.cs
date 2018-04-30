using System.Net;

namespace NotBattleCity
{
    class NetInfo
    {
        public static readonly IPAddress IP = IPAddress.Parse("192.168.56.1");
        public static readonly int Port = 9999;
    }

    enum Terrain
    {
        Void = -1,
        Brick,
        BrickRight,
        BrickDown,
        BrickLeft,
        BrickUp,
        MetalBrick,
        Tree,
        Concrete,
        Water1,
        Water2,
        Water3
    }

    enum CollisionTag
    {
        Ignore = 1 << 0,
        Brick = 1 << 1,
        MetalBrick = 1 << 2,
        Concrete = 1 << 3,
        Water = 1 << 4,
        Player = 1 << 5,
        Bullet = 1 << 6
    }
}
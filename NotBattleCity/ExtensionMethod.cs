
using Utility;
using VT2 = Microsoft.Xna.Framework.Vector2;

namespace NotBattleCity
{
    static class ExtensionMethod
    {
        public static Humper.Base.Vector2 ToHumperVector2(this VT2 vector2)
        {
            return new Humper.Base.Vector2(vector2.X, vector2.Y);
        }

        public static VT2 ToMonogameVector2(this Humper.Base.Vector2 vector2)
        {
            return new VT2(vector2.X, vector2.Y);
        }

        public static CollisionTag GetCollisionTag(this Terrain terrain)
        {
            switch (terrain)
            {
                case Terrain.Brick:
                case Terrain.BrickRight:
                case Terrain.BrickDown:
                case Terrain.BrickLeft:
                case Terrain.BrickUp:
                    return CollisionTag.Brick;

                case Terrain.MetalBrick:
                    return CollisionTag.MetalBrick;

                case Terrain.Concrete:
                    return CollisionTag.Concrete;

                case Terrain.Water1:
                case Terrain.Water2:
                case Terrain.Water3:
                    return CollisionTag.Water;

                default:
                    return CollisionTag.Ignore;
            }
        }

        public static float GetLayer(this Terrain terrain)
        {
            if (terrain == Terrain.Tree)
            {
                return LayerDepth.TerrainUpper;
            }
            return LayerDepth.TerrainBase;
        }
    }
}

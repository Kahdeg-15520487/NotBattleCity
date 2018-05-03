using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utility;

namespace NotBattleCity
{
    class MapRenderer
    {
        public static SpriteSheetMap spritesheet;

        public static void Draw(SpriteBatch spriteBatch, Map map)
        {
            for (int i = 0; i < map.Height; i++)
            {
                for (int j = 0; j < map.Width; j++)
                {
                    var terrain = map[j, i].Terrain;
                    if (terrain == Terrain.Void)
                    {
                        continue;
                    }
                    var rect = spritesheet.SpriteRect[(int)terrain];
                    var pos = map[j, i].Coordinate.ToVector2() * 16;
                    spriteBatch.Draw(spritesheet.SpriteSheet, pos, rect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, terrain.GetLayer());
                }
            }
        }
    }
}

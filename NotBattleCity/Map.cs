using Humper;

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NotBattleCity
{

    class Map
    {
        MapCell[] _cells;
        int _width;
        int _height;

        public int Width { get => _width; }
        public int Height { get => _height; }

        public MapCell this[int x, int y]
        {
            get => _cells[y * _width + x];
        }

        public Map(int width, int height)
        {
            _cells = new MapCell[width * height];
            _width = width;
            _height = height;
        }

        public void SetData(Terrain[] terrains, World world)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _height; i++)
            {
                List<string> ss = new List<string>();
                for (int j = 0; j < _width; j++)
                {
                    _cells[i * _width + j] = MapCell.GetMapCell(j, i, terrains[i * _width + j], world);
                    ss.Add(_cells[i * Width + j].ToString());
                }
                sb.AppendLine(string.Join("\t\t", ss));
            }
            File.WriteAllText("map.txt", sb.ToString());
        }

    }

    class MapCell
    {
        public Microsoft.Xna.Framework.Point Coordinate { get; private set; }
        public Humper.Base.Vector2 Position { get => collision.Bounds.Location; }
        public IBox collision;

        public Terrain Terrain;

        private MapCell(Microsoft.Xna.Framework.Point coord) { Coordinate = coord; }
        private MapCell(int x, int y) { Coordinate = new Microsoft.Xna.Framework.Point(x, y); }

        public static MapCell GetMapCell(int x, int y, Terrain terrain, World world)
        {
            MapCell result = new MapCell(x, y)
            {
                Terrain = terrain
            };
            if (terrain.GetCollisionTag() != CollisionTag.Ignore)
            {
                result.collision = world.Create(x * 16, y * 16, 16, 16).AddTags(terrain.GetCollisionTag());
            }
            else
            {
                result.collision = world.Create(x * 16, y * 16, 0, 0).AddTags(CollisionTag.Ignore);
            }
            result.collision.Data = result;

            return result;
        }

        public override string ToString()
        {
            return Terrain.ToString();
        }
    }
}

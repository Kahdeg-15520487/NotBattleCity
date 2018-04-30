using Humper;
using System;

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
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < _height; i++)
            {
                System.Collections.Generic.List<string> ss = new System.Collections.Generic.List<string>();
                for (int j = 0; j < _width; j++)
                {
                    _cells[i * _width + j] = MapCell.GetMapCell(j * 16, i * 16, terrains[i * _width + j], world);
                    ss.Add(_cells[i * Width + j].ToString());
                }
                sb.AppendLine(string.Join("\t\t", ss));
            }
            System.IO.File.WriteAllText("map.txt", sb.ToString());
        }

    }

    class MapCell
    {
        public Humper.Base.Vector2 Position { get => collision.Bounds.Location; }
        IBox collision;

        public event EventHandler<object> Hit;

        public Terrain Terrain;

        private MapCell() { }

        public static MapCell GetMapCell(float x, float y, Terrain terrain, World world)
        {
            MapCell result = new MapCell();

            result.Terrain = terrain;
            if (terrain.GetCollisionTag() != CollisionTag.Ignore)
            {
                result.collision = world.Create(x, y, 16, 16).AddTags(terrain.GetCollisionTag());
            }
            else
            {
                result.collision = world.Create(x, y, 0, 0).AddTags(CollisionTag.Ignore);
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

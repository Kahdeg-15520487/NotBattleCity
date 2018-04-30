using Humper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace NotBattleCity
{
    class MapLoader
    {
        public static Map Load(string path,World world)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            var data = HelperFunction.Make1DArray(File.ReadAllLines(path).Select(l => l.Split(',').Select(c => (Terrain)int.Parse(c)).ToArray()).ToArray());

            var map = new Map(30, 20);
            map.SetData(data,world);

            /* 0,0,0,1,1,1,1,0,0
             * 0,0,0,1,1,1,1,0,0
             * 0,0,0,1,1,1,1,0,0
             * 0,0,0,1,1,1,1,0,0
             */

            return map;
        }
    }
}

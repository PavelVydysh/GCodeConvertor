using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor
{
    [Serializable]
    public class Topology
    {
        public string name { get; set; }
        public string path { get; set; }
        public float accuracy { get; set; }
        public int[,] map { get; set; }

        public Topology() { }

        public Topology(TopologyModel model)
        {
            this.name = model.NameProject;
            this.path = model.PathProject;
            this.accuracy = model.Accuracy;
            int[,] map = new int[(int)((model.PlatformH + model.HeadIdentationY * 2) / accuracy), (int)((model.PlatformW + model.HeadIdentationX * 2) / accuracy)];


            map[0, 0] = 2;
            map[map.GetUpperBound(0), 0] = 2;
            map[0, map.GetUpperBound(1)] = 2;
            map[map.GetUpperBound(0), map.GetUpperBound(1)] = 2;

            for (int i = 0; i < (int)(model.PlatformH / accuracy); i++)
            {
                for (int j = 0; j < (int)(model.PlatformW / accuracy); j++)
                {
                    map[i + (int)(model.HeadIdentationY / accuracy), j + (int)(model.HeadIdentationX / accuracy)] = 1;
                }
            }

            int needlesDots = (int)(2 / accuracy);

            for (int j = (int)(model.StartNeedleOffsetY / accuracy) + (int)(model.HeadIdentationY / accuracy); j < map.GetUpperBound(0) + 1 - (int)(model.StartNeedleOffsetY / accuracy) - (int)(model.HeadIdentationY / accuracy); j += 1 + (int)(model.StepNeedlesY / accuracy))
            {
                for (int j1 = 0; j1 < needlesDots; j1++)
                {

                    for (int i = (int)(model.StartNeedleOffsetX / accuracy) + (int)(model.HeadIdentationX / accuracy); i < map.GetUpperBound(1) + 1 - (int)(model.StartNeedleOffsetX / accuracy) - (int)(model.HeadIdentationX / accuracy); i += 1 + (int)(model.StepNeedlesX / accuracy))
                    {
                        for (int i1 = 0; i1 < needlesDots; i1++)
                        {
                            map[i, j] = 3;
                            i++;
                        }
                        i--;
                    }
                    j++;
                }
                j--;
            }

            this.map = map;
        }

        public Topology(string name, string path, float accuracy,
            int PlatformH, int PlatformW,
            int HeadIdentationX, int HeadIdentationY,
            int StartNeedleOffsetX, int StartNeedleOffsetY, int StepNeedlesX, int StepNeedlesY)
        {
            this.name = name;
            this.path = path;
            this.accuracy = accuracy;
            int[,] map = new int[(int)((PlatformH + HeadIdentationY * 2) / accuracy), (int)((PlatformW + HeadIdentationX * 2) / accuracy)];


            map[0, 0] = 2;
            map[map.GetUpperBound(0), 0] = 2;
            map[0, map.GetUpperBound(1)] = 2;
            map[map.GetUpperBound(0), map.GetUpperBound(1)] = 2;

            for (int i = 0; i < (int)(PlatformH / accuracy); i++)
            {
                for (int j = 0; j < (int)(PlatformW / accuracy); j++)
                {
                    map[i + (int)(HeadIdentationY / accuracy), j + (int)(HeadIdentationX / accuracy)] = 1;
                }
            }

            int needlesDots = (int)(2 / accuracy);

            for (int j = (int)(StartNeedleOffsetY / accuracy) + (int)(HeadIdentationY / accuracy); j < map.GetUpperBound(0) + 1 - (int)(StartNeedleOffsetY / accuracy) - (int)(HeadIdentationY / accuracy); j += 1 + (int)(StepNeedlesY / accuracy))
            {
                for (int j1 = 0; j1 < needlesDots; j1++)
                {

                    for (int i = (int)(StartNeedleOffsetX / accuracy) + (int)(HeadIdentationX / accuracy); i < map.GetUpperBound(1) + 1 - (int)(StartNeedleOffsetX / accuracy) - (int)(HeadIdentationX / accuracy); i += 1 + (int)(StepNeedlesX / accuracy))
                    {
                        for (int i1 = 0; i1 < needlesDots; i1++)
                        {
                            map[i, j] = 3;
                            i++;
                        }
                        i--;
                    }
                    j++;
                }
                j--;
            }

            this.map = map;

        }

        


    }
}

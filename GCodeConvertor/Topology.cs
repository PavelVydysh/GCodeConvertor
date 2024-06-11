using System;
using System.Collections.Generic;
using System.IO;
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

        public Topology(TopologyModel model)
        {
            this.name = model.NameProject;
            this.path = model.PathProject;
            this.accuracy = model.Accuracy;
            this.map = generate(model);
            setupNeedles(model);
        }

        public Topology() { }

        public string getFullPath()
        {
            return Path.Combine(path, name) + ".gcd";
        }

        private int[,] generate(TopologyModel model)
        {
            int mapWidth = (int)((model.PlatformW + model.HeadIdentationX * 2) / accuracy);
            int mapHeight = (int)((model.PlatformH + model.HeadIdentationY * 2) / accuracy);

            int[,] map = new int[mapWidth, mapHeight];

            map[0, 0] = 2;
            map[map.GetUpperBound(0), 0] = 2;
            map[0, map.GetUpperBound(1)] = 2;
            map[map.GetUpperBound(0), map.GetUpperBound(1)] = 2;

            for (int workspaceX = (int)(model.HeadIdentationX / accuracy);
                workspaceX < (mapWidth - (int)(model.HeadIdentationX / accuracy));
                workspaceX++)
            {
                for (int workspaceY = (int)(model.HeadIdentationY / accuracy);
                workspaceY < (mapHeight - (int)(model.HeadIdentationY / accuracy));
                workspaceY++)
                {
                    map[workspaceX, workspaceY] = 1; // 1 - идентиф. платформы
                }
            }
            return map;
        }

        private void setupNeedles(TopologyModel model)
        {
            for (int expectedNeedlePositionX = 0;
                expectedNeedlePositionX < (int)((model.PlatformW - model.StartNeedleOffsetX) / accuracy);
                expectedNeedlePositionX++)
            {
                for (int expectedNeedlePositionY = 0;
                    expectedNeedlePositionY < (int)((model.PlatformH - model.StartNeedleOffsetY) / accuracy);
                    expectedNeedlePositionY++)
                {

                    if (expectedNeedlePositionX % ((model.NeedleDiameter + model.StepNeedlesX) / accuracy) < model.NeedleDiameter / accuracy &&
                       expectedNeedlePositionY % ((model.NeedleDiameter + model.StepNeedlesY) / accuracy) < model.NeedleDiameter / accuracy &&
                       expectedNeedlePositionX + model.NeedleDiameter / accuracy - expectedNeedlePositionX % ((model.NeedleDiameter + model.StepNeedlesX) / accuracy) <= (int)((model.PlatformW - model.StartNeedleOffsetX) / accuracy) &&
                       expectedNeedlePositionY + model.NeedleDiameter / accuracy - expectedNeedlePositionY % ((model.NeedleDiameter + model.StepNeedlesY) / accuracy) <= (int)((model.PlatformH - model.StartNeedleOffsetY) / accuracy))
                    {
                        map[expectedNeedlePositionX + (int)(model.HeadIdentationX / accuracy + model.StartNeedleOffsetX / accuracy),
                            expectedNeedlePositionY + (int)(model.HeadIdentationY / accuracy + model.StartNeedleOffsetY / accuracy)] = 3;
                    }
                }
            }
        }
    }
}

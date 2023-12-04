using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor
{
    public class TopologyModel : IDataErrorInfo
    {
        public string NameProject { get; set; }
        public string PathProject { get; set; }
        public int PlatformH { get; set; }
        public int PlatformW { get; set; }
        public int HeadIdentationX { get; set; }
        public int HeadIdentationY { get; set; }
        public int NozzleDiameter { get; set; }
        public int StartNeedleOffsetX { get; set; }
        public int StartNeedleOffsetY { get; set; }
        public int StepNeedlesX { get; set; }
        public int StepNeedlesY { get; set; }

        public string error = String.Empty;
        public string this[string columnName]
        {
            get
            {
                error = String.Empty;
                switch (columnName)
                {
                    case "NameProject":
                        if (String.IsNullOrWhiteSpace(NameProject))
                        {
                            error = "Название проекта не может быть пустым.";
                        }
                        break;
                }
                return error;
            }
        }

        public String Error
        {
            get { throw new NotImplementedException(); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor
{
    internal class CustomItem
    {
        public string LabelContent { get; set; }
        public string TextBoxContent { get; set; }

        public CustomItem(string label, string textbox)
        {
            this.LabelContent = label;
            this.TextBoxContent = textbox;
        }

        public bool Equals(CustomItem i)
        {
            return LabelContent == i.LabelContent;
        }
    }
}

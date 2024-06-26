using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor.GScript
{

    internal class CommandReader
    {

        private GScriptWindow window;

        public CommandReader(GScriptWindow window)
        {
            this.window = window;
        }

        public string[] readCommands() 
        {
            string textBoxString = window.textBox.Text.Trim().Replace(" ", "").Replace("\n", "").Replace("\r", "");
            return textBoxString.Split(";");
        }
    }
}

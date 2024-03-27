using GCodeConvertor.WorkspaceInstruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GCodeConvertor
{
    public class Hotkey
    {
        public List<Key> keys {  get; }

        public WorkspaceInstrument workspaceInstrument { get; }

        public WorkspaceDrawingControl workspaceDrawingControl { get; }

        public Hotkey(
            List<Key> keys, 
            WorkspaceInstrument workspaceInstrument,
            WorkspaceDrawingControl workspaceDrawingControl) 
        {
            this.keys = keys;
            this.workspaceInstrument = workspaceInstrument;
            this.workspaceDrawingControl = workspaceDrawingControl;
        }

        public void selectInstrument(List<Key> pressedKeys)
        {
            if (pressedKeys.All(keys.Contains) && keys.All(pressedKeys.Contains))
            {
                workspaceDrawingControl.workspaceIntrument = workspaceInstrument;
            }
        }


    }
}

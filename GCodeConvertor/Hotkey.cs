using GCodeConvertor.ProjectForm;
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

        public InstrumentButtonInfo workspaceInstrument { get; }

        public ProjectWindow projectWindow { get; }

        public Hotkey(
            List<Key> keys,
            InstrumentButtonInfo workspaceInstrument,
            ProjectWindow projectWindow) 
        {
            this.keys = keys;
            this.workspaceInstrument = workspaceInstrument;
            this.projectWindow = projectWindow;
        }

        public void selectInstrument(List<Key> pressedKeys)
        {
            if (pressedKeys.All(keys.Contains) && keys.All(pressedKeys.Contains))
            {
                int indexOfInstrument = projectWindow.workspaceInstruments.IndexOf(workspaceInstrument);
                if(indexOfInstrument != -1)
                {
                    projectWindow.setSelectedInstrument(indexOfInstrument);
                }
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GCodeConvertor.WorkspaceInstruments
{
    public abstract class WorkspaceInstrument
    {
        public WorkspaceDrawingControl workspaceDrawingControl { get; }

        public WorkspaceInstrument(WorkspaceDrawingControl workspaceDrawingControl)
        {
            this.workspaceDrawingControl = workspaceDrawingControl;
        }

        public abstract void execute(EventType eventType, object sender, EventArgs e);
    }
}

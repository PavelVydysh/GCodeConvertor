using GCodeConvertor.WorkspaceInstruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace GCodeConvertor.ProjectForm.WorkspaceInstruments
{
    public class NeedleManagingWorkspaceInstrument : WorkspaceInstrument
    {
        public NeedleManagingWorkspaceInstrument(WorkspaceDrawingControl workspaceDrawingControl) : base(workspaceDrawingControl) { }

        public override void execute(EventType eventType, object sender, EventArgs e)
        {
            if (eventType == EventType.LeftMouseButtonDown && sender is Rectangle)
                lmbDownOnCell((Rectangle)sender, (MouseButtonEventArgs)e);
        }

        private void lmbDownOnCell(Rectangle sender, MouseButtonEventArgs e)
        {
            int currentTopologyX = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).X / workspaceDrawingControl.cellSize);
            int currentTopologyY = (int)Math.Floor(e.GetPosition(workspaceDrawingControl.workspaceCanvas).Y / workspaceDrawingControl.cellSize);

            int cellType = workspaceDrawingControl.topology.map[currentTopologyX, currentTopologyY];

            switch (cellType)
            {
                case 3:
                    workspaceDrawingControl.topology.map[currentTopologyX, currentTopologyY] = 1;
                    break;
                case 1:
                    workspaceDrawingControl.topology.map[currentTopologyX, currentTopologyY] = 3;
                    break;
                default:
                    return;
            }

            workspaceDrawingControl.repaint();
        }
    }
}

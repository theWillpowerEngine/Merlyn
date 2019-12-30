using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShIDE.Controls
{
    public class DraggableTabControl : TabControl
    {
        private TabPage predraggedTab;

        public bool Dragging = false;

        public DraggableTabControl()
        {
            AllowDrop = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            predraggedTab = getPointedTab();
            //OnSelectedIndexChanged(e);

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            predraggedTab = null;

            Dragging = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && predraggedTab != null)
            {
                //Dragging = true;
                DoDragDrop(predraggedTab, DragDropEffects.Move);
            }

            base.OnMouseMove(e);
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            TabPage draggedTab = (TabPage)drgevent.Data.GetData(typeof(TabPage));
            TabPage pointedTab = getPointedTab();

            if (draggedTab == predraggedTab && pointedTab != null)
            {
                drgevent.Effect = DragDropEffects.Move;

                if (pointedTab != draggedTab)
                    swapTabPages(draggedTab, pointedTab);
            }

            base.OnDragOver(drgevent);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            Dragging = false;
            base.OnDragDrop(e);
        }

        private TabPage getPointedTab()
        {
            for (int i = 0; i < TabPages.Count; i++)
                if (GetTabRect(i).Contains(PointToClient(Cursor.Position)))
                    return TabPages[i];

            return null;
        }

        private void swapTabPages(TabPage src, TabPage dst)
        {
            int srci = TabPages.IndexOf(src);
            int dsti = TabPages.IndexOf(dst);

            TabPages[dsti] = src;
            TabPages[srci] = dst;

            if (SelectedIndex == srci)
                SelectedIndex = dsti;
            else if (SelectedIndex == dsti)
                SelectedIndex = srci;

            Refresh();
        }
    }
}

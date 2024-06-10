using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Point = System.Windows.Point;

namespace GCodeConvertor
{
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(Point))]
    public class Layer
    {
        public const string DEFAULT_NAME = "Без имени";
        public const float DEFAULT_HEIGHT = 12;

        public Guid guid { get; }
        public string name { get; set; }
        public float height { get; set; }
        public List<Point> thread { get; set; }
        public bool isEnable { get; set; }

        [XmlIgnore]
        public List<Point> selectedThread { get; set; }
        [XmlIgnore]
        public Stack<List<Point>> historyBack { get; set; }
        [XmlIgnore]
        public Stack<List<Point>> historyForward { get; set; }

        public Layer(string name, float height)
        {
            guid = Guid.NewGuid();
            this.name = name;
            this.height = height;
            thread = new List<Point>();
            selectedThread = new List<Point>();
            historyBack = new Stack<List<Point>>();
            historyForward = new Stack<List<Point>>();
            isEnable = true;
        }

        public Layer() : this(DEFAULT_NAME, DEFAULT_HEIGHT) { }
        
        public void backHistory()
        {
            if(historyBack.Count > 0)
            {
                selectedThread.Clear();
                historyForward.Push(new List<Point>(thread));
                thread = historyBack.Pop();
            }
        }

        public void forwardHistory()
        {
            if(historyForward.Count > 0)
            {
                selectedThread.Clear();
                historyBack.Push(new List<Point>(thread));
                thread = historyForward.Pop();
            }

        }
        private void changeHistory()
        {
            historyForward.Clear();
            historyBack.Push(new List<Point>(thread));
        }

        public void addThreadPoint(Point point)
        {
            if (!isPointCorrect(point))
                return;
            changeHistory();
            thread.Add(point);
        }

        public void addAllThreadPoints(List<Point> points)
        {
            List<Point> addingPoints = new List<Point>();
            foreach (Point point in points)
            {
                if (!isPointCorrect(point))
                    return;
                addingPoints.Add(point);
            }
            changeHistory();
            thread.AddRange(addingPoints);

        }

        private bool isPointCorrect(Point point)
        {
            if (point.X < 0 || point.X > GlobalPreset.topologyWidth)
                return false;
            if (point.Y < 0 || point.Y > GlobalPreset.topologyHeight)
                return false;
            return true;
        }

        public void removeThreadPoint(Point point) 
        {
            changeHistory();
            thread.Remove(point); 
        }

        public void insertBeforePositionThreadPoint(Point point, int index)
        {
            changeHistory();
            thread.Insert(index, point);
        }

        public void removeAllSelectedThreadPoint()
        {
            changeHistory();
            foreach (Point point in selectedThread)
            {
                thread.RemoveAll(item => item.Equals(point));
            }
            selectedThread.Clear();
        }
        public void changeThreadPoint(Point newPoint, int position)
        {
            if (!isPointCorrect(newPoint))
                return;
            changeHistory();
            thread[position] = newPoint;
        }

        public void removeRangeThreadPoint(int index, int count)
        {
            changeHistory();
            thread.RemoveRange(index, count);
        }

        public bool isDotSelected(Point point)
        {
            return selectedThread.Contains(point);
        }

        public bool isEnded()
        {
            if (thread.Count < 2)
                return false;
            return thread.First().Equals(thread.Last());
        }

        public bool isStarted()
        {
            if (thread.Count == 0)
                return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is not Layer)
            {
                return false;
            }
            return (this.guid.Equals(((Layer)obj).guid));
        }

        public int getThreadPoint(Point point)
        {
            for(int i = 1; i < thread.Count; i++)
            {
                if (thread[i].Equals(point)) return i;
            }
            return -1;
        }

        public int getSelectedThreadPoint(Point point)
        {
            for (int i = 1; i < selectedThread.Count; i++)
            {
                if (selectedThread[i].Equals(point)) return i;
            }
            return -1;
        }

    }
}
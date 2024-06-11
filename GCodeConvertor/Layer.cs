using Polenter.Serialization;
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
    [XmlInclude(typeof(Point))]
    [XmlInclude(typeof(Guid))]
    public class Layer
    {
        public const string DEFAULT_NAME = "Без имени";
        public const float DEFAULT_HEIGHT = 12;

        public Guid guid { get; set; }
        public string name { get; set; }
        public float height { get; set; }
        public List<Point> thread { get; set; }
        public bool isEnable { get; set; }

        [ExcludeFromSerialization]
        public List<Point> selectedThread { get; set; }
        [ExcludeFromSerialization]
        public Stack<List<Point>> historyBack { get; set; }
        [ExcludeFromSerialization]
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
            if (!ProjectSettings.preset.isPointTopologyCorrect(new Point(getTopologyValueByThreadValue(point.X), getTopologyValueByThreadValue(point.Y))))
                return;
            changeHistory();
            thread.Add(point);
        }

        public void addAllThreadPoints(List<Point> points)
        {
            List<Point> addingPoints = new List<Point>();
            foreach (Point point in points)
            {
                if (!ProjectSettings.preset.isPointTopologyCorrect(new Point(getTopologyValueByThreadValue(point.X), getTopologyValueByThreadValue(point.Y))))
                    return;
                addingPoints.Add(point);
            }
            changeHistory();
            thread.AddRange(addingPoints);
        }

        private double getTopologyValueByThreadValue(double threadValue)
        {
            return threadValue - 0.5;
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
        public bool changeThreadPoint(Point newPoint, int position)
        {
            if (!ProjectSettings.preset.isPointTopologyCorrect(new Point(getTopologyValueByThreadValue(newPoint.X), getTopologyValueByThreadValue(newPoint.Y))))
                return false;
            changeHistory();
            thread[position] = newPoint;
            return true;
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
            return (guid.Equals(((Layer)obj).guid) && thread.SequenceEqual(((Layer)obj).thread));
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
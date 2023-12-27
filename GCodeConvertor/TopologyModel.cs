using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor
{
    public class TopologyModel : IDataErrorInfo, INotifyPropertyChanged
    {
        public string NameProject { get; set; }

        private string _PathProject;
        public string PathProject 
        {
            get { return _PathProject; }
            set { _PathProject = value; OnPropertyChanged("PathProject"); }
        }
        public int PlatformH { get; set; }
        public int PlatformW { get; set; }
        public int HeadIdentationX { get; set; }
        public int HeadIdentationY { get; set; }
        public int NozzleDiameter { get; set; }
        public int StartNeedleOffsetX { get; set; }
        public int StartNeedleOffsetY { get; set; }
        public int StepNeedlesX { get; set; }
        public int StepNeedlesY { get; set; }
        public float Accuracy { get; set; }

        public string error = String.Empty;

        public Dictionary<String, String> Errors = new Dictionary<string, string>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                error = string.Empty; 

                switch (columnName)
                {
                    case "NameProject":
                        if (String.IsNullOrWhiteSpace(NameProject))
                        {
                            error = "Название проекта не может быть пустым.";
                        }
                        break;

                    case "PathProject":
                        if (String.IsNullOrWhiteSpace(PathProject))
                        {
                            error = "Путь к проекту не может быть пустым.";
                        }
                        break;

                    case "PlatformH":
                        if (PlatformH <= 0)
                        {
                            error = "Высота платформы не может быть меньше или равна 0";
                        }
                        break;
                    case "PlatformW":
                        if (PlatformW <= 0)
                        {
                            error = "Ширина платформы не может быть меньше или равна 0";
                        }
                        break;
                    case "HeadIdentationX":
                        if (HeadIdentationX <= 0)
                        {
                            error = "X-координата положения головы не может быть меньше или равна 0";
                        }
                        break;
                    case "HeadIdentationY":
                        if (HeadIdentationY <= 0)
                        {
                            error = "Y-координата положения головы не может быть меньше или равна 0";
                        }
                        break;
                    case "NozzleDiameter":
                        if (NozzleDiameter <= 0)
                        {
                            error = "Диаметр сопла не может быть меньше или равен 0";
                        }
                        break;
                    case "StartNeedleOffsetX":
                        if (StartNeedleOffsetX <= 0)
                        {
                            error = "Координата X отступа не может быть меньше или равна 0 ";
                        }
                        break;
                    case "StartNeedleOffsetY":
                        if (StartNeedleOffsetY <= 0)
                        {
                            error = "Координата Y отступа не может быть меньше или равна 0 ";
                        }
                        break;
                    case "StepNeedlesX":
                        if (StepNeedlesX <= 0)
                        {
                            error = "Координата X шага не может быть меньше или равна 0 ";
                        }
                        break;
                    case "StepNeedlesY":
                        if (StepNeedlesY <= 0)
                        {
                            error = "Координата Y шага не может быть меньше или равна 0 ";
                        }
                        break;
                    case "Accuracy":
                        if (double.IsNegative(Accuracy) || !double.IsNormal(Accuracy))
                        {
                            error = "Неверное значение точности";
                        }
                        break;

                }

                if (!Errors.ContainsKey(columnName))
                {
                    Errors.Add(columnName, error);
                }
                else
                {
                    Errors.Remove(columnName);
                }
                return error;
            }
            
        }
    }
}

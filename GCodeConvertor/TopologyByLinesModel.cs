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
    public class TopologyByLineModel : IDataErrorInfo, INotifyPropertyChanged
    {
        public string NameProject { get; set; }

        private string _PathProject;
        public string PathProject 
        {
            get { return _PathProject; }
            set { _PathProject = value; OnPropertyChanged("PathProject"); }
        }
        public int HeadIdentationX { get; set; }
        public int HeadIdentationY { get; set; }
        public float NozzleDiameter { get; set; }
        public float Accuracy { get; set; } //Точность в миллиметрах
        public float Step { get; set; } // Шаг заполнения в миллиметрах
        public string Shape { get; set; } // форма фигуры точки в миллиметрах
        public string TensionLines { get; set; } // линии напряжения точки в миллиметрах

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
                    case "Shape":
                        if (String.IsNullOrWhiteSpace(Shape))
                        {
                            error = "Путь к файлу с фигурой детали не может быть пустым";
                        }
                        break;
                    case "TensionLines":
                        if (String.IsNullOrWhiteSpace(TensionLines))
                        {
                            error = "Путь к файлу с линиями напряжения не может быть пустым";
                        }
                        break;
                    case "Accuracy":
                        if (double.IsNegative(Accuracy) || !double.IsNormal(Accuracy))
                        {
                            error = "Неверное значение точности";
                        }
                        break;
                    case "Step":
                        if (double.IsNegative(Step) || !double.IsNormal(Step))
                        {
                            error = "Неверное значение точности";
                        }
                        if (!(double.IsNegative(Accuracy) || !double.IsNormal(Accuracy)) && Step % Accuracy != 0) 
                        {
                            error = "Шаг заполнения должен быть кратен точности";
                        }
                        break;

                }

                if (error != string.Empty)
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

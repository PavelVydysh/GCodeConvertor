using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor
{
    public class TopologyByLineModel : IDataErrorInfo, INotifyPropertyChanged
    {
        private static string DEFAULT_PROJECT_NAME = "untitled";
        private static string DEFAULT_PROJECT_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static int DEFAULT_HEAD_IDENTATION_X = 5;
        private static int DEFAULT_HEAD_IDENTATION_Y = 5;
        private static int DEFAULT_NOZZLE_DIAMETER = 1;
        private static int DEFAULT_NEEDLE_DIAMETER = 1;
        private static int DEFAULT_ACCURACY = 1;
        private static int DEFAULT_STEP = 20;

        public string NameProject { get; set; }
        private string _pathProject { get; set; }
        public string PathProject { get => _pathProject; set { if (_pathProject != value) { _pathProject = value; OnPropertyChanged(); } } }
        public int HeadIdentationX { get; set; }
        public int HeadIdentationY { get; set; }
        public int NozzleDiameter { get; set; }
        public int NeedleDiameter { get; set; }
        public int Step { get; set; }
        public float Accuracy { get; set; }
        private string _PathShape { get; set; }
        public string PathShape { get => _PathShape; set { if (_PathShape != value) { _PathShape = value; OnPropertyChanged(); } } } // форма фигуры точки в миллиметрах
        private string _PathTensionLines { get; set; }
        public string PathTensionLines { get => _PathTensionLines; set { if (_PathTensionLines != value) { _PathTensionLines = value; OnPropertyChanged(); } } } // линии напряжения точки в миллиметрах

        public Dictionary<String, String> Errors = new Dictionary<string, string>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public TopologyByLineModel()
        {
            NameProject = DEFAULT_PROJECT_NAME;
            PathProject = DEFAULT_PROJECT_PATH;
            HeadIdentationX = DEFAULT_HEAD_IDENTATION_X;
            HeadIdentationY = DEFAULT_HEAD_IDENTATION_Y;
            NozzleDiameter = DEFAULT_NOZZLE_DIAMETER;
            NeedleDiameter = DEFAULT_NEEDLE_DIAMETER;
            Step = DEFAULT_STEP;
            Accuracy = DEFAULT_ACCURACY;

            if (checkIfProjectExitsts())
            {
                generateNameForExistingProject();
            }

        }

        private void generateNameForExistingProject()
        {
            string existingProjectName = NameProject;
            int projectNamingIndex = 1;
            while (checkIfProjectExitsts())
            {
                NameProject = existingProjectName + projectNamingIndex;
                projectNamingIndex++;
            }
        }

        private bool checkIfProjectExitsts()
        {
            if (Directory.Exists(PathProject) && File.Exists(PathProject + "/" + NameProject + ".gcd"))
            {
                return true;
            }
            return false;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;

                switch (columnName)
                {
                    case "NameProject":
                        if (String.IsNullOrWhiteSpace(NameProject))
                        {
                            error = "Название проекта не может быть пустым.";
                            break;
                        }
                        if (checkIfProjectExitsts())
                        {
                            error = string.Format("В директории уже существует проект с именем {0}.", NameProject);
                            break;
                        }
                        break;

                    case "PathProject":
                        if (String.IsNullOrWhiteSpace(PathProject))
                        {
                            error = "Путь к проекту не может быть пустым.";
                            break;
                        }
                        if (!Directory.Exists(PathProject))
                        {
                            error = "Указанной директории не существует.";
                        }
                        break;
                    case "HeadIdentationX":
                        if (HeadIdentationX <= 0)
                        {
                            error = "Отступ платформы по оси X должен быть больше 0 мм";
                        }
                        break;
                    case "HeadIdentationY":
                        if (HeadIdentationY <= 0)
                        {
                            error = "Отступ платформы по оси Y должен быть больше 0 мм";
                        }
                        break;
                    case "NozzleDiameter":
                        if (NozzleDiameter <= 0)
                        {
                            error = "Диаметр сопла должен быть больше 0 мм";
                        }
                        break;
                    case "NeedleDiameter":
                        if (NeedleDiameter <= 0)
                        {
                            error = "Диаметр иглы должен быть больше 0 мм";
                        }
                        break;
                    case "Accuracy":
                        if (!isPowerOfTwo(Accuracy) || Accuracy > 4)
                        {
                            error = "Значение точности должно быть не больше 4, либо быть степенью двойки";
                        }
                        break;
                    case "Step":
                        if (Step < 10)
                        {
                            error = "Шаг должен быть больше, либо равен 10 мм";
                        }
                        break;
                    case "PathShape":
                        if (String.IsNullOrWhiteSpace(PathShape))
                        {
                            error = "Путь к файлу с фигурой не может быть пустым.";
                            break;
                        }
                        if (!File.Exists(PathShape))
                        {
                            error = "Указанного файла не существует.";
                        }
                        break;
                    case "PathTensionLines":
                        if (String.IsNullOrWhiteSpace(PathTensionLines))
                        {
                            error = "Путь к файлу с линиями напряжения не может быть пустым.";
                            break;
                        }
                        if (!File.Exists(PathTensionLines))
                        {
                            error = "Указанного файла не существует.";
                        }
                        break;

                }

                if (error != string.Empty)
                {
                    if (!Errors.ContainsKey(columnName))
                    {
                        Errors.Add(columnName, error);
                    }
                }
                else
                {
                    Errors.Remove(columnName);
                }
                return error;
            }

        }
        private bool isPowerOfTwo(float value)
        {
            double log = Math.Log(Math.Abs(value), 2);
            return (log % 1) == 0;
        }

    }
}

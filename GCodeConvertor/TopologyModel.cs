using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeConvertor
{
    public class TopologyModel : IDataErrorInfo, INotifyPropertyChanged
    {
        private static string DEFAULT_PROJECT_NAME = "untitled";
        private static string DEFAULT_PROJECT_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static int DEFAULT_PLATFORM_WITDH = 100;
        private static int DEFAULT_PLATFORM_HEIGHT = 100;
        private static int DEFAULT_HEAD_IDENTATION_X = 10;
        private static int DEFAULT_HEAD_IDENTATION_Y = 10;
        private static int DEFAULT_NOZZLE_DIAMETER = 1;
        private static int DEFAULT_NEEDLE_DIAMETER = 1;
        private static int DEFAULT_START_NEEDLE_OFFSET_X = 1;
        private static int DEFAULT_START_NEEDLE_OFFSET_Y = 1;
        private static int DEFAULT_STEP_NEEDLE_X = 5;
        private static int DEFAULT_STEP_NEEDLE_Y = 5;
        private static int DEFAULT_ACCURACY = 1;

        public string NameProject { get; set; }
        public string PathProject { get; set; }
        public int PlatformH { get; set; }
        public int PlatformW { get; set; }
        public int HeadIdentationX { get; set; }
        public int HeadIdentationY { get; set; }
        public int NozzleDiameter { get; set; }
        public int NeedleDiameter { get; set; }
        public int StartNeedleOffsetX { get; set; }
        public int StartNeedleOffsetY { get; set; }
        public int StepNeedlesX { get; set; }
        public int StepNeedlesY { get; set; }
        public float Accuracy { get; set; }

        public Dictionary<String, String> Errors = new Dictionary<string, string>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public TopologyModel()
        {
            NameProject = DEFAULT_PROJECT_NAME;
            PathProject = DEFAULT_PROJECT_PATH;
            PlatformH = DEFAULT_PLATFORM_HEIGHT;
            PlatformW = DEFAULT_PLATFORM_WITDH;
            HeadIdentationX = DEFAULT_HEAD_IDENTATION_X;
            HeadIdentationY = DEFAULT_HEAD_IDENTATION_Y;
            NozzleDiameter = DEFAULT_NOZZLE_DIAMETER;
            NeedleDiameter = DEFAULT_NEEDLE_DIAMETER;
            StartNeedleOffsetX = DEFAULT_START_NEEDLE_OFFSET_X;
            StartNeedleOffsetY = DEFAULT_START_NEEDLE_OFFSET_Y;
            StepNeedlesX = DEFAULT_STEP_NEEDLE_X;
            StepNeedlesY = DEFAULT_STEP_NEEDLE_Y;
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
            if(Directory.Exists(PathProject) && File.Exists(PathProject + "/" + NameProject + ".gcd"))
            {
                return true;
            }
            return false;
        }

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

                    case "PlatformH":
                        if (PlatformH < 100)
                        {
                            error = "Длина платформы по оси Y не может быть меньше 100 мм";
                        }
                        break;
                    case "PlatformW":
                        if (PlatformW < 100)
                        {
                            error = "Длина платформы по оси X не может быть меньше 100 мм";
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
                            error = "Диаметр иглы должен быть больше 0мм";
                        }
                        break;
                    case "StartNeedleOffsetX":
                        if (StartNeedleOffsetX <= 0)
                        {
                            error = "Отступ стартовой иглы от края платформы по оси X должен быть больше 0 мм";
                        }
                        break;
                    case "StartNeedleOffsetY":
                        if (StartNeedleOffsetY <= 0)
                        {
                            error = "Отступ стартовой иглы от края платформы по оси Y должен быть больше 0 мм";
                        }
                        break;
                    case "StepNeedlesX":
                        if (StepNeedlesX <= 0)
                        {
                            error = "Шаг между иглами по оси X должен быть больше 0 мм";
                        }
                        break;
                    case "StepNeedlesY":
                        if (StepNeedlesY <= 0)
                        {
                            error = "Шаг между иглами по оси X должен быть больше 0 мм";
                        }
                        break;
                    case "Accuracy":
                        if (double.IsNegative(Accuracy) || !double.IsNormal(Accuracy))
                        {
                            error = "Неверное значение точности";
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
    }
}

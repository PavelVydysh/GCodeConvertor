using GCodeConvertor.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace GCodeConvertor
{
    public class ProjectLoader
    {
        private static string APP_SETTINGS_FOLDER_NAME = ".gconv";
        private static string PROJECT_APP_SETTING_FILE_NAME = "project-info.xml";
        private static string APP_SETTINGS_FOLDER_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, APP_SETTINGS_FOLDER_NAME);

        public static ProjectsInfo loadProjectsInfo()
        {
            if (!Directory.Exists(APP_SETTINGS_FOLDER_PATH))
            {
                Directory.CreateDirectory(APP_SETTINGS_FOLDER_PATH);
                saveProjectsInfo(new ProjectsInfo());
            }
            using (FileStream fs = new FileStream(
                    Path.Combine(APP_SETTINGS_FOLDER_PATH,
                    PROJECT_APP_SETTING_FILE_NAME), FileMode.OpenOrCreate))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProjectsInfo));
                XmlReader xmlReader = new XmlTextReader(fs);
               
                if (xmlSerializer.CanDeserialize(xmlReader))
                {
                    fs.Position = 0;
                    ProjectsInfo? projectsInfo = xmlSerializer.Deserialize(fs) as ProjectsInfo;
                    if(projectsInfo is not null)
                    {
                        return projectsInfo;
                    }
                }
            }
            return new ProjectsInfo();
        }

        public static void saveProjectsInfo(ProjectsInfo projectsInfo)
        {
            if (!Directory.Exists(APP_SETTINGS_FOLDER_PATH))
            {
                Directory.CreateDirectory(APP_SETTINGS_FOLDER_PATH);
            }
            using (FileStream fs = new FileStream(
                Path.Combine(APP_SETTINGS_FOLDER_PATH,
                    PROJECT_APP_SETTING_FILE_NAME), FileMode.OpenOrCreate))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProjectsInfo));
                xmlSerializer.Serialize(fs, projectsInfo);
            }
        }

    }
}

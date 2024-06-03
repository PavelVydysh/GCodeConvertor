using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GCodeConvertor.UI
{
    [Serializable]
    [XmlInclude(typeof(ProjectsInfoItem))]
    public class ProjectsInfo
    {
        private List<ProjectsInfoItem> projectsInfos;

        public List<ProjectsInfoItem> ProjectsInfos { 
            get
            {
                if(projectsInfos is null)
                {
                    projectsInfos =  new List<ProjectsInfoItem>();
                }
                return projectsInfos;
            }
            set { projectsInfos = value; }
        }
        
        public ProjectsInfo() { 
        }

        public class ProjectsInfoItem
        {
            public string ProjectName { get; set; }
            public string PathToProject { get; set; }
            
            public ProjectsInfoItem() { }
            
        }

    }
}

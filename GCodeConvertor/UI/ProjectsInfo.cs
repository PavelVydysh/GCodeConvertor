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

        public void addProjectInfo(ProjectsInfoItem projectsInfoItem)
        {
            if(!projectsInfos.Contains(projectsInfoItem))
            {
                projectsInfos.Add(projectsInfoItem);
            }
            ProjectLoader.saveProjectsInfo(this);
        }
        
        public ProjectsInfo() { 
        }

        public class ProjectsInfoItem
        {
            public string ProjectName { get; set; }
            public string PathToProject { get; set; }
            
            public ProjectsInfoItem(string projectName, string pathToProject) {
                this.ProjectName = projectName;
                this.PathToProject = pathToProject;
            }

            public ProjectsInfoItem() { }

            public override bool Equals(object? obj)
            {
                var item = obj as ProjectsInfoItem;
                if (item == null)
                {
                    return false;
                }
                return this.ProjectName.Equals(item.ProjectName) && this.PathToProject.Equals(item.PathToProject);
            }

        }

    }
}

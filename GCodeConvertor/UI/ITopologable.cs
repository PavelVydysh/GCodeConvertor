namespace GCodeConvertor.UI
{
    public interface ITopologable
    {
        bool isDataCorrect();

        void setTopology();

        string getName();

        string getProjectFullPath();

        string getProjectName();
    }
}
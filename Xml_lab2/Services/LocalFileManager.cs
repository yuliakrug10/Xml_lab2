using System.IO;
using System.Text;

namespace Xml_lab2.Services
{
    public class LocalFileManager
    {
        public void SaveToFile(string content, string path)
        {
            File.WriteAllText(path, content, Encoding.UTF8);
        }

        public string LoadFromFile(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
    }
}

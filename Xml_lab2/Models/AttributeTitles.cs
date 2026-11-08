using System.Collections.Generic;

namespace Xml_lab2.Models
{
    public static class AttributeTitles
    {
        private static Dictionary<string, string> _titles = new Dictionary<string, string>
        {
            { "Section", "Секція" },
            { "Faculty", "Факультет" },
            { "Course", "Курс" },
            { "Rank", "Розряд" },
            { "Role", "Роль" },
            { "Position", "Посада" },
            { "Degree", "Науковий ступінь" }
        };

        public static string GetTitle(string xmlName)
        {
            if (_titles.ContainsKey(xmlName))
            {
                return _titles[xmlName];
            }
            return xmlName;
        }

        public static string GetXmlName(string title)
        {
            foreach (KeyValuePair<string, string> pair in _titles)
            {
                if (pair.Value == title)
                {
                    return pair.Key;
                }
            }
            return title;
        }
    }
}

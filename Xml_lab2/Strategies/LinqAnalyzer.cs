using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xml_lab2.Models;

namespace Xml_lab2.Strategies
{
    public class LinqAnalyzer : IXmlAnalyzerStrategy
    {
        public List<SportRecord> Analyze(string filePath, string attributeName, string attributeValue, string keyword)
        {
            XDocument doc = XDocument.Load(filePath);
            return BuildQuery(doc, attributeName, attributeValue, keyword).ToList();
        }

        private IEnumerable<SportRecord> BuildQuery(XDocument doc, string attributeName, string attributeValue, string keyword)
        {
            return from section in doc.Descendants("Section")
                   let sectionName = GetSectionName(section)
                   let sectionAttrs = GetAttributes(section)
                   let scheduleAttrs = GetAttributes(section.Element("Schedule"))
                   from member in section.Descendants("Member")
                   let attrs = GetAttributes(member)
                   where AnalyzerHelper.MatchesAttribute(attrs, sectionName, attributeName, attributeValue)
                   where AnalyzerHelper.Matches(AnalyzerHelper.SearchValues(attrs, sectionName, scheduleAttrs), keyword)
                   select AnalyzerHelper.BuildRecord(attrs, sectionName, sectionAttrs, scheduleAttrs);
        }

        private static string GetSectionName(XElement section)
        {
            XAttribute nameAttr = section.Attribute("Name");
            if (nameAttr == null)
            {
                return "";
            }
            return nameAttr.Value;
        }

        private static Dictionary<string, string> GetAttributes(XElement element)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();

            if (element == null)
            {
                return attrs;
            }

            foreach (XAttribute attr in element.Attributes())
            {
                attrs[attr.Name.LocalName] = attr.Value;
            }

            return attrs;
        }
    }
}

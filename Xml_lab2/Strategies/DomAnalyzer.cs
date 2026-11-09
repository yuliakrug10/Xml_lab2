using System.Collections.Generic;
using System.Xml;
using Xml_lab2.Models;

namespace Xml_lab2.Strategies
{
    public class DomAnalyzer : IXmlAnalyzerStrategy
    {
        public List<SportRecord> Analyze(string filePath, string attributeName, string attributeValue, string keyword)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            string xpath = BuildXPath(attributeName, attributeValue, keyword);
            XmlNodeList members = xmlDoc.SelectNodes(xpath);

            List<SportRecord> results = new List<SportRecord>();
            TraverseNodes(members, results);
            return results;
        }

        private static string BuildXPath(string attributeName, string attributeValue, string keyword)
        {
            List<string> conditions = new List<string>();

            if (!string.IsNullOrEmpty(attributeName) && !string.IsNullOrEmpty(attributeValue))
            {
                string v = ToXPathLiteral(attributeValue);
                if (attributeName == "Section")
                {
                    conditions.Add("ancestor::Section[@Name=" + v + "]");
                }
                else
                {
                    conditions.Add("@" + attributeName + "=" + v);
                }
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string k = ToXPathLiteral(keyword.Trim());
                string memberCondition = "@*[contains(., " + k + ")]";
                string sectionCondition = "ancestor::Section[@Name[contains(., " + k + ")] or Schedule/@*[contains(., " + k + ")]]";
                conditions.Add("(" + memberCondition + " or " + sectionCondition + ")");
            }

            if (conditions.Count == 0)
            {
                return "//Member";
            }

            return "//Member[" + string.Join(" and ", conditions) + "]";
        }

        private static string ToXPathLiteral(string value)
        {
            if (!value.Contains("'"))
            {
                return "'" + value + "'";
            }

            if (!value.Contains("\""))
            {
                return "\"" + value + "\"";
            }

            return "concat('" + value.Replace("'", "', \"'\", '") + "')";
        }

        private void TraverseNodes(XmlNodeList members, List<SportRecord> results)
        {
            foreach (XmlNode member in members)
            {
                string sectionName = "";
                Dictionary<string, string> sectionAttrs = new Dictionary<string, string>();
                Dictionary<string, string> scheduleAttrs = new Dictionary<string, string>();

                XmlNode section = member.SelectSingleNode("ancestor::Section");

                if (section != null)
                {
                    sectionAttrs = GetAttributes(section);
                    if (sectionAttrs.ContainsKey("Name"))
                    {
                        sectionName = sectionAttrs["Name"];
                    }

                    XmlNode scheduleNode = section.SelectSingleNode("Schedule");
                    if (scheduleNode != null)
                    {
                        scheduleAttrs = GetAttributes(scheduleNode);
                    }
                }

                results.Add(AnalyzerHelper.BuildRecord(GetAttributes(member), sectionName, sectionAttrs, scheduleAttrs));
            }
        }

        private static Dictionary<string, string> GetAttributes(XmlNode node)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();

            foreach (XmlAttribute attr in node.Attributes)
            {
                attrs[attr.Name] = attr.Value;
            }

            return attrs;
        }
    }
}

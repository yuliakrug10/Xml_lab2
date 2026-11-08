using System.Collections.Generic;
using System.Xml;
using Xml_lab2.Models;

namespace Xml_lab2.Strategies
{
    public class SaxAnalyzer : IXmlAnalyzerStrategy
    {
        private List<SportRecord> _results;
        private string _keyword;
        private string _currentSection;
        private Dictionary<string, string> _currentSchedule;

        public List<SportRecord> Analyze(string filePath, string keyword)
        {
            _results = new List<SportRecord>();
            _keyword = keyword;
            _currentSection = "";
            _currentSchedule = new Dictionary<string, string>();

            XmlTextReader xmlReader = new XmlTextReader(filePath);
            try
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        ParseNode(xmlReader);
                    }
                }
            }
            finally
            {
                xmlReader.Close();
            }

            return _results;
        }

        private void ParseNode(XmlTextReader xmlReader)
        {
            string elementName = xmlReader.Name;
            Dictionary<string, string> attrs = ReadAttributes(xmlReader);

            switch (elementName)
            {
                case "Section":
                    _currentSection = "";
                    if (attrs.ContainsKey("Name"))
                    {
                        _currentSection = attrs["Name"];
                    }
                    _currentSchedule = new Dictionary<string, string>();
                    break;

                case "Schedule":
                    _currentSchedule = attrs;
                    break;

                case "Member":
                    List<string> values = AnalyzerHelper.SearchValues(attrs, _currentSection, _currentSchedule);
                    if (AnalyzerHelper.Matches(values, _keyword))
                    {
                        string schedule = AnalyzerHelper.FormatSchedule(_currentSchedule);
                        _results.Add(AnalyzerHelper.BuildRecord(attrs, _currentSection, schedule));
                    }
                    break;
            }
        }

        private static Dictionary<string, string> ReadAttributes(XmlTextReader xmlReader)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();

            if (xmlReader.HasAttributes)
            {
                while (xmlReader.MoveToNextAttribute())
                {
                    attrs[xmlReader.Name] = xmlReader.Value;
                }
                xmlReader.MoveToElement();
            }

            return attrs;
        }
    }
}

using System.Collections.Generic;
using Xml_lab2.Models;

namespace Xml_lab2.Strategies
{
    public interface IXmlAnalyzerStrategy
    {
        List<SportRecord> Analyze(string filePath, string attributeName, string attributeValue, string keyword);
    }
}

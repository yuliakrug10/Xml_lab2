using System.Collections.Generic;
using Xml_lab2.Models;

namespace Xml_lab2.Cloud
{
    public interface IDocumentBuilder
    {
        string BuildContent(List<SportRecord> data);
    }
}

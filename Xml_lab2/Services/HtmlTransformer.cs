using System;
using System.Xml.Xsl;

namespace Xml_lab2.Services
{
    public class HtmlTransformer
    {
        private string _lastError = "";

        public string LastError
        {
            get { return _lastError; }
        }

        public bool Transform(string xmlPath, string xsltPath, string outHtmlPath)
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(xsltPath);
                xslt.Transform(xmlPath, outHtmlPath);
                _lastError = "";
                return true;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                return false;
            }
        }
    }
}

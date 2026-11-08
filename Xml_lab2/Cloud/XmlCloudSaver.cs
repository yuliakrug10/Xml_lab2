namespace Xml_lab2.Cloud
{
    public class XmlCloudSaver : CloudDocumentSaver
    {
        protected override IDocumentBuilder CreateBuilder()
        {
            return new XmlBuilder();
        }

        protected override string Extension
        {
            get { return ".xml"; }
        }

        protected override string MimeType
        {
            get { return "text/xml"; }
        }
    }
}

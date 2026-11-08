namespace Xml_lab2.Cloud
{
    public class HtmlCloudSaver : CloudDocumentSaver
    {
        protected override IDocumentBuilder CreateBuilder()
        {
            return new HtmlBuilder();
        }

        protected override string Extension
        {
            get { return ".html"; }
        }

        protected override string MimeType
        {
            get { return "text/html"; }
        }
    }
}

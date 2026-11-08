using System.Collections.Generic;
using System.Threading.Tasks;
using Xml_lab2.Models;

namespace Xml_lab2.Cloud
{
    public abstract class CloudDocumentSaver
    {
        protected abstract IDocumentBuilder CreateBuilder();

        protected abstract string Extension { get; }

        protected abstract string MimeType { get; }

        public async Task<string> SaveAsync(List<SportRecord> data, string fileName, GoogleDriveManager drive)
        {
            IDocumentBuilder builder = CreateBuilder();
            string content = builder.BuildContent(data);
            string fullName = fileName + Extension;

            await drive.UploadFileAsync(fullName, content, MimeType);
            return fullName;
        }
    }
}

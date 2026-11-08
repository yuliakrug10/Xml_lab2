using System.Collections.Generic;
using System.Net;
using System.Text;
using Xml_lab2.Models;

namespace Xml_lab2.Cloud
{
    public class HtmlBuilder : IDocumentBuilder
    {
        public string BuildContent(List<SportRecord> data)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"uk\">");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"utf-8\">");
            html.AppendLine("<title>Відфільтровані дані</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Segoe UI, sans-serif; margin: 30px; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; }");
            html.AppendLine("th, td { border: 1px solid #999; padding: 6px 10px; text-align: left; }");
            html.AppendLine("th { background: #e8e0f3; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<h1>Спорт на факультеті: відфільтровані дані</h1>");
            html.AppendLine("<p>Кількість записів: " + data.Count + "</p>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>№</th><th>ПІБ</th><th>Факультет</th><th>Секція</th><th>Графік</th><th>Додатково</th></tr>");

            int number = 1;
            foreach (SportRecord record in data)
            {
                html.Append("<tr>");
                html.Append("<td>" + number + "</td>");
                html.Append("<td>" + WebUtility.HtmlEncode(record.StudentName) + "</td>");
                html.Append("<td>" + WebUtility.HtmlEncode(record.Faculty) + "</td>");
                html.Append("<td>" + WebUtility.HtmlEncode(record.SportSection) + "</td>");
                html.Append("<td>" + WebUtility.HtmlEncode(record.Schedule) + "</td>");
                html.Append("<td>" + WebUtility.HtmlEncode(record.Details) + "</td>");
                html.AppendLine("</tr>");
                number++;
            }

            html.AppendLine("</table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            return html.ToString();
        }
    }
}

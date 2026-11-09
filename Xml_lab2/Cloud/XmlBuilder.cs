using System.Collections.Generic;
using System.Xml.Linq;
using Xml_lab2.Models;

namespace Xml_lab2.Cloud
{
    public class XmlBuilder : IDocumentBuilder
    {
        public string BuildContent(List<SportRecord> data)
        {
            XElement root = new XElement("SportClub", new XAttribute("Name", "Відфільтровані дані"));
            Dictionary<string, XElement> membersBySection = new Dictionary<string, XElement>();

            foreach (SportRecord record in data)
            {
                if (!membersBySection.ContainsKey(record.SportSection))
                {
                    XElement section = new XElement("Section", new XAttribute("Name", record.SportSection));
                    foreach (KeyValuePair<string, string> attr in record.SectionAttributes)
                    {
                        section.Add(new XAttribute(attr.Key, attr.Value));
                    }

                    if (record.ScheduleAttributes.Count > 0)
                    {
                        XElement schedule = new XElement("Schedule");
                        foreach (KeyValuePair<string, string> attr in record.ScheduleAttributes)
                        {
                            schedule.Add(new XAttribute(attr.Key, attr.Value));
                        }
                        section.Add(schedule);
                    }

                    XElement members = new XElement("Members");
                    section.Add(members);

                    root.Add(section);
                    membersBySection[record.SportSection] = members;
                }

                XElement member = new XElement("Member");
                member.Add(new XAttribute("Name", record.StudentName));
                member.Add(new XAttribute("Faculty", record.Faculty));

                foreach (KeyValuePair<string, string> attr in record.ExtraAttributes)
                {
                    member.Add(new XAttribute(attr.Key, attr.Value));
                }

                membersBySection[record.SportSection].Add(member);
            }

            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
            return doc.Declaration.ToString() + "\n" + doc.ToString();
        }
    }
}

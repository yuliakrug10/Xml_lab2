using System.Collections.Generic;

namespace Xml_lab2.Models
{
    public class SportRecord
    {
        public string StudentName { get; set; }
        public string Faculty { get; set; }
        public string SportSection { get; set; }
        public string Schedule { get; set; }

        public Dictionary<string, string> ExtraAttributes { get; set; } = new Dictionary<string, string>();

        public string Details
        {
            get
            {
                List<string> parts = new List<string>();
                foreach (KeyValuePair<string, string> attr in ExtraAttributes)
                {
                    parts.Add(AttributeTitles.GetTitle(attr.Key) + ": " + attr.Value);
                }
                return string.Join("; ", parts);
            }
        }

        public SportRecord(string studentName, string faculty, string sportSection, string schedule)
        {
            StudentName = studentName;
            Faculty = faculty;
            SportSection = sportSection;
            Schedule = schedule;
        }
    }
}

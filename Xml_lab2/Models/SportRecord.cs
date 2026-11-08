using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml_lab2.Models
{
    public class SportRecord
    {
        public string StudentName { get; set; }
        public string Faculty { get; set; }
        public string SportSection { get; set; }
        public string Schedule { get; set; }

        public Dictionary<string, string> ExtraAttributes { get; set; } = new Dictionary<string, string>();

        public string Details => string.Join("; ", ExtraAttributes.Select(a => a.Key + ": " + a.Value));

        public SportRecord(string studentName, string faculty, string sportSection, string schedule)
        {
            StudentName = studentName;
            Faculty = faculty;
            SportSection = sportSection;
            Schedule = schedule;
        }
    }
}

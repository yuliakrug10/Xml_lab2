using System;
using System.Collections.Generic;
using Xml_lab2.Models;

namespace Xml_lab2.Strategies
{
    public static class AnalyzerHelper
    {
        public static bool Matches(List<string> values, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return true;
            }

            string key = keyword.Trim();

            foreach (string value in values)
            {
                if (value != null && value.Contains(key, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool MatchesAttribute(Dictionary<string, string> memberAttributes, string section, string attributeName, string attributeValue)
        {
            if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(attributeValue))
            {
                return true;
            }

            if (attributeName == "Section")
            {
                return section == attributeValue;
            }

            if (!memberAttributes.ContainsKey(attributeName))
            {
                return false;
            }

            return memberAttributes[attributeName] == attributeValue;
        }

        public static string FormatSchedule(Dictionary<string, string> scheduleAttributes)
        {
            List<string> parts = new List<string>();

            foreach (string value in scheduleAttributes.Values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    parts.Add(value);
                }
            }

            return string.Join(" ", parts);
        }

        public static SportRecord BuildRecord(Dictionary<string, string> memberAttributes, string section, string schedule)
        {
            string name = "";
            string faculty = "";

            if (memberAttributes.ContainsKey("Name"))
            {
                name = memberAttributes["Name"];
            }

            if (memberAttributes.ContainsKey("Faculty"))
            {
                faculty = memberAttributes["Faculty"];
            }

            SportRecord record = new SportRecord(name, faculty, section, schedule);

            foreach (KeyValuePair<string, string> attr in memberAttributes)
            {
                if (attr.Key != "Name" && attr.Key != "Faculty")
                {
                    record.ExtraAttributes[attr.Key] = attr.Value;
                }
            }

            return record;
        }

        public static List<string> SearchValues(Dictionary<string, string> memberAttributes, string section, Dictionary<string, string> scheduleAttributes)
        {
            List<string> values = new List<string>();
            values.AddRange(memberAttributes.Values);
            values.Add(section);
            values.AddRange(scheduleAttributes.Values);
            return values;
        }
    }
}

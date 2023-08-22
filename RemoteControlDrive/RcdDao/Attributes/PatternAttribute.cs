using System;
using System.Text.RegularExpressions;

namespace RcdDao.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class PatternAttribute : Attribute
    {
        public Regex Pattern { get; private set; }

        public string Msg { get; private set; }

        public PatternAttribute(string _regex, string _msg)
        {
            Pattern = new Regex($"^{_regex}$");
            Msg = _msg;
        }
    }
}
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Project
{
    static class Util
    {
        // ex nnar[topic] -> (nnar, topic)
        //    beb -> (beb, "")
        public static Tuple<string, string> SplitInstanceId(string instanceId)
        {
            var pattern = @"([^\[]*)(\[([^\]]*)\])?";
            var match = Regex.Match(instanceId, pattern);
            return Tuple.Create(match.Groups[1].Value, match.Groups[3].Value);
        }

        public static string ParentAbstractionId(string abstractionId)
        {
            var list = abstractionId.Split('.').ToList();
            list.RemoveAt(list.Count - 1);
            return string.Join('.', list);
        }
    }
}
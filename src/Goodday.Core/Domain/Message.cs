using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goodday.Core.Domain
{
    public class Message : IMessage
    {
        public Header Header { get; set; } = new Header
        {
            QR = true,
            QDCount = 0
        };
        
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<ResourceRecord> Answers { get; set; } = new List<ResourceRecord>();
        public List<ResourceRecord> Authorities { get; set; } = new List<ResourceRecord>();
        public List<ResourceRecord> Additionals { get; set; } = new List<ResourceRecord>();


        public byte[] ToBytes()
        {
            var data = new List<byte>();
            Header.QDCount = (ushort) Questions.Count;
            data.AddRange(Header.Data);
            foreach (var q in Questions)
            {
                data.AddRange(q.ToBytes());
            }

            return data.ToArray();
        }

        public override string ToString()
        {
            var builder = new StringBuilder()
                .AppendLine(Header.QR ? "Response" : "Query")
                .AppendLine("Header:")
                .AppendLine(Header.ToString())
                .AppendIfAny("Questions", Questions)
                .AppendIfAny("Answers", Answers)
                .AppendIfAny("Authorities", Authorities)
                .AppendIfAny("Additionals", Additionals);
            return builder.ToString();
        }
    }

    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendIfAny<T>(this StringBuilder builder, string name, IList<T> items)
        {
            if (items.Any())
            {
                builder.AppendLine(name);
                foreach (var item in items)
                {
                    builder.AppendLine($"{item}");
                }
            }

            return builder;
        }
    }
}
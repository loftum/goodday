using System;
using System.Collections.Generic;
using System.Text;
using Goodday.Core.Domain.Records;

namespace Goodday.Core.Domain
{
    public class MessageWriter
    {
        private readonly List<byte> _bytes = new List<byte>();
        
        public byte[] Write(Message message)
        {
            var header = message.Header;
            Sanitize(message);
            Write(header);
            foreach (var question in message.Questions)
            {
                Write(question);
            }

            foreach (var answer in message.Answers)
            {
                Write(answer);
            }

            return _bytes.ToArray();
        }

        private void Write(ResourceRecord resourceRecord)
        {
            _bytes.AddRange(DomainNameToBytes(resourceRecord.Name));
            _bytes.AddRange(((ushort)resourceRecord.Type).ToBytes());
            _bytes.AddRange(((ushort)resourceRecord.Class).ToBytes());
            _bytes.AddRange(resourceRecord.Ttl.ToBytes());
            //_bytes.AddRange(resourceRecord.RDLength.ToBytes());
            Write(resourceRecord.Record);
        }

        private void Write(IRecord record)
        {
            switch (record)
            {
                case null:
                    throw new ArgumentNullException(nameof(record));
                case PointerRecord p:
                {
                    var bytes = DomainNameToBytes(p.PTRDName);
                    _bytes.AddRange(((ushort)bytes.Length).ToBytes());
                    _bytes.AddRange(DomainNameToBytes(p.PTRDName));
                    break;
                }
                case ServiceRecord s:
                {
                    var bytes = new List<byte>();
                    bytes.AddRange(s.Priority.ToBytes());
                    bytes.AddRange(s.Weight.ToBytes());
                    bytes.AddRange(s.Port.ToBytes());
                    bytes.AddRange(DomainNameToBytes(s.Target));
                    
                    _bytes.AddRange(((ushort)bytes.Count).ToBytes());
                    _bytes.AddRange(bytes);
                }
                    break;
                case TextRecord t:
                {
                    var bytes = new List<byte>();
                    foreach (var text in t.Text)
                    {
                        bytes.Add((byte)text.Length);
                        bytes.AddRange(Encoding.UTF8.GetBytes(text));
                    }
                    _bytes.AddRange(((ushort)bytes.Count).ToBytes());
                    _bytes.AddRange(bytes);
                    break;
                }
                case UnknownRecord u:
                {
                    _bytes.AddRange(((ushort)u.RData.Length).ToBytes());
                    _bytes.AddRange(u.RData);
                }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown record {record.GetType().Name}");
            }
        }

        private void Write(Question question)
        {
            _bytes.AddRange(DomainNameToBytes(question.QName));
            _bytes.AddRange(((ushort) question.QType).ToBytes());
            _bytes.AddRange(((ushort) question.QClass).ToBytes());
        }
        
        private static void Sanitize(Message message)
        {
            var header = message.Header;
            header.QDCount = (ushort) message.Questions.Count;
            header.ANCount = (ushort) message.Answers.Count;
            header.NSCount = (ushort) message.Authorities.Count;
            header.ARCount = (ushort) message.Additionals.Count;
        }

        private void Write(Header header)
        {
            _bytes.AddRange(header.Id.ToBytes());
            _bytes.AddRange(header.Flags.ToBytes());
            _bytes.AddRange(header.QDCount.ToBytes());
            _bytes.AddRange(header.ANCount.ToBytes());
            _bytes.AddRange(header.NSCount.ToBytes());
            _bytes.AddRange(header.ARCount.ToBytes());
        }
        
        static byte[] DomainNameToBytes(string src)
        {
            if (!src.EndsWith(".", StringComparison.Ordinal))
            {
                src = $"{src}.";
            }

            if (src == ".")
            {
                return new byte[1];
            }
                

            var sb = new StringBuilder();
            int ii, jj, intLen = src.Length;
            sb.Append('\0');
            for (ii = 0, jj = 0; ii < intLen; ii++, jj++)
            {
                sb.Append(src[ii]);
                if (src[ii] == '.')
                {
                    sb[ii - jj] = (char)(jj & 0xff);
                    jj = -1;
                }
            }
            sb[^1] = '\0';
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
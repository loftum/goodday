namespace Goodday.Core.Domain.Records
{
    public class NSECRecord : IRecord
    {
        public byte[] RData { get; set; }

        public override string ToString()
        {
            return "NOT USED";
        }
    }
}
namespace Goodday.Core.Domain.Records
{
    public class ARecord: IRecord
    {
        public string Address { get; set; }

        public override string ToString()
        {
            return Address;
        }
    }
}
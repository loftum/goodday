namespace Goodday.Core.Domain.Records
{
    public class AAAARecord : IRecord
    {
        public string Address { get; set; }

        public override string ToString()
        {
            return Address;
        }
    }
}
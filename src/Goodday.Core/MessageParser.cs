using Goodday.Core.Domain;

namespace Goodday.Core
{
    public class MessageParser
    {
        public static Message Parse(byte[] bytes)
        {
            return new MessageReader(bytes).Read();
        } 
    }
}
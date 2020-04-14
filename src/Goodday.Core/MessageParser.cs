using Goodday.Core.Domain;

namespace Goodday.Core
{
    public class MessageParser
    {
        public static Message Decode(byte[] bytes)
        {
            return new MessageReader(bytes).Read();
        }

        public static byte[] Encode(Message message)
        {
            return new MessageWriter().Write(message);
        }
    }
}
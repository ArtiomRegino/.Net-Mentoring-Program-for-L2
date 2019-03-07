using System;

namespace CentralManagementService.MessageProcessors
{
    public class MessageCorruptedException : Exception
    {
        public MessageCorruptedException()
        {
        }

        public MessageCorruptedException(string message)
            : base(message)
        {
        }

        public MessageCorruptedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

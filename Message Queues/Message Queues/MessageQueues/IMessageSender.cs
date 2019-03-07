using System.IO;

namespace Message_Queues.MessageQueues
{
    public interface IMessageSender
    {
        void SendToQueue(Stream stream);
    }
}

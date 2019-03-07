namespace CentralManagementService.MessageProcessors
{
    public interface IMessageProcessor
    {
        void StartProcessing();
        void CancelProcessing();
    }
}

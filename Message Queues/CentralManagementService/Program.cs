using Topshelf;

namespace CentralManagementService
{
    internal class Program
    {
        private static void Main()
        {
            HostFactory.Run(
                conf => conf.Service<CentralManagementService>(
                    service =>
                    {
                        service.ConstructUsing(() => new CentralManagementService());
                        service.WhenStarted(serv => serv.Start());
                        service.WhenStopped(serv => serv.Stop());
                    }
                )
            );
        }
    }
}

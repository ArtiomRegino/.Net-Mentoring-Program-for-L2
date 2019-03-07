using System;

namespace CentralManagementService.FileConstruction
{
    public class FileConstructionException: Exception
    {
        public FileConstructionException()
        {
        }

        public FileConstructionException(string message)
            : base(message)
        {
        }

        public FileConstructionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

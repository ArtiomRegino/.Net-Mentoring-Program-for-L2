using System.IO;
using System.Threading;

namespace CentralManagementService.Exstensions
{
    public static class FileInfoExstensions
    {
        /// <summary>
        /// Checks if file is avalable to work with.
        /// </summary>
        /// <param name="file">FileInfo</param>
        /// <param name="fileAccess">Access type.</param>
        /// <returns>True - unavailable (still being written to, or being processed by another thread, or does not exist). False - available.</returns>
        public static bool IsLocked(this FileInfo file, FileAccess fileAccess)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, fileAccess, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }

            return false;
        }

        /// <summary>
        /// Makes a number of attempts to open a file with a delay.
        /// </summary>
        /// <param name="fileInfo">FileInfo object.</param>
        /// <param name="access">File access type.</param>
        /// <param name="times">Number of attempts.</param>
        /// <param name="delay">Delay in seconds.</param>
        public static void TryOpenFile(this FileInfo fileInfo, FileAccess access, int times, int delay)
        {
            int i = 0;
            do
            {
                if (fileInfo.IsLocked(access))
                    Thread.Sleep(delay * 1000);
                else
                    return;
            } while (i++ < times);
        }
    }
}

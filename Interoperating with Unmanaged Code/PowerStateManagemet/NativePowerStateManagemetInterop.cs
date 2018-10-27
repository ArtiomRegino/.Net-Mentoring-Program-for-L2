using System;
using System.Runtime.InteropServices;

namespace PowerStateManagemet
{
    internal static class NativePowerStateManagemetInterop
    {
        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern UInt32 CallNtPowerInformation(
            int InformationLevel,
            IntPtr lpInputBuffer,
            int nInputBufferSize,
            IntPtr lpOutputBuffer,
            int nOutputBufferSize
        );

        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern bool SetSuspendState(
            bool bHibernate,
            bool bForce,
            bool bWakeupEventsDisabled
        );
    }
}

using System;
using System.Runtime.InteropServices;

namespace PowerStateManagemet
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("F746492A-6B2F-4E15-98EF-04515EEBE60B")]
    interface IPowerStateManagement
    {
        UInt32 CallNtPowerInformation(int informationLevel, IntPtr lpInputBuffer, int nInputBufferSize, IntPtr lpOutputBuffer, int nOutputBufferSize);
        bool SetSuspendState(bool bHibernate, bool bWakeupEventsDisabled);
    }
}

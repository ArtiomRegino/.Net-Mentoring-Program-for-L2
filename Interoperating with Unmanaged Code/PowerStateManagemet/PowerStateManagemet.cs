using System;
using System.Runtime.InteropServices;

namespace PowerStateManagemet
{
    [ComVisible(true)]
    [Guid("EC375605-EA60-48C5-B3DD-43468195340F")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class PowerStateManagemet: IPowerStateManagement
    {
        [DispId(1)]
        public uint CallNtPowerInformation(int informationLevel, IntPtr lpInputBuffer, int nInputBufferSize, IntPtr lpOutputBuffer,
            int nOutputBufferSize)
        {
            return NativePowerStateManagemetInterop.CallNtPowerInformation(informationLevel, lpInputBuffer,
                nInputBufferSize, lpOutputBuffer, nOutputBufferSize);
        }

        [DispId(2)]
        public bool SetSuspendState(bool bHibernate, bool bWakeupEventsDisabled)
        {
            return NativePowerStateManagemetInterop.SetSuspendState(bHibernate, false, bWakeupEventsDisabled);
        }
    }
}

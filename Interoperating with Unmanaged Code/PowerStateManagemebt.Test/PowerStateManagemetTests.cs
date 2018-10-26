using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerStateManagemebt.Test;

namespace PowerStateManagemet.Test
{
    [TestClass]
    public class PowerStateManagemetTests
    {
        public IntPtr AllocateMemory(Type type)
        {
            return Marshal.AllocCoTaskMem(Marshal.SizeOf(type));
        }

        [TestMethod]
        public void CallNtPowerInformation_LastSleepTime_Test()
        {
            IntPtr lastSleep = AllocateMemory(typeof(ulong));
            
            var status = PowerStateManagemet.CallNtPowerInformation((int)InformationLevel.LastSleepTime,
                                                    IntPtr.Zero, 0, lastSleep, Marshal.SizeOf(typeof(ulong)));

            var battStatus = (ulong)Marshal.ReadInt64(lastSleep, 0);
            Marshal.FreeCoTaskMem(lastSleep);
            Console.WriteLine($"{battStatus}");
            
            Assert.AreEqual(0, (int)status);
        }

        [TestMethod]
        public void CallNtPowerInformation_LastWakeTime_Test()
        {
            IntPtr lastWakeTime = AllocateMemory(typeof(ulong));

            var status = PowerStateManagemet.CallNtPowerInformation((int)InformationLevel.LastWakeTime,
                IntPtr.Zero, 0, lastWakeTime, Marshal.SizeOf(typeof(ulong)));

            var battStatus = (ulong)Marshal.ReadInt64(lastWakeTime, 0);
            Marshal.FreeCoTaskMem(lastWakeTime);
            Console.WriteLine($"{battStatus}");

            Assert.AreEqual(0, (int)status);
        }

        [TestMethod]
        public void CallNtPowerInformation_SystemBatteryState_Test()
        {
            IntPtr sbs = AllocateMemory(typeof(SYSTEM_BATTERY_STATE));

            var status = PowerStateManagemet.CallNtPowerInformation((int)InformationLevel.SystemBatteryState,
                                        IntPtr.Zero, 0, sbs, Marshal.SizeOf(typeof(SYSTEM_BATTERY_STATE)));

            var battStatus = (SYSTEM_BATTERY_STATE)Marshal.PtrToStructure(sbs, typeof(SYSTEM_BATTERY_STATE));
            Marshal.FreeCoTaskMem(sbs);
            Console.WriteLine($"{battStatus}");

            Assert.AreEqual(0, (int)status);
        }

        [TestMethod]
        public void CallNtPowerInformation_SystemPowerInformation_Test()
        {
            IntPtr powerInfo = AllocateMemory(typeof(SYSTEM_POWER_INFORMATION));

            var status = PowerStateManagemet.CallNtPowerInformation((int)InformationLevel.SystemPowerInformation,
                                            IntPtr.Zero, 0, powerInfo, Marshal.SizeOf(typeof(SYSTEM_POWER_INFORMATION)));

            var info = (SYSTEM_POWER_INFORMATION)Marshal.PtrToStructure(powerInfo, typeof(SYSTEM_POWER_INFORMATION));
            Marshal.FreeCoTaskMem(powerInfo);
            Console.WriteLine($"{info}");

            Assert.AreEqual(0, (int)status);
        }

        [TestMethod]
        public void CallNtPowerInformation_SystemReserveHiberFile_Test()
        {
            IntPtr infoHyb = AllocateMemory(typeof(bool));
            Marshal.WriteByte(infoHyb, Convert.ToByte(false));

            var status = PowerStateManagemet.CallNtPowerInformation((int)InformationLevel.SystemReserveHiberFile,
                infoHyb, Marshal.SizeOf(infoHyb), IntPtr.Zero, 0);

            Marshal.FreeCoTaskMem(infoHyb);

            Assert.AreEqual(0, (int)status);
        }

        [TestMethod]
        public void CallNtPowerInformation_SetSuspendState_Test()
        {
            bool bHibernate = true;
            bool bForce = true;
            bool bWakeupEventsDisabled = false;

            var status = PowerStateManagemet.SetSuspendState(bHibernate, bForce, bWakeupEventsDisabled);

            Console.WriteLine(status);
            Assert.IsTrue(status);
        }
    }
}

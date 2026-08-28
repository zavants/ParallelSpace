using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ParallelSpace.Services;

/// <summary>
/// A Windows job object holding every process of one launched space.
///
/// Many Windows apps start through a stub that spawns the real process and exits
/// immediately (Squirrel/Electron updaters, bootstrappers). Tracking only the
/// process we started would report such a space as "stopped" seconds after
/// launch, and leave nothing to close. Children inherit the job, so the job stays
/// an accurate handle on the whole tree.
///
/// The kill-on-close limit is deliberately NOT set: closing the manager must
/// leave launched apps running.
/// </summary>
public sealed class JobObject : IDisposable
{
    private IntPtr _handle;

    public bool IsValid => _handle != IntPtr.Zero;

    public JobObject()
    {
        _handle = CreateJobObject(IntPtr.Zero, null);
    }

    /// <summary>Puts a process (and everything it later spawns) into this job.</summary>
    public bool Assign(Process process)
    {
        if (!IsValid)
            return false;
        try { return AssignProcessToJobObject(_handle, process.Handle); }
        catch { return false; }
    }

    /// <summary>Same, for a raw process handle (used while the process is suspended).</summary>
    public bool AssignHandle(IntPtr processHandle)
    {
        if (!IsValid || processHandle == IntPtr.Zero)
            return false;
        try { return AssignProcessToJobObject(_handle, processHandle); }
        catch { return false; }
    }

    /// <summary>Live process IDs in this job. Empty when the job is gone or idle.</summary>
    public List<int> GetProcessIds()
    {
        var ids = new List<int>();
        if (!IsValid)
            return ids;

        // JOBOBJECT_BASIC_PROCESS_ID_LIST: two DWORDs (assigned, in-list), then a
        // ULONG_PTR-aligned array of ids starting at offset 8 on both 32/64-bit.
        const int maxProcesses = 512;
        const int headerSize = 8;
        int size = headerSize + IntPtr.Size * maxProcesses;
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.WriteInt32(buffer, 0, 0);
            Marshal.WriteInt32(buffer, 4, 0);

            // Returns false with ERROR_MORE_DATA when the list is truncated; the
            // ids written so far are still valid, so keep parsing either way.
            QueryInformationJobObject(_handle, JobObjectBasicProcessIdList, buffer, size, out _);

            int inList = Marshal.ReadInt32(buffer, 4);
            for (int i = 0; i < inList && i < maxProcesses; i++)
            {
                int pid = Marshal.ReadIntPtr(buffer, headerSize + i * IntPtr.Size).ToInt32();
                if (pid != 0)
                    ids.Add(pid);
            }
        }
        catch
        {
            // Fall through with whatever we collected.
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
        return ids;
    }

    /// <summary>Terminates every process in this job — and nothing outside it.</summary>
    public bool TerminateAll()
    {
        if (!IsValid)
            return false;
        try { return TerminateJobObject(_handle, 0); }
        catch { return false; }
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            try { CloseHandle(_handle); } catch { }
            _handle = IntPtr.Zero;
        }
    }

    private const int JobObjectBasicProcessIdList = 3;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateJobObject(IntPtr securityAttributes, string? name);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool QueryInformationJobObject(IntPtr job, int infoClass,
        IntPtr info, int infoLength, out int returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool TerminateJobObject(IntPtr job, uint exitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);
}

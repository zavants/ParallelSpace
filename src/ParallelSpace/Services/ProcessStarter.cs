using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ParallelSpace.Services;

/// <summary>
/// Starts a process suspended so it can be placed in a job object before it runs
/// a single instruction.
///
/// Process.Start + AssignProcessToJobObject loses a race against launcher stubs:
/// a Squirrel/Electron style stub can spawn the real app and exit before the
/// assignment lands, leaving that app outside the job — untracked and unclosable.
/// Creating the process suspended closes that window entirely.
/// </summary>
public static class ProcessStarter
{
    public readonly record struct Result(bool Ok, int ProcessId, string? Error);

    /// <summary>
    /// Creates the process suspended, hands it to <paramref name="job"/>, then resumes it.
    /// <paramref name="environment"/> overrides are applied on top of the current environment.
    /// </summary>
    public static Result StartInJob(
        string exePath,
        string arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string>? environment,
        JobObject job)
    {
        // CreateProcess may write to lpCommandLine, so hand it a mutable buffer.
        var commandLine = new StringBuilder($"\"{exePath}\" {arguments}".TrimEnd());

        var si = new STARTUPINFO { cb = Marshal.SizeOf<STARTUPINFO>() };
        IntPtr envBlock = IntPtr.Zero;

        try
        {
            uint flags = CREATE_SUSPENDED;
            if (environment is { Count: > 0 })
            {
                envBlock = BuildEnvironmentBlock(environment);
                flags |= CREATE_UNICODE_ENVIRONMENT;
            }

            if (!CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, false,
                    flags, envBlock, workingDirectory, ref si, out var pi))
            {
                var err = Marshal.GetLastWin32Error();
                return new Result(false, 0, new System.ComponentModel.Win32Exception(err).Message);
            }

            try
            {
                // Assign before the first instruction runs: anything the process
                // spawns from here on is inside the job.
                job.AssignHandle(pi.hProcess);
                ResumeThread(pi.hThread);
                return new Result(true, pi.dwProcessId, null);
            }
            finally
            {
                CloseHandle(pi.hThread);
                CloseHandle(pi.hProcess);
            }
        }
        catch (Exception ex)
        {
            return new Result(false, 0, ex.Message);
        }
        finally
        {
            if (envBlock != IntPtr.Zero)
                Marshal.FreeHGlobal(envBlock);
        }
    }

    /// <summary>Current environment plus overrides, as a sorted double-null-terminated block.</summary>
    private static IntPtr BuildEnvironmentBlock(IReadOnlyDictionary<string, string> overrides)
    {
        var vars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (System.Collections.DictionaryEntry e in Environment.GetEnvironmentVariables())
        {
            var key = e.Key?.ToString();
            if (!string.IsNullOrEmpty(key))
                vars[key] = e.Value?.ToString() ?? "";
        }
        foreach (var kv in overrides)
            vars[kv.Key] = kv.Value;

        var sb = new StringBuilder();
        foreach (var kv in vars.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            sb.Append(kv.Key).Append('=').Append(kv.Value).Append('\0');
        sb.Append('\0');

        return Marshal.StringToHGlobalUni(sb.ToString());
    }

    private const uint CREATE_SUSPENDED = 0x00000004;
    private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct STARTUPINFO
    {
        public int cb;
        public IntPtr lpReserved, lpDesktop, lpTitle;
        public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags;
        public short wShowWindow, cbReserved2;
        public IntPtr lpReserved2, hStdInput, hStdOutput, hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
        public IntPtr hProcess, hThread;
        public int dwProcessId, dwThreadId;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateProcess(
        string? lpApplicationName, StringBuilder lpCommandLine,
        IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles,
        uint dwCreationFlags, IntPtr lpEnvironment, string? lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);
}

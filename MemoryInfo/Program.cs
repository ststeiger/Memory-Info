
namespace MemoryInfo
{



    class Program
    {

        static void Main(string[] args)
        {
            //Test();
            System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
            Nat.Mem.GetAllMem(proc);
            Nat.IO.GetALLIO(proc);
        }
    }


    // http://nadeausoftware.com/articles/2012/07/c_c_tip_how_get_process_resident_set_size_physical_memory_use
    // http://www.pinvoke.net/default.aspx/psapi.getprocessmemoryinfo
    static class Nat
    {



        // The <sys/time.h> header defines the timeval structure that includes at least the following members:
        // printf ("sizeof time_t is: %d\n", sizeof(time_t));
        // time_t         tv_sec      seconds // long
        // suseconds_t    tv_usec     microseconds

        // Linux/include/linux/types.h
        // typedef __kernel_suseconds_t    suseconds_t;
        // . typedef __kernel_suseconds_t suseconds_t; 
        // http://www.sde.cs.titech.ac.jp/~gondow/dwarf2-xml/HTML-rxref/app/gcc-3.3.2/lib/gcc-lib/sparc-sun-solaris2.8/3.3.2/include/sys/types.h.html
        // typedef long	suseconds_t;

        // typedef struct timeval {
        //   long tv_sec;
        //   long tv_usec;
        // } timeval;

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct timeval // Linux
        {
            /// <summary>
            /// Time interval, in seconds.
            /// </summary>
            public System.IntPtr tv_sec;

            /// <summary>
            /// Time interval, in microseconds.
            /// </summary>
            public System.IntPtr tv_usec;
        };


        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct timeval_windows
        {
            /// <summary>
            /// Time interval, in seconds.
            /// </summary>
            public int tv_sec;

            /// <summary>
            /// Time interval, in microseconds.
            /// </summary>
            public int tv_usec;
        };


        // http://stackoverflow.com/questions/11356330/getting-cpu-usage-with-golang
        // https://github.com/kognate/codemash-2013/blob/master/GoGoGolang/golang.org
        // https://github.com/kognate/codemash-2013/tree/master/GoGoGolang/src
        // https://github.com/dreyou/spwd
        // https://github.com/datacratic/gometer/blob/master/meter/meter_process.go


        // http://stackoverflow.com/questions/131303/how-to-measure-actual-memory-usage-of-an-application-or-process
        // http://stackoverflow.com/questions/22372960/is-this-explanation-about-vss-rss-pss-uss-accurately
        // procrank
        // http://stackoverflow.com/questions/669438/how-to-get-memory-usage-at-run-time-in-c

        // http://www.cyberciti.biz/faq/linux-which-process-is-using-swap/

        // sudo apt-get install smem
        // smem -P firefox
        // cat /proc/PID/smaps | grep "Swap" | awk '{print $2}' >> lol.csv
        // http://stackoverflow.com/questions/15086993/know-how-much-swap-is-used-by-each-process-on-linux
        // http://superuser.com/questions/300004/how-can-i-find-out-processes-are-using-swap-space
        // http://serverfault.com/questions/550793/how-to-find-what-is-using-linux-swap-or-what-is-in-the-swap


        // http://elinux.org/Runtime_Memory_Measurement
        // /proc/<pid>/statm fields: columns are (in pages):
        //total program size|
        //resident set size|
        //shared pages|
        //text (code) |
        //data/stack |
        //library |
        //dirty pages |
        //Here an example: 693 406 586 158 0 535 0

        // http://man7.org/linux/man-pages/man5/proc.5.html

        // Vss = virtual set size
        // http://elinux.org/Android_Memory_Usage

        // RSS - Resident Set Size. This is the amount of shared memory plus unshared memory used by each process. 
        // If any processes share memory, this will over-report the amount of memory actually used, 
        // because the same shared memory will be counted more than once - appearing again in each other process 
        // that shares the same memory. Thus it is fairly unreliable, especially when high-memory processes have a lot of forks - 
        // which is common in a server, with things like Apache or PHP(fastcgi/FPM) processes.

        // PSS - Proportional Set Size. This is what you want. It adds together the unique memory (USS), 
        // along with a proportion of its shared memory divided by the number of other processes sharing that memory. 
        // Thus it will give you an accurate representation of how much actual physical memory is being used per process - 
        // with shared memory truly represented as shared. 
        // Think of the P being for physical memory.

        // USS - Unique Set Size. This is the amount of unshared memory unique to that process (think of it as U for unique memory). 
        // It does not include shared memory. Thus this will under-report the amount of memory a process uses, 
        // but is helpful when you want to ignore shared memory.




        // http://manpages.ubuntu.com/manpages/trusty/man2/getrusage.2freebsd.html
        // #include <sys/types.h>
        // #include <sys/time.h>
        // #include <sys/resource.h>
        // int getrusage(int who, struct rusage *rusage);
        // #define RUSAGE_BOTH -2 
        
        // #define RUSAGE_CHILDREN -1
        // Return resource usage statistics for all children of the calling process that have terminated and been waited for. 
        // These statistics will include the resources used by grandchildren, and further removed descendants, 
        // if all of the intervening descendants waited on their terminated children.

        // #define RUSAGE_SELF 0  
        // Return resource usage statistics for the calling process, which is the sum of resources used by all threads in the process.
        
        // #define RUSAGE_THREAD 1 // since Linux 2.6.26) 
        // Return resource usage statistics for the calling thread. 
        // The _GNU_SOURCE feature test macro must be defined (before including any header file) 
        // in order to obtain the definition of this constant from <sys/resource.h>




        // http://linux.die.net/man/2/getrusage
        // http://manpages.courier-mta.org/htmlman2/getrusage.2.html
        // #include <sys/time.h>
        // #include <sys/resource.h>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct rusage
        {
            public timeval ru_utime; /* user CPU time used */
            public timeval ru_stime; /* system CPU time used */
            public System.IntPtr ru_maxrss; // C-long: maximum resident set size 
            public System.IntPtr ru_ixrss; // C-long: integral shared memory size 
            public System.IntPtr ru_idrss; // C-long: integral unshared data size 
            public System.IntPtr ru_isrss; // C-long: integral unshared stack size 
            public System.IntPtr ru_minflt; // C-long: page reclaims (soft page faults) 
            public System.IntPtr ru_majflt; // C-long: page faults (hard page faults) 
            public System.IntPtr ru_nswap; // C-long: swaps 
            public System.IntPtr ru_inblock; // C-long: block input operations 
            public System.IntPtr ru_oublock; // C-long: block output operations 
            public System.IntPtr ru_msgsnd; // C-long: IPC messages sent 
            public System.IntPtr ru_msgrcv; // C-long: IPC messages received 
            public System.IntPtr ru_nsignals; // C-long: signals received 
            public System.IntPtr ru_nvcsw; // C-long: voluntary context switches 
            public System.IntPtr ru_nivcsw; // C-long: involuntary context switches 
        };


        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private unsafe static extern int getrusage(int who, out rusage usage);



        // [DllImport("kernel32.dll")]
        // public static extern IntPtr GetCurrentProcess();

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }


        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms684877(v=vs.85).aspx
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = 40)]
        private struct PROCESS_MEMORY_COUNTERS
        {
            public uint cb; // The size of the structure, in bytes (DWORD).
            public uint PageFaultCount; // The number of page faults (DWORD).
            public uint PeakWorkingSetSize; // The peak working set size, in bytes (SIZE_T).
            public uint WorkingSetSize; // The current working set size, in bytes (SIZE_T).
            public uint QuotaPeakPagedPoolUsage; // The peak paged pool usage, in bytes (SIZE_T).
            public uint QuotaPagedPoolUsage; // The current paged pool usage, in bytes (SIZE_T).
            public uint QuotaPeakNonPagedPoolUsage; // The peak nonpaged pool usage, in bytes (SIZE_T).
            public uint QuotaNonPagedPoolUsage; // The current nonpaged pool usage, in bytes (SIZE_T).
            public uint PagefileUsage; // The Commit Charge value in bytes for this process (SIZE_T). Commit Charge is the total amount of memory that the memory manager has committed for a running process.
            public uint PeakPagefileUsage; // The peak value in bytes of the Commit Charge during the lifetime of this process (SIZE_T).
        }


        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private unsafe static extern bool GetProcessIoCounters(System.IntPtr ProcessHandle, out IO_COUNTERS IoCounters);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms683219(v=vs.85).aspx
        [System.Runtime.InteropServices.DllImport("psapi.dll", SetLastError = true)]
        private unsafe static extern bool GetProcessMemoryInfo(System.IntPtr hProcess, out PROCESS_MEMORY_COUNTERS counters, uint size);


        public static class IO
        {
            unsafe public static System.Collections.Generic.Dictionary<string, ulong> GetALLIO(System.Diagnostics.Process procToRtrivIO)
            {
                IO_COUNTERS counters;
                System.Collections.Generic.Dictionary<string, ulong> retCountIoDict = 
                    new System.Collections.Generic.Dictionary<string, ulong>();
                System.IntPtr ptr = System.Diagnostics.Process.GetCurrentProcess().Handle;

                GetProcessIoCounters(ptr, out counters);
                retCountIoDict.Add("ReadOperationCount", counters.ReadOperationCount);
                retCountIoDict.Add("WriteOperationCount", counters.WriteOperationCount);
                retCountIoDict.Add("OtherOperationCount", counters.OtherOperationCount);
                retCountIoDict.Add("ReadTransferCount", counters.ReadTransferCount);
                retCountIoDict.Add("WriteTransferCount", counters.WriteTransferCount);
                retCountIoDict.Add("OtherTransferCount", counters.OtherTransferCount);
                return retCountIoDict;
                //return  "This process has read " + ((counters.ReadTransferCount/1024)/1024).ToString("N0") +
                //    " Mb of data.";

            }
        } // End Class IO 


        public static class Mem
        {
            unsafe public static System.Collections.Generic.Dictionary<string, uint> GetAllMem(System.Diagnostics.Process procToRtrivMem)
            {
                PROCESS_MEMORY_COUNTERS MemCounters;
                System.Collections.Generic.Dictionary<string, uint> retCountMemDict = 
                    new System.Collections.Generic.Dictionary<string, uint>();
                System.IntPtr ptr = System.Diagnostics.Process.GetCurrentProcess().Handle;
                uint nativeStructSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(PROCESS_MEMORY_COUNTERS));


                GetProcessMemoryInfo(ptr, out MemCounters, nativeStructSize); //MemCounters.cb);
                retCountMemDict.Add("cb", MemCounters.cb);
                retCountMemDict.Add("PageFaultCount", MemCounters.PageFaultCount);
                retCountMemDict.Add("PeakWorkingSetSize", MemCounters.PeakWorkingSetSize);
                retCountMemDict.Add("WorkingSetSize", MemCounters.WorkingSetSize);
                retCountMemDict.Add("QuotaPeakPagedPoolUsage", MemCounters.QuotaPeakPagedPoolUsage);
                retCountMemDict.Add("QuotaPagedPoolUsage", MemCounters.QuotaPagedPoolUsage);

                retCountMemDict.Add("QuotaPeakNonPagedPoolUsage", MemCounters.QuotaPeakNonPagedPoolUsage);
                retCountMemDict.Add("QuotaNonPagedPoolUsage", MemCounters.QuotaNonPagedPoolUsage);
                retCountMemDict.Add("PagefileUsage", MemCounters.PagefileUsage);
                retCountMemDict.Add("PeakPagefileUsage", MemCounters.PeakPagefileUsage);

                return retCountMemDict;
                //return  "This process has read " + ((counters.ReadTransferCount/1024)/1024).ToString("N0") +
                //    " Mb of data.";
            }


        } // End Class Mem

    }




}

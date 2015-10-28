
// http://nadeausoftware.com/articles/2012/07/c_c_tip_how_get_process_resident_set_size_physical_memory_use
// Author:  David Robert Nadeau
// Site : http ://NadeauSoftware.com/
// License : Creative Commons Attribution 3.0 Unported License
// http ://creativecommons.org/licenses/by/3.0/deed.en_US

// Format fixes (C) 28.10.2015, sts


#include <stdio.h>
#include <stdlib.h>


// For waiting...
#if defined(_WIN32)
	#include <conio.h>
	// #include <tchar.h> // if tchar main

	#define snprintf(buf,len, format,...) _snprintf_s(buf, len,len, format, __VA_ARGS__)

#elif defined(__linux__) || defined(__linux) || defined(linux) || defined(__gnu_linux__)
	// #include <unistd.h> //STDIN_FILENO
	#include <termios.h> //termios, TCSANOW, ECHO, ICANON
#endif



#if defined(_WIN32)
	#include <windows.h>
	#include <psapi.h>
#elif defined(__unix__) || defined(__unix) || defined(unix) || (defined(__APPLE__) && defined(__MACH__))
	#include <unistd.h>
	#include <sys/resource.h>

#if defined(__APPLE__) && defined(__MACH__)
	#include <mach/mach.h>

#elif (defined(_AIX) || defined(__TOS__AIX__)) || (defined(__sun__) || defined(__sun) || defined(sun) && (defined(__SVR4) || defined(__svr4__)))
	#include <fcntl.h>
	#include <procfs.h>

#elif defined(__linux__) || defined(__linux) || defined(linux) || defined(__gnu_linux__)
	#include <stdio.h>
#endif

#else
#error "Cannot define getPeakRSS( ) or getCurrentRSS( ) for an unknown OS."
#endif



// http://stackoverflow.com/questions/1798511/how-to-avoid-press-enter-with-any-getchar
int WaitChar()
{
#if defined(_WIN32)
	int c = getch();
	
	fflush(stdin);
	return c;
#elif defined(__linux__) || defined(__linux) || defined(linux) || defined(__gnu_linux__)
	int c;
	static struct termios oldt, newt;
	/*tcgetattr gets the parameters of the current terminal
	STDIN_FILENO will tell tcgetattr that it should write the settings
	of stdin to oldt*/
	tcgetattr( STDIN_FILENO, &oldt);
	/*now the settings will be copied*/
	newt = oldt;

	/*ICANON normally takes care that one line at a time will be processed
	that means it will return if it sees a "\n" or an EOF or an EOL*/
	newt.c_lflag &= ~(ICANON);

	/*Those new settings will be set to STDIN
	TCSANOW tells tcsetattr to change attributes immediately. */
	tcsetattr(STDIN_FILENO, TCSANOW, &newt);

	c = getchar();

	// restore the old settings
	tcsetattr( STDIN_FILENO, TCSANOW, &oldt);

	fflush(stdin);
	return c;
#else
	// system("/bin/stty raw");
	int c;
	printf("Note: Waitchar for OS not supported, waits for ENTER-key instead.\n");
	c = getchar();
	
	fflush(stdin);
	return c;
#endif

	return 0;
}




/**
* Returns the peak (maximum so far) resident set size (physical
* memory use) measured in bytes, or zero if the value cannot be
* determined on this OS.
*/
size_t getPeakRSS()
{
#if defined(_WIN32)
	/* Windows -------------------------------------------------- */
	PROCESS_MEMORY_COUNTERS info;
	GetProcessMemoryInfo(GetCurrentProcess(), &info, sizeof(info));
	return (size_t)info.PeakWorkingSetSize;

#elif (defined(_AIX) || defined(__TOS__AIX__)) || (defined(__sun__) || defined(__sun) || defined(sun) && (defined(__SVR4) || defined(__svr4__)))
	/* AIX and Solaris ------------------------------------------ */
	struct psinfo psinfo;
	int fd = -1;
	if ((fd = open("/proc/self/psinfo", O_RDONLY)) == -1)
		return (size_t)0L;		/* Can't open? */
	if (read(fd, &psinfo, sizeof(psinfo)) != sizeof(psinfo))
	{
		close(fd);
		return (size_t)0L;		/* Can't read? */
	}
	close(fd);
	return (size_t)(psinfo.pr_rssize * 1024L);

#elif defined(__unix__) || defined(__unix) || defined(unix) || (defined(__APPLE__) && defined(__MACH__))
	/* BSD, Linux, and OSX -------------------------------------- */
	struct rusage rusage;
	getrusage(RUSAGE_SELF, &rusage);
#if defined(__APPLE__) && defined(__MACH__)
	return (size_t)rusage.ru_maxrss;
#else
	return (size_t)(rusage.ru_maxrss * 1024L);
#endif

#else
	/* Unknown OS ----------------------------------------------- */
	return (size_t)0L;			/* Unsupported. */
#endif
}



/**
* Returns the current resident set size (physical memory use) measured
* in bytes, or zero if the value cannot be determined on this OS.
*/
size_t getCurrentRSS()
{
#if defined(_WIN32)
	/* Windows -------------------------------------------------- */
	PROCESS_MEMORY_COUNTERS info;
	GetProcessMemoryInfo(GetCurrentProcess(), &info, sizeof(info));
	return (size_t)info.WorkingSetSize;

#elif defined(__APPLE__) && defined(__MACH__)
	/* OSX ------------------------------------------------------ */
	struct mach_task_basic_info info;
	mach_msg_type_number_t infoCount = MACH_TASK_BASIC_INFO_COUNT;
	if (task_info(mach_task_self(), MACH_TASK_BASIC_INFO,
		(task_info_t)&info, &infoCount) != KERN_SUCCESS)
		return (size_t)0L;		/* Can't access? */
	return (size_t)info.resident_size;

#elif defined(__linux__) || defined(__linux) || defined(linux) || defined(__gnu_linux__)
	/* Linux ---------------------------------------------------- */
	long rss = 0L;
	FILE* fp = NULL;
	if ((fp = fopen("/proc/self/statm", "r")) == NULL)
		return (size_t)0L;		/* Can't open? */
	if (fscanf(fp, "%*s%ld", &rss) != 1)
	{
		fclose(fp);
		return (size_t)0L;		/* Can't read? */
	}
	fclose(fp);
	return (size_t)rss * (size_t)sysconf(_SC_PAGESIZE);

#else
	/* AIX, BSD, Solaris, and Unknown OS ------------------------ */
	return (size_t)0L;			/* Unsupported. */
#endif
}



char* format_commas(int n, char *out)
{
	int c;
	char buf[100];
	char *p;
	char* q = out; // Backup pointer for return...

	if (n < 0)
	{
		*out++ = '-';
		n = abs(n);
	}


	snprintf(buf, 100, "%d", n);
	c = 2 - strlen(buf) % 3;

	for (p = buf; *p != 0; p++) {
		*out++ = *p;
		if (c == 1) {
			*out++ = '\'';
		}
		c = (c + 1) % 3;
	}
	*--out = 0;

	return q;
}




//int _tmain(int argc, _TCHAR* argv[])
int main(int argc, char* argv[])
{
	size_t currentSize = getCurrentRSS();
	size_t peakSize = getPeakRSS();


	printf("Current size: %d\n", currentSize);
	printf("Peak size: %d\n\n\n", peakSize);

	char* szcurrentSize = (char*)malloc(100 * sizeof(char));
	char* szpeakSize = (char*)malloc(100 * sizeof(char));

	printf("Current size (f): %s\n", format_commas((int)currentSize, szcurrentSize));
	printf("Peak size (f): %s\n", format_commas((int)currentSize, szpeakSize));

	free(szcurrentSize);
	free(szpeakSize);


	printf("\n--- Press any key to continue ---\n");
	WaitChar();

	return EXIT_SUCCESS;
}

#define _GNU_SOURCE
#include <stdio.h>
#include <stdarg.h>
#include <string.h>
#include <dlfcn.h>
#include <fcntl.h>

static int (*orig_open)(const char *pathname, int flags, ...) = NULL;

int open(const char *pathname, int flags, ...)
{
    if (orig_open == NULL)
        orig_open = dlsym(RTLD_NEXT, "open");

    if (__OPEN_NEEDS_MODE(flags))
    {
        va_list arg;
        va_start(arg, flags);
        mode_t mode = va_arg(arg, mode_t);
        va_end(arg);

        return orig_open(pathname, flags & ~O_DIRECT, mode);
    }
    else
    {
        return orig_open(pathname, flags & ~O_DIRECT);
    }
}
Update File Dll Version using msys2 mingw shell
=======================================
Here are instructions on how to update to a newer version of libmagic using msys2 on windows.

1. Download and install msys2 from https://msys2.github.io/
2. Start MinGW-w64 Win32 Shell from the start menu
3. Install dependencies
`pacman -S tar base-devel mingw32/mingw-w64-i686-gcc mingw32/mingw-w64-i686-libsystre`
4. Download the desired version of file, eg. 5.27, and extract it
`curl ftp://ftp.astron.com/pub/file/file-5.27.tar.gz -o file.tar.gz
tar xf file.tar.gz
cd file-5.27`
6. edit src/der.c
at around line 44 change from
`#include <sys/mman.h>`
 to
` #ifdef QUICK
 #include <sys/mman.h>
 #endif`
6. Build file
`./configure
make`
7. Copy the build output and dependencies to a new folder named `output`
`mkdir output
cp src/.libs/libmagic-1.dll output/fileidentifier.native.dll
cp magic/magic.mgc /mingw32/bin/libtre-5.dll /mingw32/bin/libsystre-0.dll /mingw32/bin/libgcc_s_dw2-1.dll /mingw32/bin/libwinpthread-1.dll output/`

8. open the output folder in the file explorer
`start output`

9. Copy all the files from the output folder to ExternalFiles folder in MagicWrapper, replacing all files
10. In the MagicWrapper solution
	1. remove the libgnurx dll from ExternalFiles
	2. Add all the files from ExternalFiles to the solution
	3. Set them all to Build Action: ExternalFile and Copy to Output Directory: Copy if newer
11. Since the ncmagic.list file definitions incompatible with the latest version of libmagic, don't load that file
	In Program.cs, change to
	`fileMagic = new Magic(GetPath("magic.mgc"), Magic.MAGIC_NONE | Magic.MAGIC_NO_CHECK_CDF);
	mimeMagic = new Magic(GetPath("magic.mgc"), Magic.MAGIC_MIME_TYPE | Magic.MAGIC_NO_CHECK_CDF);`


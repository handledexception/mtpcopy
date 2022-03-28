# mtpcopy

A simple dotnet core app for copying media files from an MTP device (i.e. iPhone, Android, etc.)

Uses the MediaDevices library: https://github.com/Bassman2/MediaDevices

### Usage:
```
"mtpcopy usage:\n",
"dotnet run <args 1..N>\n",
"Arguments",
"---------",
"[ -d, --device ] Specify the friendly name of the MTP device (i.e. "Apple iPhone")",
"[ -m, --dcim ] Specify the path of the DCIM directory on the MTP device (i.e. "Internal Storage\\DCIM")",
"[ -o, --output ] Specify the local output path to copy files from the MTP device (i.e. "C:\\iPhone")",
"[ -s, --search ] Specify the search filter when enumerating files on the MTP device (i.e. "*.mov|*.jpg)"",
```

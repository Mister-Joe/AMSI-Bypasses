# AMSI-Bypasses
Bypass AMSI by patching AmsiScanBuffer two different ways.

## Overview
**jmp.cs** places a mid-function hook in AmsiScanBuffer. A relative jmp instruction is used to jump to the end of the function & return 0 (AMSI_RESULT_CLEAN).

**ret.cs** places a hook at the beginning of AmsiScanBuffer to return 0 (AMSI_RESULT_CLEAN).

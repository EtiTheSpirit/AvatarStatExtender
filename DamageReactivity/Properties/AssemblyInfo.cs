using XansTools;
using MelonLoader;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a55e452e-2b5a-4b67-bfbc-8d84d3c461e2")]

[assembly: MelonInfo(typeof(XTCore), "Xan's Tools", "1.0.0", "Xan")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
[assembly: MelonPriority(1)] // Priority 1 instead of 0 so this loads late.
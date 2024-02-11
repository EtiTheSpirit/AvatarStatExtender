#if !SYSTEM_PRIVATE_CORELIB
namespace System.Diagnostics.CodeAnalysis {

	/// <summary>
	/// Specifies that <see langword="null"/> is allowed as an input even if the corresponding type disallows it.
	/// <para/>
	/// This is a reimplementation of the native attribute from C# provided via a Unity script. Visual Studio's analyzer, when
	/// nullability is enabled, simply looks for this class in this namespace. Thus, redefining it here manually works
	/// to achieve the purpose of informing the analyzer about vital context.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
	public sealed class AllowNullAttribute : Attribute { }
}
#endif
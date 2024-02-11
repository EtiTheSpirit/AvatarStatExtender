#if !SYSTEM_PRIVATE_CORELIB
namespace System.Diagnostics.CodeAnalysis {

	/// <summary>
	/// Specifies that the method or property will ensure that the listed field and property members have non-null values 
	/// when returning with the specified return value condition.
	/// <para/>
	/// This is a reimplementation of the native attribute from C# provided via a Unity script. Visual Studio's analyzer, when
	/// nullability is enabled, simply looks for this class in this namespace. Thus, redefining it here manually works
	/// to achieve the purpose of informing the analyzer about vital context.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
	public sealed class MemberNotNullWhenAttribute : Attribute {
		public bool ReturnValue { get; }

		public string[] Members { get; }

		public MemberNotNullWhenAttribute(bool returnValue, string member) {
			ReturnValue = returnValue;
			Members = new string[1] { member };
		}

		public MemberNotNullWhenAttribute(bool returnValue, params string[] members) {
			ReturnValue = returnValue;
			Members = members;
		}
	}
}
#endif
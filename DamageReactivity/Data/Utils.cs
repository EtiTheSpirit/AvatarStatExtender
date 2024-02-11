#nullable enable

using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace XansTools.Data {
	public static unsafe class Utils {

		/*
		 
		const string METHOD_PTR_FLD_NAME_PLAYER = "NativeMethodInfoPtr_OnReceivedDamage_Public_Virtual_Void_Attack_BodyPart_0";
		const string METHOD_PTR_FLD_NAME_ENEMY = "NativeMethodInfoPtr_OnReceivedDamage_Public_Void_Attack_BodyPart_0";
		 */

		[Obsolete("Not yet available.")]
		public static string TryGuessMethodNameOf<TDelegate>(TDelegate del) where TDelegate : Delegate {
			throw new NotImplementedException("This method is not yet available.");
			/*
			MethodBase method = del.Method;
			if (method == null) throw new InvalidOperationException("You must input the method as a parameter to this function.");

			string name = $"NativeMethodInfoPtr_{method.Name}_";
			if (method.Attributes.HasFlag(MethodAttributes.Public)) {
				name += "Public_";
			} else if (method.Attributes.HasFlag(MethodAttributes.Private)) {
				name += "Private_";
			} else if (method.Attributes.HasFlag(MethodAttributes.Family)) {
				// protected
			} else if (method.Attributes.HasFlag(MethodAttributes.FamANDAssem)) {
				// private protected
			} else if (method.Attributes.HasFlag(MethodAttributes.FamORAssem)) {
				// protected internal
			}
			if (method.Attributes.HasFlag(MethodAttributes.Virtual)) {
				name += "Virtual_";
			}
			*/
		}
		/// <summary>
		/// Provided with the owner of the method being patched (an il2cpp class), and a delegate type for that method's hook, this will automatically
		/// perform a native hook into said method that redirects to the desired delegate. The original method is returned.
		/// </summary>
		/// <typeparam name="TMethodOwner"></typeparam>
		/// <typeparam name="TDelegate"></typeparam>
		/// <param name="ptrFieldName"></param>
		/// <param name="detour"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static TDelegate NativeHookAttachFrom<TMethodOwner, TDelegate>(string ptrFieldName, TDelegate detour) where TDelegate : Delegate {
			if (detour == null) throw new ArgumentNullException(nameof(detour));
			if (detour.Method == null) throw new InvalidOperationException($"Parameter '{nameof(detour)}' does not have a Method?");

			ParameterInfo[] @params = detour.Method.GetParameters();
			if (@params.Any(param => !param.ParameterType.IsValueType)) throw new InvalidOperationException("For patch methods, all parameters should be value types. To receive an object type, instead receive IntPtr and then create a pointer to that object.");

			// To future Xan / coders:
			// This is janky as fuck. It's cursed. I know. It has to be this way.
			// There's a few things that require this behavior to be used (for example, a new pointer has to be used in NativeHookAttach or
			// else invoking the original method causes a stack overflow due to re-entrance).
			IntPtr tgtPtr = *(IntPtr*)(IntPtr)typeof(TMethodOwner).GetField(ptrFieldName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			IntPtr desiredPatch = detour.Method.MethodHandle.GetFunctionPointer();

			// Its this line that bothers me. Yes, the return value of that field's GetValue() call ultimately ends up at the same pointer.
			// You can't use that, though. ML uses that data table (which is where the original value resides, in the .data section of the PE)
			// to find the original method. If you override that pointer by passing it into NativeHookAttach, you *replace* the original method.
			// Thus, a new pointer is created instead, using the & operator.
			MelonUtils.NativeHookAttach((IntPtr)(&tgtPtr), desiredPatch);
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(tgtPtr);
		}
	}
}

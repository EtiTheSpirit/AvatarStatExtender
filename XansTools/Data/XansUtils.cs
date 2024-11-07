#nullable enable

using MelonLoader;
using MelonLoader.NativeUtils;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using size_t = System.IntPtr;

namespace XansTools.Data {
	public static unsafe class XansUtils {

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

			size_t tgtPtr = *(size_t*)(size_t)typeof(TMethodOwner).GetField(ptrFieldName, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
			size_t desiredPatch = detour.Method.MethodHandle.GetFunctionPointer();

			NativeHook<TDelegate> hook = new NativeHook<TDelegate>(tgtPtr, desiredPatch);
			hook.Attach();
			return hook.Trampoline;
			//MelonUtils.NativeHookAttach((size_t)(&tgtPtr), desiredPatch);
			//return Marshal.GetDelegateForFunctionPointer<TDelegate>(tgtPtr);
			/*
			CppMethod* tgtPtr = GetFunctionPointerFromField<TMethodOwner>(ptrFieldName);
			CppMethod* desiredPatch = (CppMethod*)detour.Method.MethodHandle.GetFunctionPointer();

			return NativeHookAttach<TDelegate>(tgtPtr, desiredPatch);
			*/
		}

		/*
		/// <summary>
		/// Accesses the field of the provided name, which stores a two-layer pointer (a pointer to yet another memory address).
		/// The first layer pointer is resolved to go to the other address, which itself is the address of a function (thus, a function pointer).
		/// </summary>
		/// <typeparam name="TMethodOwner"></typeparam>
		/// <param name="ptrFieldName"></param>
		/// <returns></returns>
		private static CppMethod* GetFunctionPointerFromField<TMethodOwner>(string ptrFieldName) {
			size_t ptrToFunctionPtr_Address = (size_t)typeof(TMethodOwner).GetField(ptrFieldName, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
			CppMethod** ptrToFunctionPtr = (CppMethod**)ptrToFunctionPtr_Address;
			
			return *ptrToFunctionPtr;
		}

		/// <summary>
		/// Mimics <see cref="MelonUtils.NativeHookAttach(size_t, size_t)"/> using newer API.
		/// </summary>
		/// <typeparam name="TDelegate"></typeparam>
		/// <param name="target"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		private static TDelegate NativeHookAttach<TDelegate>(CppMethod* target, CppMethod* patch) where TDelegate : Delegate {
			if (target == null) throw new ArgumentNullException(nameof(target));
			//CppMethod* buf = target;

			NativeHook<TDelegate> hook = new NativeHook<TDelegate>((size_t)target, (size_t)patch);
			hook.Attach();
			return @delegate.Trampoline;
		}
		*/

		/// <summary>
		/// Quickly accesses a method from the provided type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MethodInfo QuickGetMethod<T>(string name) {
			return typeof(T).GetMethod(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
		}

		/// <summary>
		/// A purposeful zero-size struct as a replacement to <see cref="void"/>.
		/// <para/>
		/// The entire purpose of this struct is mnemonic - it helps with code readability,
		/// as <c><see cref="CppMethod"/>*</c> makes more evident sense than <c><see cref="void"/>*</c>
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Size = 0)]
		private readonly ref struct CppMethod { }

	}
}

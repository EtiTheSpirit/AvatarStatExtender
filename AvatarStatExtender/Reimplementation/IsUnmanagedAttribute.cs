﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !SYSTEM_PRIVATE_CORELIB
using System.ComponentModel;
namespace System.Runtime.CompilerServices {
	/// <summary>
	/// Reserved for use by a compiler for tracking metadata.
	/// This attribute should not be used by developers in source code.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[AttributeUsage(AttributeTargets.All)]
	public sealed class IsUnmanagedAttribute : Attribute {
		/// <summary>Initializes the attribute.</summary>
		public IsUnmanagedAttribute() { }
	}
}
#endif
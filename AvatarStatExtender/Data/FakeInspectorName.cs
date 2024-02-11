#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !UNITY_EDITOR
namespace AvatarStatExtender.Data {

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class InspectorNameAttribute : Attribute {

		public readonly string name;

		public InspectorNameAttribute(string name) {
			this.name = name;
		}

	}
}
#endif
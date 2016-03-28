using System;

namespace K2Field.SmartForms.Authorization
{
	[Flags]
	public enum ResourceTypes
	{
		None = 0,
		View = 1,
		Form = 2,
		Category = 3,

		All = -1
	}
}

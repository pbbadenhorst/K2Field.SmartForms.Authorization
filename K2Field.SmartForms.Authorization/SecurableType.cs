using System;

namespace K2Field.SmartForms.Authorization
{
	[Flags]
	public enum ResourceTypes
	{
		None = 0,
		View = 1,
		Form = 2,
		//SmartObject = 4,
		//Category = 8,

		All = -1
	}
}

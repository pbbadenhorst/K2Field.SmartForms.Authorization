using System;

namespace K2Field.SmartForms.Authorization
{
    #region Permission Type Enum

    [Flags]
    public enum PermissionType
	{
		View = 1,
		Create = 2,
		Edit = 3,
        Delete = 4,
        Full = -1
	}

    #endregion
}
using System;

namespace K2Field.SmartForms.Authorization
{
    #region Securable Type Enum

    [Flags]
	public enum SecurableType
	{
		View = 1,
		Form = 2,
		FormView = -1
    }

    #endregion
}

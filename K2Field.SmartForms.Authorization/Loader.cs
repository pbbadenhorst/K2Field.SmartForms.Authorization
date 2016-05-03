
namespace K2Field.SmartForms.Authorization
{
	using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    /// <summary>
    /// Represents a dynamic registrator of the Authorization Module as an HTTP Module.
    /// </summary>
    public class Loader
	{
        #region Methods

        #region Load Module

        /// <summary>
        /// Dynamically registers the Auhtorization Module assembly as a custom HTTP Module.
        /// </summary>
        public static void LoadModule()
		{
			DynamicModuleUtility.RegisterModule(typeof(AuthorizationModule));
		}

        #endregion

        #endregion
    }
}

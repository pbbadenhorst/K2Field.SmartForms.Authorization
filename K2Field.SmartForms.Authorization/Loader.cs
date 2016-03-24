
namespace K2Field.SmartForms.Authorization
{
	using Microsoft.Web.Infrastructure.DynamicModuleHelper;

	/// <summary>
	/// Representn
	/// </summary>
	public class Loader
	{
		public static void LoadModule()
		{
			DynamicModuleUtility.RegisterModule(typeof(AuthorizationModule));
		}
	}
}

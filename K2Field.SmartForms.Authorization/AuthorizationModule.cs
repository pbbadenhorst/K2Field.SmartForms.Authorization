
#if RELEASE
// When running in release mode the assembly will load itself.
// To debug the load process compile the dll in DEBUG and add the module to the <modules> section of the web.config (as below)
// The IHttpModule will call the Loader.LoadModule when compiled in debug mode.
//  <configuration>
//   <!-- snip -->
//   <system.webServer>
//    <!-- snip -->
//    <modules>
//     <!-- snip -->
//    
//     <!-- Field Authorization module -->
//     <add name = "FieldAuthorizationModule" type="K2Field.SmartForms.Authorization.SecurityModule, K2Field.SmartForms.Authorization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=74c3737efa394b02" />
//    </modules>
//    <!-- snip -->
//   <//system.webServer>
//   <!-- snip -->
//  </configuration>
[assembly: System.Web.PreApplicationStartMethod(typeof(K2Field.SmartForms.Authorization.Loader), "LoadModule")]
#endif

namespace K2Field.SmartForms.Authorization
{
	using SourceCode.Forms.AppFramework;
	using System;
	using System.Configuration;
	using System.IO;
	using System.Web;

	public class AuthorizationModule : IHttpModule
	{
		#region Fields

		private static string _formRuntimeUrl;
		private static string _viewRuntimeUrl;
		//private static string _ajaxRuntimeUrl;
		private static string _notAuthorizedUrl;
		#endregion

		#region Properties

		internal static AuthorizationRuleCollection AuthorizationRules
		{
			get
			{
				return AuthorizationRuleProvider.GetRules();
			}
		}

		internal static IAuthorizationRuleProvider AuthorizationRuleProvider { get; }

		#endregion

		#region Constructors

		static AuthorizationModule()
		{
			Log("Info", "Initializing authorization module...");

			_formRuntimeUrl = (ConfigurationManager.AppSettings["FormRuntimeUrl"] ?? "~/Runtime/Form.aspx").Replace("~", HttpRuntime.AppDomainAppVirtualPath);
			_viewRuntimeUrl = (ConfigurationManager.AppSettings["ViewRuntimeUrl"] ?? "~/Runtime/View.aspx").Replace("~", HttpRuntime.AppDomainAppVirtualPath);

			//_ajaxRuntimeUrl = ("~/Runtime/AJAXCall.ashx").Replace("~", HttpRuntime.AppDomainAppVirtualPath);
			_notAuthorizedUrl = (ConfigurationManager.AppSettings["UnauthorisedAccessPath"] ?? "~/NotAuthorised.aspx").Replace("~", HttpRuntime.AppDomainAppVirtualPath);


			//var typeName = (ConfigurationManager.AppSettings["AuthorizationProvider"] ?? "K2Field.SmartForms.Authorization.ConfigurationRuleProvider");
			var typeName = (ConfigurationManager.AppSettings["AuthorizationProvider"] ?? "K2Field.SmartForms.Authorization.SmartObjectRuleProvider");

			Log("Info", "Creating authorization rule provider: {0}", typeName);

			var type = Type.GetType(typeName);
			AuthorizationRuleProvider = Activator.CreateInstance(type) as IAuthorizationRuleProvider;
		}

		#endregion

		#region IHttpModule Members

		public void Dispose()
		{
		}

		public void Init(HttpApplication context)
		{
			Log("Info", "Registering for ASP.net pipeline events...");
			context.AuthorizeRequest += OnAuthorizeRequest;
		}

		#endregion

		#region Event Handlers

		private void OnAuthorizeRequest(object sender, EventArgs e)
		{
			// Initialize variables
			var request = (sender as HttpApplication)?.Context?.Request;
			var fqn = string.Empty;
			try { fqn = ConnectionClass.GetCurrentUser(); } catch (Exception ex) { fqn = ex.Message; }
			var url = string.Empty;
			try { url = request.RawUrl; } catch (Exception ex) { url = ex.Message; }

			// Check if authorized
			if (!IsAuthorized(request))
			{
				// Log
				Log("Warning", "Access UNAUTHORIZED, fqn={1}, url={0}, ", url, fqn);

				// Redirect
				HttpContext.Current.Response.Redirect(_notAuthorizedUrl, true);
			}
			else
			{
				Log("Debug", "Access authorized, fqn={1}, url={0}", url, fqn);
			}
		}

		#endregion

		#region Helpers

		private static bool IsAuthorized(HttpRequest request)
		{
			// Argument checking
			if (request == null) throw new ArgumentNullException("request");

			string guidString;
			string url = request.FilePath;
			string fqn = ConnectionClass.GetCurrentUser();

			if (!string.IsNullOrEmpty(url))
			{
				// Dont need to check vanity urls because the UrlWriter module already rewrote them to the actual urls
				// Need to check both QueryString and Form variables because runtime supports both

				#region Check for anonymous forms and views

				var isFormUrl = url.Equals(_formRuntimeUrl, StringComparison.OrdinalIgnoreCase);
				var isViewUrl = url.Equals(_viewRuntimeUrl, StringComparison.OrdinalIgnoreCase);

				if (isFormUrl || isViewUrl)
				{
					bool isAuthorized = false;
					string name = request.QueryString["_Name"];
					if (!string.IsNullOrEmpty(name))
					{
						isAuthorized = isFormUrl
							? IsAuthorized(fqn, SecurableTypes.Form, name)
							: IsAuthorized(fqn, SecurableTypes.View, name);
						Log("Debug", "Checking '{0}' for '{1}'", url, fqn);
					}
					else
					{
						guidString = request.QueryString["_ID"];
						Guid guid;
						if (!string.IsNullOrEmpty(guidString) && Guid.TryParse(guidString, out guid))
						{
							isAuthorized = isFormUrl
								? IsAuthorized(fqn, SecurableTypes.Form, guid)
								: IsAuthorized(fqn, SecurableTypes.View, guid);

							if (isAuthorized)
								Log("Info", "Authorization success. User='{0}', Type={1}, ID={2}", url, isFormUrl ? "Form" : "View", fqn);
							else
								Log("Warning", "Authorization FAILED. User='{0}', Type={1}, ID={2}", url, isFormUrl ? "Form" : "View", fqn); 
						}
					}
					return isAuthorized;
				}

				#endregion

				//#region Check for Runtime AJAX handler

				//var isAjaxUrl = url.Equals(_ajaxRuntimeUrl, StringComparison.OrdinalIgnoreCase);

				//if (isAjaxUrl)
				//{
				//	// Get the current View/Form guid from the token and check
				//	// if the anonymous cache contains the SMO items for it
				//	Guid currentGuid = new Guid(guidString);
				//	string anonItemKey = guidString + "_" + state;
				//	if (!AnonymousSMOs.ContainsKey(anonItemKey))
				//	{
				//		// If the AnonymousSMOs does not contain the form or view Guid due to Appool recycle etc, we try get them from the actual Form/view 
				//		UpdateAnonymousSMOsFromFormOrView(currentGuid, tokenType, state);
				//	}

				//	if (AnonymousSMOs[anonItemKey] == null)
				//	{
				//		return false;
				//	}
				//	return true;
				//}

				//#endregion
			}

			// Authorized by default
			return true;
		}

		private static bool IsAuthorized(string fqn, SecurableTypes type, string name)
		{
			var rules = AuthorizationRuleProvider.GetRules();
			var permissionType = default(PermissionType);
			var identities = new string[] { fqn };

			foreach (var rule in rules)
			{
				if (rule.TryApply(identities, type, name, out permissionType))
				{
				}
			}

			if (name.Equals("Restricted", StringComparison.OrdinalIgnoreCase))
				return false;
			else
				return true;
		}

		private static bool IsAuthorized(string fqn, SecurableTypes type, Guid guid)
		{
			return true;
		}

		#endregion

		#region Logging

		private static object _sync = new object();
		public static string FilePath { get; } = string.Format(@"C:\Debug\K2Field.Authorization.{0}.log", DateTime.Now.ToString("yyyy-MM-dd HHmmss"));

		public static void Log(string category, string message, params object[] arguments)
		{
			var text = string.Format("{0}\t[{1}]\t{2}", DateTime.Now.ToString("o"), category, string.Format(message, arguments));
			lock (_sync)
			{
				using (var stream = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.Write))
				{
					using (var writer = new StreamWriter(stream))
					{
						writer.WriteLine(text);
					}
				}
			}
		}

		#endregion
	}
}

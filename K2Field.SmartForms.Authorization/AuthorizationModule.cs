
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
//     <add name = "AuthorizationModule" type="K2Field.SmartForms.Authorization.AuthorizationModule, K2Field.SmartForms.Authorization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=74c3737efa394b02" />
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
    using System.Runtime.Caching;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;

    public class AuthorizationModule : IHttpModule
    {
        #region Fields

        private static string _formRuntimeUrl;
        private static string _viewRuntimeUrl;
        private static string _notAuthorizedUrl;
        //private static string _ajaxRuntimeUrl;

        private static string _logOutputFolder;
        private static bool _enableLogging;
        private static bool _writeToEventLog;

        private static PermissionType _requestedAccess;

        private static object _logSync = new object();
        private static object _cacheSync = new object();

        private static readonly string _ruleCacheName = "AuthRuleCache";
        private static readonly string _userAuthHistoryCacheName = "_Hist";

        private static int _cacheRefreshInterval = 8;


        #endregion

        #region Properties

        internal AuthorizationRuleCollection AuthorizationRules
        {
            get
            {
                return RuleProvider.GetRules(_enableLogging, FilePath, ref _logSync);
            }
        }

        internal static Interfaces.IAuthorizationIdentityResolver IdentityResolver { get; }

        internal static Interfaces.IAuthorizationRuleProvider RuleProvider { get; }

        public static string FilePath
        {
            get
            {
                return (string.Format(@"{0}\AuthorizationModule {1}.log", _logOutputFolder, DateTime.Now.ToString("yyyy-MM-dd HHtt").ToLower()));
            }
        }

        #endregion

        #region Constructors

        static AuthorizationModule()
        {
            _formRuntimeUrl = (ConfigurationManager.AppSettings["FormRuntimeUrl"] ?? "~/Runtime/Form.aspx").Replace("~", HttpRuntime.AppDomainAppVirtualPath);
            _viewRuntimeUrl = (ConfigurationManager.AppSettings["ViewRuntimeUrl"] ?? "~/Runtime/View.aspx").Replace("~", HttpRuntime.AppDomainAppVirtualPath);
            _notAuthorizedUrl = (ConfigurationManager.AppSettings["UnauthorisedAccessPath"] ?? "~/NotAuthorised.aspx").Replace("~", HttpRuntime.AppDomainAppVirtualPath);
            //_ajaxRuntimeUrl = ("~/Runtime/AJAXCall.ashx").Replace("~", HttpRuntime.AppDomainAppVirtualPath);

            _logOutputFolder = (ConfigurationManager.AppSettings["AuthorizationModule.LogOutputFolder"] ?? "C:\\Debug");
            if (_logOutputFolder.EndsWith("\\") == true)
            {
                _logOutputFolder = _logOutputFolder.Substring(0, _logOutputFolder.Length - 1);
            }

            _enableLogging = bool.Parse((ConfigurationManager.AppSettings["AuthorizationModule.EnableLogging"] ?? "true"));
            _writeToEventLog = bool.Parse((ConfigurationManager.AppSettings["AuthorizationModule.WriteToEventLog"] ?? "true"));

            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "Constructor", "Info", "Initializing authorization module...");

            _cacheRefreshInterval = int.Parse((ConfigurationManager.AppSettings["AuthorizationModule.CacheRefreshInterval"] ?? "600"));
            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "Constructor", "Info", "Cache refresh interval (Seconds): " + _cacheRefreshInterval.ToString());

            var ruleProviderType = (ConfigurationManager.AppSettings["AuthorizationModule.RuleProvider"] ?? "K2Field.SmartForms.Authorization.SmartObjectRuleProvider");
            //var ruleProviderType = (ConfigurationManager.AppSettings["AuthorizationModule.RuleProvider"] ?? "K2Field.SmartForms.Authorization.ConfigurationRuleProvider");
            var type = Type.GetType(ruleProviderType);
            RuleProvider = Activator.CreateInstance(type) as Interfaces.IAuthorizationRuleProvider;
            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "Constructor", "Info", "Created authorization rule provider: " + ruleProviderType.ToString());

            var identityProviderType = (ConfigurationManager.AppSettings["AuthorizationModule.IdentityProvider"] ?? "K2Field.SmartForms.Authorization.AuthorizationIdentityResolver");
            type = Type.GetType(identityProviderType);
            IdentityResolver = Activator.CreateInstance(type) as Interfaces.IAuthorizationIdentityResolver;
            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "Constructor", "Info", "Created identity provider: " + identityProviderType.ToString());
        }

        #endregion

        #region IHttpModule Members

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.AuthorizeRequest += OnAuthorizeRequest;
        }

        #endregion

        #region Event Handlers

        #region On Authorize Request

        private void OnAuthorizeRequest(object sender, EventArgs e)
        {
            // Initialize variables
            string userFQN = string.Empty;
            string requestedSecurableURL = string.Empty;

            try
            {
                var request = (sender as HttpApplication)?.Context?.Request;

                userFQN = ConnectionClass.GetCurrentUser();
                requestedSecurableURL = request.RawUrl;
                //Helpers.Logfile.Log(_enableLogging, FilePath, ref _sync, "AuthorizationModule", "OnAuthorizeRequest", "Info", "Requested URL: " + requestedSecurableURL);

                if (requestedSecurableURL.Equals(_notAuthorizedUrl, StringComparison.OrdinalIgnoreCase) == false)
                {
                    // Check if authorized
                    if (!IsAuthorized(request))
                    {
                        // Log
                        Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "OnAuthorizeRequest", "Warning", "Authorization FAILED");

                        if (_writeToEventLog == true)
                        {
                            using (EventLog eventLog = new EventLog("Application"))
                            {
                                eventLog.Source = "SourceCode.Logging.Extension.EventLogExtension";
                                eventLog.WriteEntry("AUTHENTICATION FAILED. User " + userFQN + " requested access SmartForms resource \"" + requestedSecurableURL + "\" and access was denied", EventLogEntryType.Warning, 1, 1);
                            }
                        }

                        try
                        {
                            // Redirect
                            HttpContext.Current.Response.Redirect(_notAuthorizedUrl, true);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().ToString().Equals("System.Threading.ThreadAbortException", StringComparison.OrdinalIgnoreCase) == false)
                {
                    Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "OnAuthorizeRequest", "Error", "Exception occured: " + ex.ToString());
                    throw;
                }
            }
        }

        #endregion

        #endregion

        #region Helpers

        #region Is Authorized

        private static bool IsAuthorized(HttpRequest request)
        {
            bool isAuthorized = false;
            bool isCached = false;
            string guidString = string.Empty;
            string requestedSecurableURL = string.Empty;
            string rawRequestedURL = string.Empty;
            SecurableType requestedSecurableType = SecurableType.Form;
            string userFQN = string.Empty;
            string userAuthCacheName = string.Empty;
            string requestedSecurableName = string.Empty;

            List<AuthorizationResult> userAuthorizations = null;

            try
            {
                rawRequestedURL = request.RawUrl;
                requestedSecurableURL = request.FilePath;
                userFQN = ConnectionClass.GetCurrentUser();

                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (!string.IsNullOrEmpty(requestedSecurableURL))
                {
                    // Dont need to check vanity urls because the UrlWriter module already rewrote them to the actual urls
                    // Need to check both QueryString and Form variables because runtime supports both

                    #region Check for anonymous forms and views

                    var isFormUrl = requestedSecurableURL.Equals(_formRuntimeUrl, StringComparison.OrdinalIgnoreCase);
                    var isViewUrl = requestedSecurableURL.Equals(_viewRuntimeUrl, StringComparison.OrdinalIgnoreCase);

                    if (isFormUrl || isViewUrl)
                    {
                        Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Requested URL: " + rawRequestedURL);
                        Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "User FQN: " + userFQN);

                        if (isViewUrl == true)
                        {
                            requestedSecurableType = SecurableType.View;
                            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Securable is a View");
                        }

                        if (isFormUrl == true)
                        {
                            requestedSecurableType = SecurableType.Form;
                            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Securable is a Form");
                        }

                        userAuthCacheName = userFQN.ToUpper() + _userAuthHistoryCacheName.ToUpper();

                        // Based on the URL we are setting the requested access to View
                        _requestedAccess = PermissionType.View;
                        requestedSecurableName = request.QueryString["_Name"];

                        if (string.IsNullOrEmpty(requestedSecurableName) == true)
                        {
                            guidString = request.QueryString["_ID"];
                            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Requested securable GUID : " + guidString);
                            Guid requestedSecurableGUID;

                            if (!string.IsNullOrEmpty(guidString) && Guid.TryParse(guidString, out requestedSecurableGUID))
                            {
                                requestedSecurableName = GetSecurableName(requestedSecurableType, requestedSecurableGUID);
                            }
                            else
                            {
                                throw new Exception("Unable to determine name of securable");
                            }
                        }

                        Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Requested securable name : " + requestedSecurableName);

                        if (MemoryCache.Default.Get(userAuthCacheName) != null)
                        {
                            userAuthorizations = (List<AuthorizationResult>)MemoryCache.Default.Get(userAuthCacheName);
                            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Loaded " + userAuthorizations.Count().ToString() + " authorization results from cache");

                            var authorizationResult = (from ar in userAuthorizations
                                                       where ((ar.SecurableName.ToLower().Equals(requestedSecurableName.ToLower(), StringComparison.OrdinalIgnoreCase)) && (ar.SecurableType == requestedSecurableType))
                                                       select ar).FirstOrDefault();

                            if (authorizationResult != null)
                            {
                                Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Cached authentication for requested securable was found");

                                lock (_cacheSync)
                                {
                                    var index = userAuthorizations.IndexOf(authorizationResult);
                                    isAuthorized = userAuthorizations[index].Authorized;
                                    userAuthorizations[index].Timestamp = DateTime.Now;
                                    userAuthorizations.RemoveAll(ar => ar.Timestamp > DateTime.Now.AddSeconds(_cacheRefreshInterval));
                                    MemoryCache.Default.Remove(userAuthCacheName);
                                    MemoryCache.Default.Add(userAuthCacheName, userAuthorizations, DateTime.Now.AddSeconds(_cacheRefreshInterval));
                                }

                                isCached = true;

                                Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Cache sanitized and updated");
                            }
                        }

                        if (isCached == false)
                        {
                            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Cached authentication for requested securable was not found");
                            isAuthorized = IsAuthorizedByName(userFQN, requestedSecurableType, requestedSecurableName, _requestedAccess);

                            lock (_cacheSync)
                            {
                                if (MemoryCache.Default.Get(userAuthCacheName) != null)
                                {
                                    userAuthorizations = (List<AuthorizationResult>)MemoryCache.Default.Get(userAuthCacheName);
                                    userAuthorizations.Add(new AuthorizationResult(requestedSecurableName.ToLower(), requestedSecurableType, isAuthorized, DateTime.Now));
                                    userAuthorizations.RemoveAll(ar => ar.Timestamp > DateTime.Now.AddSeconds(_cacheRefreshInterval));
                                    MemoryCache.Default.Remove(userAuthCacheName);
                                    Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Added new authentication result to cache for user");
                                }
                                else
                                {
                                    userAuthorizations = new List<AuthorizationResult>();
                                    userAuthorizations.Add(new AuthorizationResult(requestedSecurableName.ToLower(), requestedSecurableType, isAuthorized, DateTime.Now));

                                    Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Created new authorization cache for user");
                                }

                                MemoryCache.Default.Add(userAuthCacheName, userAuthorizations, DateTime.Now.AddSeconds(_cacheRefreshInterval));
                            }

                            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Cache sanitized and updated");
                        }

                        if (isAuthorized == true)
                        {
                            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Info", "Authorization SUCCEEDED");
                        }
                    }
                    else
                    {
                        isAuthorized = true;
                    }

                    #endregion

                    #region Check for Runtime AJAX handler

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

                    #endregion
                }
            }
            catch (Exception ex)
            {
                Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorized", "Error", "Exception occured: " + ex.ToString());
                throw;
            }

            return isAuthorized;
        }

        #endregion

        #region Is Authorized By Name

        private static bool IsAuthorizedByName(string userFQN, SecurableType requestedSecurableType, string requestedSecurableName, PermissionType requestedAccess)
        {
            bool isAuthorized = false;

            try
            {
                var rules = GetAuthorizationRules();
                var identities = IdentityResolver.GetIdentities(_enableLogging, FilePath, ref _logSync, userFQN);

                isAuthorized = rules.IsAuthorized(_enableLogging, FilePath, ref _logSync, requestedSecurableName, requestedSecurableType, identities, requestedAccess);
            }
            catch (Exception ex)
            {
                Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "IsAuthorizedByName", "Error", "Exception occured: " + ex.ToString());
                throw;
            }

            return isAuthorized;
        }

        #endregion

        #region Get Securable Name

        private static string GetSecurableName(SecurableType requestedSecurableType, Guid requestedSecurableGUID)
        {
            string securableName = string.Empty;

            try
            {
                var asAppPool = ConnectionClass.ConnectAsAppPool;

                if (!asAppPool)
                {
                    ConnectionClass.ConnectAsAppPool = true;
                }

                var client = ConnectionClass.GetFormsClient();
                if (!asAppPool)
                {
                    ConnectionClass.ConnectAsAppPool = false;
                }

                switch (requestedSecurableType)
                {
                    case SecurableType.View:
                        {
                            // Uses internal caches
                            var viewDetails = client.GetView(requestedSecurableGUID);
                            if (viewDetails != null)
                            {
                                securableName = viewDetails.Name;
                            }

                            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "GetSecurableName", "Info", "View name: " + securableName);
                        }
                        break;
                    case SecurableType.Form:
                        {
                            // Uses internal caches
                            var formDetails = client.GetForm(requestedSecurableGUID);
                            if (formDetails != null)
                            {
                                securableName = formDetails.Name;
                            }

                            Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "GetSecurableName", "Info", "Form name: " + securableName);
                        }
                        break;
                    default:
                        throw new NotSupportedException(requestedSecurableType.ToString());
                }
            }
            catch (Exception ex)
            {
                Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "GetSecurableName", "Error", "Exception occured: " + ex.ToString());
                throw;
            }

            return securableName;
        }

        #endregion

        #region Get Authorization Rules

        private static AuthorizationRuleCollection GetAuthorizationRules()
        {
            AuthorizationRuleCollection ruleStore = null;
            DateTime cacheExperiration;

            try
            {

                if (MemoryCache.Default.Get(_ruleCacheName) == null)
                {
                    cacheExperiration = DateTime.Now.AddSeconds(_cacheRefreshInterval);
                    Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "GetAuthorizationRules", "Info", "Authenticaiton rule cache is expired or empty. Reloading cache...");
                    ruleStore = RuleProvider.GetRules(_enableLogging, FilePath, ref _logSync);
                    MemoryCache.Default.Add(_ruleCacheName, ruleStore, cacheExperiration);
                    Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "GetAuthorizationRules", "Info", "Authenticaiton rule cache reloaded successfully. Cache expires at " + cacheExperiration.ToString());
                }
                else
                {
                    ruleStore = (AuthorizationRuleCollection)MemoryCache.Default.Get(_ruleCacheName);
                    Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "GetAuthorizationRules", "Info", "Authenticaiton rule loaded from cache");
                }

            }
            catch (Exception ex)
            {
                Helpers.Logfile.Log(_enableLogging, FilePath, ref _logSync, "AuthorizationModule", "GetAuthorizationRules", "Error", "Exception occured: " + ex.ToString());
                throw;
            }

            return ruleStore;
        }
        #endregion

        #endregion
    }
}
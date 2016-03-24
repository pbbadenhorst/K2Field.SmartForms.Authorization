using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using web = System.Web;
using log = K2Field.SmartForms.Authorization.Helpers.Logfile;
using k2urm = SourceCode.Security.UserRoleManager.Client;
using intf = SourceCode.Hosting.Server.Interfaces;
using cache = System.Runtime.Caching;
using sql = System.Data.SqlClient;

namespace K2Field.SmartForms.Authorization
{
    public class AccessControl : web.IHttpModule
    {
        private string _logfilePath = @"C:\temp\SFAccessControl.log";
        private string _k2ConnectionString = "Integrated=True;IsPrimaryLogin=True;Authenticate=True;EncryptedPassword=False;Host=k2.denallix.com;Port=5555";
        private string _dbConnectionString = "Data Source=DLX;Initial Catalog=7Vendors;Integrated Security=sspi;Pooling=True";

        private string _securityLabelName = "K2";
        private string _accessDeniedFormURL = "Common.AccessDenied";
        private string _accessControlStoredProcedure = "vnd.sp_GetFormGroupAccessList";
        private static readonly string _cacheName = "SFFormGroupAccess";
        private bool _enableLogging = true;
        private int _cacheRefreshInterval = 8;

        #region Constructors

        public AccessControl()
        {

        }

        #endregion

        #region Event Handlers

        public void Init(web.HttpApplication application)
        {
            if (cache.MemoryCache.Default.Get(_cacheName) == null)
            {
                this.RefreshCache();
            }

            application.PostAuthenticateRequest += (new EventHandler(this.Application_PostAuthenticateRequest));
        }

        public void Dispose() { }

        #endregion

        #region Private Methods

        #region Application - Post Authenticate Request

        private void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            web.HttpApplication application = null;
            web.HttpContext context = null;

            k2urm.UserRoleManagerServer urmServer = null;
            k2urm.GroupCollection groups = null;

            Dictionary<string, object> formGroupAccessConfig = null;
            List<string> formGroups = null;
            string username = string.Empty;
            string form = string.Empty;
            bool authorized = false;

            try
            {
                application = (web.HttpApplication)sender;
                context = application.Context;

                if ((context.Request.Url.PathAndQuery.Contains("/Form.aspx") == false) || (context.Request.Url.PathAndQuery.Contains("Common.AccessDenied") == true))
                {
                    //log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "Access control not required");
                    return;
                }

                log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "Current user: " + context.User.Identity.Name + " [" + context.User.Identity.AuthenticationType + "]");
                log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "Requested URL: " + context.Request.Url.PathAndQuery);
                username = context.User.Identity.Name;

                form = context.Request.Url.PathAndQuery.Replace(@"/Runtime/Runtime/Form.aspx?_Name=", string.Empty);
                if (form.Contains("&") == true)
                {
                    form = form.Substring(0, form.IndexOf(@"&")).Trim();
                }
                log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "Requested form: " + form);

                if (cache.MemoryCache.Default.Get(_cacheName) == null)
                {
                    this.RefreshCache();
                }

                formGroupAccessConfig = (Dictionary<string, object>)cache.MemoryCache.Default.Get(_cacheName);

                if (formGroupAccessConfig.Keys.Contains(form.Trim().ToUpper()) == false)
                {
                    log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "No access control defined for form [" + form + "]");
                    return;
                }
                else
                {
                    formGroups = (List<string>)formGroupAccessConfig[form.Trim().ToUpper()];
                    log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "Number of groups access to this form: " + formGroups.Count.ToString());
                }

                urmServer = new k2urm.UserRoleManagerServer();
                urmServer.CreateConnection();
                urmServer.Connection.Open(this._k2ConnectionString);
                log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "Connected to URM Server: " + urmServer.Connection.Host);

                groups = (k2urm.GroupCollection)urmServer.FindGroups(username, new Dictionary<string, object>(), this._securityLabelName);
                log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "Received user groups from URM");

                foreach (SourceCode.Hosting.Server.Interfaces.IGroup group in groups)
                {
                    log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "Checking group [" + group.GroupName + "]");

                    if (formGroups.Contains(group.GroupName.Trim().ToUpper()) == true)
                    {
                        log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "User is AUHTORIZED");
                        authorized = true;
                        break;
                    }
                }

                if (authorized == false)
                {
                    log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "Application_PostAuthenticateRequest", "User is NOT AUHTORIZED");
                    context.Response.Redirect("/Runtime/Runtime/Form/" + _accessDeniedFormURL + "/");
                }
                else
                {
                   
                }
            }
            catch (Exception ex)
            {
                log.WriteToLog(this._enableLogging, this._logfilePath, "ERROR", "Application_PostAuthenticateRequest", ex.ToString());
                throw;
            }
            finally
            {
                log.WriteToLog(this._enableLogging, this._logfilePath, "###########", "##################################################", "######################################################################################################################################################");
                if (urmServer != null)
                {
                    urmServer.Connection.Close();
                    urmServer.Connection.Dispose();
                    urmServer = null;
                }

                formGroupAccessConfig = null;
                formGroups = null;
            }
        }

        #endregion

        #region Refresh Cache

        private void RefreshCache()
        {
            sql.SqlConnection sqlConn = null;
            sql.SqlCommand sqlComm = null;
            sql.SqlDataReader sqlReader = null;

            Dictionary<string, object> formGroupAccessConfig = null;
            List<string> groups = null;
            string currentFormName = string.Empty;

            try
            {
                log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "RefreshCache", "Start refreshing cache...");
                formGroupAccessConfig = new Dictionary<string, object>();

                sqlConn = new sql.SqlConnection();
                sqlConn.ConnectionString = this._dbConnectionString;
                sqlConn.Open();
                log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "RefreshCache", "Connected to database: " + sqlConn.Database + " on server " + sqlConn.DataSource);

                sqlComm = new sql.SqlCommand(this._accessControlStoredProcedure, sqlConn);
                sqlComm.CommandType = System.Data.CommandType.StoredProcedure;
                sqlReader = sqlComm.ExecuteReader(System.Data.CommandBehavior.Default);

                if (sqlReader.HasRows == true)
                {
                    while (sqlReader.Read() == true)
                    {
                        if (string.IsNullOrEmpty(currentFormName) == true)
                        {
                            currentFormName = sqlReader["FormName"].ToString().Trim().ToUpper();
                            groups = new List<string>();
                            groups.Add(sqlReader["GroupName"].ToString().Trim().ToUpper());
                        }
                        else if (currentFormName.Equals(sqlReader["FormName"].ToString().Trim().ToUpper(), StringComparison.InvariantCulture) == true)
                        {
                            groups.Add(sqlReader["GroupName"].ToString().Trim().ToUpper());
                        }
                        else
                        {
                            formGroupAccessConfig.Add(currentFormName, groups);
                            currentFormName = sqlReader["FormName"].ToString().Trim().ToUpper();
                            groups = new List<string>();
                            groups.Add(sqlReader["GroupName"].ToString().Trim().ToUpper());
                        }

                        log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "RefreshCache", "Adding form [" + sqlReader["FormName"].ToString() + "] and group [" + sqlReader["GroupName"].ToString() + "] to cache");
                    }
                }
                else
                {
                    log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "RefreshCache", "Stored procedure [sp_GetFormGroupAccessList] returned no results");
                }

                cache.MemoryCache.Default.Add(_cacheName, formGroupAccessConfig, DateTime.Now.AddHours(this._cacheRefreshInterval));
                log.WriteToLog(this._enableLogging, this._logfilePath, "DEBUG", "RefreshCache", "Cache updated successfully");
            }
            catch (Exception ex)
            {
                log.WriteToLog(this._enableLogging, this._logfilePath, "ERROR", "RefreshCache", ex.ToString());
                throw;
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                    sqlConn.Dispose();
                    sqlConn = null;
                }

                if (sqlComm != null)
                {
                    sqlComm.Dispose();
                    sqlComm = null;
                }

                if (sqlReader != null)
                {
                    sqlReader.Close();
                    sqlReader.Dispose();
                    sqlReader = null;
                }
            }
        }

        #endregion

        #endregion
    }
}

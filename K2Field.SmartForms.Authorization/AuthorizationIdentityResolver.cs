using SourceCode.Forms.AppFramework;
using System.Configuration;
using System.Collections.Generic;
using System;

namespace K2Field.SmartForms.Authorization
{
    /// <summary>
    /// Represents a K2 SmartObject based user identity resolver implementing the <see cref="IAuthorizationIdentityResolver"/> interface.
    /// </summary>
	public class AuthorizationIdentityResolver : Interfaces.IAuthorizationIdentityResolver
	{
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationIdentityResolver"/> class.
        /// </summary>
        public AuthorizationIdentityResolver()
        {
            IdentitySmartObjectName = ConfigurationManager.AppSettings["AuthorizationModule.SmartObjectName"] ?? "AM_UserIdentity";
            IdentitySmartObjectMethod = ConfigurationManager.AppSettings["AuthorizationModule.SmartObjectMethod"] ?? "Get_User_Groups";
        }

        #endregion

        #region Properties

        private string IdentitySmartObjectName { get; }
        private string IdentitySmartObjectMethod { get; }
        private string SecurityLabel { get; }

        #endregion

        #region Methods

        #region Get Identities

        /// <summary>
        /// Gets a collection of identities obtained from K2 Identity Service for a provided user fully-qualified name.
        /// </summary>
        /// <param name="enableLogging">A flag to indicate whether or not logging has been enabled.</param>
        /// <param name="filePath">The path where the log file resides, or should be created.</param>
        /// <param name="logSync">The thread synchronization object for writting to the log file.</param>
        /// <param name="userFQN">The fully-qualified name of the user as defined in K2 Identity Service.</param>
        /// <returns>Returns an arry of string values representing the identies cached for the user in the K2 Identity Service.</returns>
        public string[] GetIdentities(bool enableLogging, string filePath, ref object logSync, string userFQN)
		{
            var original = ConnectionClass.ConnectAsAppPool;

            try
            {
                if (userFQN.Contains(":") == false)
                {
                    throw new System.Exception("userFQN is malformatted");
                }

                List<string> identities = new List<string>();
                identities.Add(userFQN);

                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationIdentityResolver", "GetIdentities", "Info", "Connecting to SmartObject Server...");
                ConnectionClass.ConnectAsAppPool = true;
                var client = ConnectionClass.GetSmartObjectClient();
                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationIdentityResolver", "GetIdentities", "Info", "Connected to " + client.Connection.Host + " on port " + client.Connection.Port.ToString());

                var smo = client.GetSmartObject(IdentitySmartObjectName);
                smo.MethodToExecute = IdentitySmartObjectMethod;
                smo.Properties["User_Name"].Value = userFQN.Split(':')[1];
                smo.Properties["Security_Label"].Value = userFQN.Split(':')[0];

                using (var reader = client.ExecuteListReader(smo))
                {
                    Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationIdentityResolver", "GetIdentities", "Info", "Successfully executed method " + IdentitySmartObjectMethod + " Rule SmartObject " + IdentitySmartObjectName);
                    var ordinalGroupName = reader.GetOrdinal("Group Name");
                    string groupName;

                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(ordinalGroupName)) groupName = reader.GetString(ordinalGroupName); else continue;
                        identities.Add(groupName);
                        Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationIdentityResolver", "GetIdentities", "Info", "Add Identity: " + groupName);
                    }
                }

                // I've added a call in 4.7 to do this but we may need to add an IIdentitiesResolver to wrap it
                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationIdentityResolver", "GetIdentities", "Info", "Number of identities: " + identities.Count.ToString());
                return identities.ToArray();
            } 
            catch (Exception ex)
            {
                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationIdentityResolver", "GetIdentities", "Error", "Exception occurred: " + ex.ToString());
                throw;
            }
            finally
            {
                ConnectionClass.ConnectAsAppPool = original;
            }
		}

        /// <summary>
        /// Gets a collection of identities obtained from K2 Identity Service for a provided user fully-qualified name.
        /// </summary>
        /// <param name="userFQN">The fully-qualified name of the user as defined in K2 Identity Service.</param>
        /// <returns>Returns an arry of string values representing the identies cached for the user in the K2 Identity Service.</returns>
        public string[] GetIdentities(string userFQN)
        {
            var empty = new object();
            return GetIdentities(false, string.Empty, ref empty, userFQN);
        }

        #endregion

        #endregion
    }
}

using System.Collections.ObjectModel;

namespace K2Field.SmartForms.Authorization
{
    /// <summary>
    /// Represents a collection of authorization rules defined for K2 smartforms Runtime web-app.
    /// </summary>
	public class AuthorizationRuleCollection : Collection<AuthorizationRule>
	{
        #region Methods

        #region Is Authorized

        /// <summary>
        /// Checks whether the supplied identities of the user is authorized to access the specified resource across all defined authorization rules.
        /// </summary>
        /// <param name="enableLogging">A flag to indicate whether or not logging has been enabled.</param>
        /// <param name="filePath">The path where the log file resides, or should be created.</param>
        /// <param name="logSync">The thread synchronization object for writting to the log file.</param>
        /// <param name="requestedSecurableName">The name of the K2 resource being requested by the user.</param>
        /// <param name="requestedSecurableType">The type of resource being requested.</param>
        /// <param name="identities">The identities of the user requesting access.</param>
        /// <param name="requestedAccess">The type of access the user is requesting.</param>
        /// <returns><c>true</c> if the user is authorized to access the resource; Otherwise <c>false</c>.</returns>
        public bool IsAuthorized(bool enableLogging, string filePath, ref object logSync, string requestedSecurableName, SecurableType requestedSecurableType, string[] identities, PermissionType requestedAccess)
		{
			var isAuthorized = false;

			foreach (var rule in this)
			{
				if (rule.Matches(enableLogging, filePath, ref logSync, identities, requestedSecurableName, requestedSecurableType, requestedAccess) == true)
				{
                    Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRuleCollection", "IsAuthorized", "Info", "Evaluating Rule " + rule.ID.ToString() + " ...  SUCCESS");

                    // Matching allow found
                    isAuthorized = true;
                    break;
				}

                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRuleCollection", "IsAuthorized", "Info", "Evaluating Rule " + rule.ID.ToString() + " ...  FAILED");
            }

            return isAuthorized;
		}

        #endregion

        #endregion
    }
}

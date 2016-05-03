using SourceCode.Forms.AppFramework;
using System.Configuration;
using System;

namespace K2Field.SmartForms.Authorization
{
    /// <summary>
    /// Represents a configuration K2 SmartObject based authorization rule provider implementing the <see cref="IAuthorizationRuleProvider"/> interface.
    /// </summary>
    class SmartObjectRuleProvider : Interfaces.IAuthorizationRuleProvider
	{
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartObjectRuleProvider"/> class.
        /// </summary>
        public SmartObjectRuleProvider()
		{
			RulesSmartObjectName = ConfigurationManager.AppSettings["AuthorizationModule.SmartObjectName"] ?? "AM_RuleStore";
			RulesSmartObjectMethod = ConfigurationManager.AppSettings["AuthorizationModule.SmartObjectMethod"] ?? "GetList";
		}

        #endregion

        #region Properties

        private string RulesSmartObjectName { get; }

		private string RulesSmartObjectMethod { get; }

        #endregion

        #region Metods

        #region Get Rules

        /// <summary>
        /// Gets a collection of authorization rules defined for K2 smartforms Runtime web-app which stored in a SmartObject.
        /// </summary>
        /// <param name="enableLogging">A flag to indicate whether or not logging has been enabled.</param>
        /// <param name="filePath">The path where the log file resides, or should be created.</param>
        /// <param name="logSync">The thread synchronization object for writting to the log file.</param>
        /// <returns>Returns <see cref="AuthorizationRuleCollection"/> containing auhtorization rules that has been specified for the K2 smartforms Runtime web-app.</returns>
        public AuthorizationRuleCollection GetRules(bool enableLogging, string filePath, ref object logSync)
		{
			// TODO: Cache

			var original = ConnectionClass.ConnectAsAppPool;

			try
			{
				ConnectionClass.ConnectAsAppPool = true;
                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "SmartObjectRuleProvider", "GetRules", "Info", "Trying to connect to SmartObject Server...");
                var client = ConnectionClass.GetSmartObjectClient();
                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "SmartObjectRuleProvider", "GetRules", "Info", "Connected to " + client.Connection.Host + " on port " + client.Connection.Port.ToString());

                var smo = client.GetSmartObject(RulesSmartObjectName);
				smo.MethodToExecute = RulesSmartObjectMethod;
                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "SmartObjectRuleProvider", "GetRules", "Info", "Successfully executed method " + RulesSmartObjectMethod + " Rule SmartObject " + RulesSmartObjectName);

                var rules = new AuthorizationRuleCollection();

				using (var reader = client.ExecuteListReader(smo))
				{
					var ordinalIdentityPattern = reader.GetOrdinal("Identity Pattern");
					var ordinalSecurablePattern = reader.GetOrdinal("Securable Pattern");
					var ordinalSecurableType = reader.GetOrdinal("Securable Type");
					var ordinalPermissionType = reader.GetOrdinal("Permission Type");

                    string identityPattern;
                    string securablePattern;
					long securableType;
					long permissionType;

					while (reader.Read())
					{

                        if (!reader.IsDBNull(ordinalIdentityPattern)) identityPattern = reader.GetString(ordinalIdentityPattern); else continue;
                        if (!reader.IsDBNull(ordinalSecurablePattern)) securablePattern = reader.GetString(ordinalSecurablePattern); else continue;
						if (!reader.IsDBNull(ordinalSecurableType)) securableType = reader.GetInt64(ordinalSecurableType); else continue;
						if (!reader.IsDBNull(ordinalPermissionType)) permissionType = reader.GetInt64(ordinalPermissionType); else continue;

						var rule = new AuthorizationRule(
                            securablePattern.Split(',', ';'),
							(SecurableType)securableType,
							(PermissionType)permissionType,
                            identityPattern.Split(',', ';')
							);
						rules.Add(rule);

                        Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "SmartObjectRuleProvider", "GetRules", "Info", "Add Rule: Identity Pattern=" + identityPattern + "; Securable Pattern=" + securablePattern + "; Securable Type=" + securableType + "; Permission Type=" + permissionType);
                    }
                }

                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "SmartObjectRuleProvider", "GetRules", "Info", "Number of authorization rules: " + rules.Count.ToString());
                return rules;
			}
            catch (Exception ex)
            {
                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "SmartObjectRuleProvider", "GetRules", "Error", "Exception occurred: " + ex.ToString());
                throw;
            }
            finally
            {
				ConnectionClass.ConnectAsAppPool = original;
			}
		}

        /// <summary>
        /// Gets a collection of authorization rules defined for K2 smartforms Runtime web-app which stored in a SmartObject.
        /// </summary>
        /// <returns>Returns <see cref="AuthorizationRuleCollection"/> containing auhtorization rules that has been specified for the K2 smartforms Runtime web-app.</returns>
        public AuthorizationRuleCollection GetRules()
        {
            var empty = new object();
            return GetRules(false, string.Empty, ref empty);
        }

        #endregion

        #endregion
    }
}

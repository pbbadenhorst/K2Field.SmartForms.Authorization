using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace K2Field.SmartForms.Authorization
{
    /// <summary>
    /// Represents a authorization rule instance defined for K2 smartforms Runtime web-app.
    /// </summary>
	public class AuthorizationRule
	{
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationRule"/> class.
        /// </summary>
        /// <param name="securablePatterns">The pattern that the rule applies to as a list of one or more K2 resource names (names with *, % for wildcards supported).</param>
        /// <param name="securableType">The type of K2 resource that is secured.</param>
        /// <param name="permissionType">Type of the permission that is granted.</param>
        /// <param name="identityPatterns">The pattern that the rule applies to as a list of one or more identity names (names with *, % for wildcards supported) for the a user.</param>
        public AuthorizationRule(IEnumerable<string> securablePatterns, SecurableType securableType, PermissionType permissionType, IEnumerable<string> identityPatterns)
		{
			// Argument checking
			if (securablePatterns == null) throw new ArgumentNullException("securablePatterns");
			if (identityPatterns == null) throw new ArgumentNullException("identityPatterns");


			this.Securables = new HashSet<string>(identityPatterns, StringComparer.OrdinalIgnoreCase);
			this.SecurablePatterns = new List<Regex>();
			foreach (var securable in securablePatterns)
			{
				if (securable.IndexOfAny(new char[] { '*', '%' }) >= 0)
				{
					var securablePattern =
						"^" + // Begining of the string
						Regex.Escape(securable)
							.Replace("\\*", ".*") // Any length wild-card   --- "(.)+"
                            .Replace("%", ".*") // Any length wild-card   --- "(.)+"
                        + "$"; // End of the string

					this.SecurablePatterns.Add(new Regex(securablePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
				}
				else
				{
					this.Securables.Add(securable);
				}

			}

			// Sanity check
			if (this.Securables.Count == 0 && this.SecurablePatterns.Count == 0)
			{
				throw new ArgumentException(Properties.Resources.AtLeastOneResourceNameOrPatternIsRequired);
			}

			this.SecurableType = securableType;
			this.PermissionType = permissionType;
			this.Identities = new HashSet<string>(identityPatterns, StringComparer.OrdinalIgnoreCase);
			this.IdentityPatterns = new List<Regex>();

			foreach (var identity in identityPatterns)
			{
				if (identity.IndexOfAny(new char[] { '*', '%' }) >= 0)
				{
					var identityPattern =
						"^" + // Begining of the string
						Regex.Escape(identity)
							.Replace("\\*", ".*") // Any length wild-card
							.Replace("%", ".*") // Any length wild-card
						+ "$"; // End of the string

					this.IdentityPatterns.Add(new Regex(identityPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
				}
				else
				{
					this.Identities.Add(identity);
				}
			}

			// Sanity check
			if (this.Identities.Count == 0 && this.IdentityPatterns.Count == 0)
			{
				throw new ArgumentException(Properties.Resources.AtLeastOneIdentityNameOrPatternIsRequired);
			}
		}

        #endregion

        #region Properties

        public HashSet<string> Identities { get; }
		public IList<Regex> IdentityPatterns { get; }

		public HashSet<string> Securables { get; }
		public IList<Regex> SecurablePatterns { get; }
		public SecurableType SecurableType { get; }
		public PermissionType PermissionType { get; }

        #endregion

        #region Methods

        #region Matches

        /// <summary>
        /// Checks whether the supplied identities of the user is authorized to access the specified resource as defined by the authorization rule instance.
        /// </summary>
        /// <param name="enableLogging">A flag to indicate whether or not logging has been enabled.</param>
        /// <param name="filePath">The path where the log file resides, or should be created.</param>
        /// <param name="logSync">The thread synchronization object for writting to the log file.</param>
        /// <param name="identities">The identities of the user requesting access.</param>
        /// <param name="requestedSecurableName">The name of the K2 resource being requested by the user.</param>
        /// <param name="requestedSecurableType">The type of resource being requested.</param>
        /// <param name="requestedAccess">The type of access the user is requesting.</param>
        /// <returns><c>true</c> if the user is authorized to access the resource; Otherwise <c>false</c>.</returns>
        internal bool Matches(bool enableLogging, string filePath, ref object logSync, string[] identities, string requestedSecurableName, SecurableType requestedSecurableType, PermissionType requestedAccess)
		{
            try
            {
                if ((requestedSecurableType != this.SecurableType) || (requestedAccess != this.PermissionType))
                {
                    if (enableLogging == true)
                    {
                        if (requestedSecurableType != this.SecurableType)
                        {
                            Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRule", "Matches", "Info", "Requested resource does not match securable type: " + requestedSecurableType.ToString() + " <> " + this.SecurableType.ToString());
                        }
                        if (requestedAccess != this.PermissionType)
                        {
                            Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRule", "Matches", "Info", "Requested access does not match permission: " + requestedAccess.ToString() + " <> " + this.PermissionType.ToString());
                        }
                    }
                    return false;
                }

                // Check if the specified resource matches any of the resources specified id the rule
                var match = (this.Securables.Contains(requestedSecurableName));
                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRule", "Matches", "Info", "Contain match on all Rule Securables to \"" + requestedSecurableName + "\"... Result: " + match.ToString());

                if (!match)
                {
                    foreach (var securablePattern in this.SecurablePatterns)
                    {
                        if (securablePattern.IsMatch(requestedSecurableName))
                        {
                            match = true;
                        }
                        Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRule", "Matches", "Info", "Matching securable pattern \"" + securablePattern + "\" to \"" + requestedSecurableName + "\"... Result: " + match.ToString());
                    }
                }

                if (match)
                {
                    // Check if any identities specified matches any of the identities specified in the rule
                    if (this.Identities.Count > 0)
                    {
                        foreach (var identity in identities)
                        {
                            Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRule", "Matches", "Info", "Contain match on all Rule Identities to \"" + identity + "\"...");
                            if (this.Identities.Contains(identity) == true)
                            {
                                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRule", "Matches", "Info", "Success");
                                return true;
                            }
                        }
                    }

                    if (this.IdentityPatterns.Count > 0)
                    {
                        foreach (var identityPattern in this.IdentityPatterns)
                        {
                            foreach (var identity in identities)
                            {
                                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRule", "Matches", "Info", "Matching identity pattern \"" + identityPattern + "\" to \"" + identity + "\"...");
                                if (identityPattern.IsMatch(identity) == true)
                                {
                                    Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRule", "Matches", "Info", "Success");
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.Logfile.Log(enableLogging, filePath, ref logSync, "AuthorizationRule", "Matches", "Error", "Exception occured: " + ex.ToString());
                throw;
            }

            // Could not match at least one resource and one identity specified in rule
            return false;
		}

        #endregion

        #endregion
    }
}

namespace K2Field.SmartForms.Authorization
{
	/// <summary>
	/// Represents a configuration file based authorization rule provider implementing the <see cref="IAuthorizationRuleProvider"/> interface.
	/// </summary>
	public class ConfigurationRuleProvider : Interfaces.IAuthorizationRuleProvider
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationRuleProvider"/> class.
		/// </summary>
		public ConfigurationRuleProvider()
		{
		}

        #endregion

        #region Methods

        #region Get Rules

        /// <summary>
        /// Gets a collection of authorization rules defined for K2 smartforms Runtime web-app which stored in a configuration file.
        /// </summary>
        /// <param name="enableLogging">A flag to indicate whether or not logging has been enabled.</param>
        /// <param name="filePath">The path where the log file resides, or should be created.</param>
        /// <param name="logSync">The thread synchronization object for writting to the log file.</param>
        /// <returns>Returns <see cref="AuthorizationRuleCollection"/> containing auhtorization rules that has been specified for the K2 smartforms Runtime web-app.</returns>
        public AuthorizationRuleCollection GetRules(bool enableLogging, string filePath, ref object sync)
		{
			var rules = new AuthorizationRuleCollection();
			return new AuthorizationRuleCollection();
		}

        /// <summary>
        /// Gets a collection of authorization rules defined for K2 smartforms Runtime web-app which stored in a configuration file.
        /// </summary>
        /// <returns>Returns <see cref="AuthorizationRuleCollection"/> containing auhtorization rules that has been specified for the K2 smartforms Runtime web-app.</returns>
        public AuthorizationRuleCollection GetRules()
        {
            var rules = new AuthorizationRuleCollection();
            return new AuthorizationRuleCollection();
        }

        #endregion

        #endregion
    }
}

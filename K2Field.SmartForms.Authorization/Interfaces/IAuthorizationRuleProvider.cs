namespace K2Field.SmartForms.Authorization.Interfaces
{
    /// <summary>
    /// An interface for authorization rules defined for K2 smartforms Runtime web-app.
    /// </summary>
	public interface IAuthorizationRuleProvider
	{
        #region Methods

        #region Get Rules

        /// <summary>
        /// Gets a collection of authorization rules defined for K2 smartforms Runtime web-app which stored in a SmartObject.
        /// Preformance Critical: This method will be called many times and needs to implement caching, locking etc. internally.
        /// </summary>
        /// <param name="enableLogging">A flag to indicate whether or not logging has been enabled.</param>
        /// <param name="filePath">The path where the log file resides, or should be created.</param>
        /// <param name="logSync">The thread synchronization object for writting to the log file.</param>
        /// <returns>Returns <see cref="AuthorizationRuleCollection"/> containing auhtorization rules that has been specified for the K2 smartforms Runtime web-app.</returns>
        AuthorizationRuleCollection GetRules(bool enableLogging, string filePath, ref object logSync);

        /// <summary>
        /// Gets a collection of authorization rules defined for K2 smartforms Runtime web-app which stored in a SmartObject.
        /// Preformance Critical: This method will be called many times and needs to implement caching, locking etc. internally.
        /// </summary>
        /// <returns>Returns <see cref="AuthorizationRuleCollection"/> containing auhtorization rules that has been specified for the K2 smartforms Runtime web-app.</returns>
        AuthorizationRuleCollection GetRules();

        #endregion

        #endregion
    }
}

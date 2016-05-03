namespace K2Field.SmartForms.Authorization.Interfaces
{
    /// <summary>
    /// An interface for resolving user identities form the K2 Identity Service.
    /// </summary>
	public interface IAuthorizationIdentityResolver
	{
        #region Methods

        #region Get Identities

        /// <summary>
        /// Gets a collection of identities obtained from K2 Identity Service for a provided user fully-qualified name.
        /// Preformance Critical: This method will be called many times and needs to implement caching, locking etc. internally.
        /// </summary>
        /// <param name="enableLogging">A flag to indicate whether or not logging has been enabled.</param>
        /// <param name="filePath">The path where the log file resides, or should be created.</param>
        /// <param name="logSync">The thread synchronization object for writting to the log file.</param>
        /// <param name="userFQN">The fully-qualified name of the user as defined in K2 Identity Service.</param>
        /// <returns>Returns an arry of string values representing the identies cached for the user in the K2 Identity Service.</returns>
        string[] GetIdentities(bool enableLogging, string filePath, ref object logSync, string userFQN);

        /// <summary>
        /// Gets a collection of identities obtained from K2 Identity Service for a provided user fully-qualified name.
        /// Preformance Critical: This method will be called many times and needs to implement caching, locking etc. internally.
        /// </summary>
        /// <param name="userFQN">The fully-qualified name of the user as defined in K2 Identity Service.</param>
        /// <returns>Returns an arry of string values representing the identies cached for the user in the K2 Identity Service.</returns>
        string[] GetIdentities(string userFQN);

        #endregion

        #endregion
    }
}

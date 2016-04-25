namespace K2Field.SmartForms.Authorization
{
	public interface IAuthorizationRuleProvider
	{
		/// <summary>
		/// Get the current set of authorization rules from the provider.
		/// Preformance Critical: This method will be called many times and needs to implement caching, locking etc. internally.
		/// </summary>
		/// <returns>The current rules (see <see cref="AuthorizationRuleCollection"/>)</returns>
		AuthorizationRuleCollection GetRules();
	}
}

namespace K2Field.SmartForms.Authorization
{
	/// <summary>
	/// Represents a configuration file based authorization rule provider implementing the <see cref="IAuthorizationRuleProvider"/> interface.
	/// </summary>
	public class ConfigurationRuleProvider
		: IAuthorizationRuleProvider
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationRuleProvider"/> class.
		/// </summary>
		public ConfigurationRuleProvider()
		{
		}

		#endregion

		#region IAuthorizationRuleProvider members

		AuthorizationRuleCollection IAuthorizationRuleProvider.GetRules()
		{
			var rules = new AuthorizationRuleCollection();

			return new AuthorizationRuleCollection();
		}

		#endregion
	}
}

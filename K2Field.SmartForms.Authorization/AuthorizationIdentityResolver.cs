namespace K2Field.SmartForms.Authorization
{
	public class AuthorizationIdentityResolver : IIdentityResolver
	{
		public string[] GetIdentities(string userFqn)
		{
			// TODO: Include the FQNs of the user's groups and roles
			// I've added a call in 4.7 to do this but we may need to add an IIdentitiesResolver to wrap it
			return new string[] { userFqn };
		}
	}
}

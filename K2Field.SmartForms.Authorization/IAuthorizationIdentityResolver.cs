namespace K2Field.SmartForms.Authorization
{
	public interface IAuthorizationIdentityResolver
	{
		string[] GetIdentities(string userFqn);
	}
}

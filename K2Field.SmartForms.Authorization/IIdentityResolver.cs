namespace K2Field.SmartForms.Authorization
{
	public interface IIdentityResolver
	{
		string[] GetIdentities(string userFqn);
	}
}

using System.Collections.ObjectModel;

namespace K2Field.SmartForms.Authorization
{
	public class AuthorizationRuleCollection : Collection<AuthorizationRule>
	{
		/// <summary>
		/// Checks whether the supplied identities of the user is authorized to access the specified resource.
		/// </summary>
		/// <param name="resourceName">The resource name being requested.</param>
		/// <param name="resourceType">The type of resource being requested.</param>
		/// <param name="identities">The identities of the user requesting access.</param>
		/// <returns><c>true</c> if the user is authorized to access the resource; Otherwise <c>false</c>.</returns>
		public bool IsAuthorized(string resourceName, ResourceTypes resourceType, string[] identities)
		{
			var isAuthorized = false;

			foreach (var rule in this)
			{
				if (rule.Matches(identities, resourceName, resourceType))
				{
					if (rule.PermissionType == PermissionType.Deny)
					{
						// Found deny rule - can return immediately
						return false;
					}

					// Matching allow found, continue because we still need to check for other matching deny rules.
					isAuthorized = true;
				}
			}

			// No deny rules - return true if an allow rule was found; otherwise false;
			return isAuthorized;
		}
	}
}

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace K2Field.SmartForms.Authorization
{
	internal class AuthorizationRule
	{
		public AuthorizationRule(string identityPattern, string securablePattern, SecurableTypes securableTypes, PermissionType permissionType)
		{
			this.IdentityPattern = new Regex(identityPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			this.SecurablePattern = new Regex(identityPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			this.SecurableTypes = securableTypes;
			this.PermissionType = PermissionType;
		}

		public Regex IdentityPattern { get; }
		public Regex SecurablePattern { get; }
		public SecurableTypes SecurableTypes { get; }
		public PermissionType PermissionType { get; }

		public bool TryApply(IEnumerable<string> identities, SecurableTypes securableType, string securable, out PermissionType permissionType)
		{
			if (this.SecurablePattern.IsMatch(securable))
			{
				foreach (var identity in identities)
				{
					if (this.IdentityPattern.IsMatch(identity))
					{
						permissionType = this.PermissionType;
						return true;
					}
				}
			}

			// Securable not matched - Rule not applied
			permissionType = PermissionType.NotApplicable;
			return false;
		}
	}
}

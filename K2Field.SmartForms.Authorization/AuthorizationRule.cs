using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace K2Field.SmartForms.Authorization
{
	public class AuthorizationRule
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationRule"/> class.
		/// </summary>
		/// <param name="resourceName">The resource name.</param>
		/// <param name="resourceTypes">The resource types.</param>
		/// <param name="permissionType">Type of the permission.</param>
		/// <param name="identities">The identities that the rule apply to.</param>
		public AuthorizationRule(string resourceName, ResourceTypes resourceTypes, PermissionType permissionType, IEnumerable<string> identities)
		{
			this.ResourcePattern = new Regex(resourceName, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			this.ResourceTypes = resourceTypes;
			this.PermissionType = permissionType;
			this.Identities = new HashSet<string>(identities, StringComparer.OrdinalIgnoreCase);
		}

		public HashSet<string> Identities { get; }
		public Regex ResourcePattern { get; }
		public ResourceTypes ResourceTypes { get; }
		public PermissionType PermissionType { get; }

		internal bool Matches(string[] identities, string resourceName, ResourceTypes resourceType)
		{
			if (ResourcePattern.IsMatch(resourceName))
			{
				foreach (var identity in identities)
				{
					if (this.Identities.Contains(identity))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}

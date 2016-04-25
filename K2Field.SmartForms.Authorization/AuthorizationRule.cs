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
		/// <param name="resources">The resources that the rule applies to as a list of one or more names (names with *, % for wildcards supported).</param>
		/// <param name="resourceTypes">The resource types.</param>
		/// <param name="permissionType">Type of the permission.</param>
		/// <param name="identities">The identities that the rule applies to as a list of one or more names (names with *, % for wildcards supported).</param>
		public AuthorizationRule(IEnumerable<string> resources, ResourceTypes resourceTypes, PermissionType permissionType, IEnumerable<string> identities)
		{
			// Argument checking
			if (resources == null) throw new ArgumentNullException("resources");
			if (identities == null) throw new ArgumentNullException("identities");


			this.Resources = new HashSet<string>(identities, StringComparer.OrdinalIgnoreCase);
			this.ResourcePatterns = new List<Regex>();
			foreach (var resource in resources)
			{
				if (resource.IndexOfAny(new char[] { '*', '%' }) >= 0)
				{
					var resourcePattern =
						"^" + // Begining of the string
						Regex.Escape(resource)
							.Replace("\\*", "(.)+") // Any length wild-card
							.Replace("%", "(.)+") // Any length wild-card
						+ "$"; // End of the string

					this.ResourcePatterns.Add(new Regex(resourcePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
				}
				else
				{
					this.Resources.Add(resource);
				}

			}

			// Sanity check
			if (this.Resources.Count == 0 && this.ResourcePatterns.Count == 0)
			{
				throw new ArgumentException(Properties.Resources.AtLeastOneResourceNameOrPatternIsRequired);
			}

			this.ResourceTypes = resourceTypes;
			this.PermissionType = permissionType;

			this.Identities = new HashSet<string>(identities, StringComparer.OrdinalIgnoreCase);
			this.IdentityPatterns = new List<Regex>();
			foreach (var identity in identities)
			{
				if (identity.IndexOfAny(new char[] { '*', '%' }) >= 0)
				{
					var identityPattern =
						"^" + // Begining of the string
						Regex.Escape(identity)
							.Replace("\\*", "(.)+") // Any length wild-card
							.Replace("%", "(.)+") // Any length wild-card
						+ "$"; // End of the string

					this.IdentityPatterns.Add(new Regex(identityPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
				}
				else
				{
					this.Identities.Add(identity);
				}
			}

			// Sanity check
			if (this.Identities.Count == 0 && this.IdentityPatterns.Count == 0)
			{
				throw new ArgumentException(Properties.Resources.AtLeastOneIdentityNameOrPatternIsRequired);
			}
		}


		public HashSet<string> Identities { get; }
		public IList<Regex> IdentityPatterns { get; }

		public HashSet<string> Resources { get; }
		public IList<Regex> ResourcePatterns { get; }
		public ResourceTypes ResourceTypes { get; }
		public PermissionType PermissionType { get; }

		internal bool Matches(string[] identities, string resourceName, ResourceTypes resourceType)
		{
			// Check if the specified resource matches any of the resources specified id the rule
			var match = (this.Resources.Contains(resourceName));
			if (!match)
			{
				foreach (var resourcePattern in this.ResourcePatterns)
				{
					if (resourcePattern.IsMatch(resourceName))
					{
						match = true;
					}
				}
			}

			if (match)
			{
				// Check if any identities specified matches any of the identities specified in the rule
				if (this.Identities.Count > 0)
				{
					foreach (var identity in identities)
					{
						if (this.Identities.Contains(identity))
						{
							return true;
						}
					}
				}

				if (this.IdentityPatterns.Count > 0)
				{
					foreach (var identityPattern in this.IdentityPatterns)
					{
						foreach (var identity in identities)
						{
							if (identityPattern.IsMatch(identity))
							{
								return true;
							}
						}
					}
				}
			}
			
			// Could not match at least one resource and one identity specified in rule
			return false;
		}
	}
}

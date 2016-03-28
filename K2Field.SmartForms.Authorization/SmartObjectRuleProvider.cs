using SourceCode.Forms.AppFramework;
using System.Configuration;

namespace K2Field.SmartForms.Authorization
{
	class SmartObjectRuleProvider : IAuthorizationRuleProvider
	{
		public SmartObjectRuleProvider()
		{
			RulesSmartObjectName = ConfigurationManager.AppSettings["K2Field.SmartForms.Authorization.SmartObjectRuleProvider.SmartObject.Name"] ?? "K2Field_SmartForms_Authorization_Rules";
			RulesSmartObjectMethod = ConfigurationManager.AppSettings["K2Field.SmartForms.Authorization.SmartObjectRuleProvider.SmartObject.Method"] ?? "GetList";
		}

		private string RulesSmartObjectName { get; }
		private string RulesSmartObjectMethod { get; }

		public AuthorizationRuleCollection GetRules()
		{
			// TODO: Cache

			var original = ConnectionClass.ConnectAsAppPool;
			try
			{
				ConnectionClass.ConnectAsAppPool = true;
				var client = ConnectionClass.GetSmartObjectClient();

				var smo = client.GetSmartObject(RulesSmartObjectName);
				smo.MethodToExecute = RulesSmartObjectMethod;

				var rules = new AuthorizationRuleCollection();

				using (var reader = client.ExecuteListReader(smo))
				{
					var ordinalIdentities = reader.GetOrdinal("Identities");
					var ordinalResourceName = reader.GetOrdinal("ResourceName");
					var ordinalResourceType = reader.GetOrdinal("ResourceType");
					var ordinalAllow = reader.GetOrdinal("Allow");

					string resourceName;
					int resourceType;
					bool allow;
					string identities;

					while (reader.Read())
					{

						if (!reader.IsDBNull(ordinalResourceName)) resourceName = reader.GetString(ordinalResourceName); else continue;
						if (!reader.IsDBNull(ordinalResourceType)) resourceType = reader.GetInt32(ordinalResourceType); else continue;
						if (!reader.IsDBNull(ordinalAllow)) allow = reader.GetBoolean(ordinalAllow); else continue;
						if (!reader.IsDBNull(ordinalIdentities)) identities = reader.GetString(ordinalIdentities); else continue;

						var rule = new AuthorizationRule(
							resourceName,
							(ResourceTypes)resourceType,
							allow ? PermissionType.Allow : PermissionType.Deny,
							identities.Split(',', ';')
							);
						rules.Add(rule);
					}
				}

				return rules;
			}
			finally
			{
				ConnectionClass.ConnectAsAppPool = original;
			}
		}
	}
}

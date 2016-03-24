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
					var ordinalIdentity = reader.GetOrdinal("Identity");
					var ordinalSecurableName = reader.GetOrdinal("SecurableName");
					var ordinalSecurableType = reader.GetOrdinal("SecurableType");
					var ordinalAllow = reader.GetOrdinal("Allow");

					string identity;
					string securableName;
					int securableType;
					bool allow;

					while (reader.Read())
					{

						if (!reader.IsDBNull(ordinalIdentity)) identity = reader.GetString(ordinalIdentity); else continue;
						if (!reader.IsDBNull(ordinalSecurableName)) securableName = reader.GetString(ordinalSecurableName); else continue;
						if (!reader.IsDBNull(ordinalSecurableType)) securableType = reader.GetInt32(ordinalSecurableType); else continue;
						if (!reader.IsDBNull(ordinalAllow)) allow = reader.GetBoolean(ordinalAllow); else continue;

						var rule = new AuthorizationRule(
							identity,
							securableName,
							(SecurableTypes)securableType,
							allow?PermissionType.Allow : PermissionType.Deny
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

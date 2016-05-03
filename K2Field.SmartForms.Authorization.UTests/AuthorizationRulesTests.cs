using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace K2Field.SmartForms.Authorization.UTests
{
	[TestClass]
	public class given_no_rules_configured
	{

		[TestMethod]
		public void when_checking_authorization()
		{
			var rules = new AuthorizationRuleCollection();
			var currentIdentities = new string[] { "L1:DOMAIN1\\User1", "L1:DOMAIN1\\Group1", "Role1" };
            var empty = new object();

			Assert.IsFalse(rules.IsAuthorized(false, string.Empty, ref empty, "Form1", SecurableType.Form, currentIdentities, PermissionType.View), "it should not be authorized");
			Assert.IsFalse(rules.IsAuthorized(false, string.Empty, ref empty, "View1", SecurableType.View, currentIdentities, PermissionType.View), "it should not be authorized");
		}
	}

	[TestClass]
	public class given_a_single_allow_rule_configured_for_a_resource
	{
		[TestMethod]
		public void when_checking_authorization_for_a_matching_resource_and_a_matching_identity()
		{
			var currentIdentities = new string[] { "L1:DOMAIN1\\User1", "L1:DOMAIN1\\Group1", "Role1" };
			var resourceName = "Form1";
			var resources = new string[] { resourceName };
			var resourceType = SecurableType.Form;
			var allowedIdentities = new string[] { "L1:DOMAIN1\\User1", "L1:DOMAIN1\\User2" };
			var rules = new AuthorizationRuleCollection();
			var allowRule = new AuthorizationRule(resources, resourceType, PermissionType.View, allowedIdentities);
            var empty = new object();
			rules.Add(allowRule);

			Assert.IsTrue(rules.IsAuthorized(false, string.Empty, ref empty, resourceName, resourceType, currentIdentities, PermissionType.View), "it should be authorized");
		}

		[TestMethod]
		public void when_checking_authorization_for_non_matching_resource_and_a_matching_identitiy()
		{
			var currentIdentities = new string[] { "L1:DOMAIN1\\User1", "L1:DOMAIN1\\Group1", "Role1" };
			var resourceName = "Form1";
			var resources = new string[] { resourceName };
			var resourceNameOther = "FormA";
			var resourceType = SecurableType.Form;
			var allowedIdentities = new string[] { "L1:DOMAIN1\\User1", "L1:DOMAIN1\\User2" };
			var rules = new AuthorizationRuleCollection();
			var allowRule = new AuthorizationRule(resources, resourceType, PermissionType.View, allowedIdentities);
            var empty = new object();
			rules.Add(allowRule);

			Assert.IsFalse(rules.IsAuthorized(false, string.Empty, ref empty, resourceNameOther, resourceType, currentIdentities, PermissionType.View), "it should not be authorized");
		}

		[TestMethod]
		public void when_checking_authorization_for_non_matching_resource_and_non_matching_identities()
		{
			var currentIdentities = new string[] { "L1:DOMAIN1\\User1", "L1:DOMAIN1\\Group1", "Role1" };
			var resourceName = "Form1";
			var resources = new string[] { resourceName };
			var resourceNameOther = "FormA";
			var resourceType = SecurableType.Form;
			var allowedIdentities = new string[] { "L1:DOMAIN1\\UserA", "L1:DOMAIN1\\User2" };
			var rules = new AuthorizationRuleCollection();
			var allowRule = new AuthorizationRule(resources, resourceType, PermissionType.View, allowedIdentities);
            var empty = new object();
			rules.Add(allowRule);

			Assert.IsFalse(rules.IsAuthorized(false, string.Empty, ref empty, resourceNameOther, resourceType, currentIdentities, PermissionType.View), "it should not be authorized");
		}
	}

	[TestClass]
	public class given_an_allow_and_deny_rule_configured_for_a_resource
	{
		[TestMethod]
		public void when_checking_authorization_for_a_matching_resource_and_user_identity_with_deny_rule()
		{
			var currentIdentities = new string[] { "L1:DOMAIN1\\User1", "L1:DOMAIN1\\Group1", "Role1" };
			var resourceName = "Form1";
			var resources = new string[] { resourceName };
			var resourceType = SecurableType.Form;
			var deniedIdentities = new string[] { "L1:DOMAIN1\\User1", "L1:DOMAIN1\\User2" };
			var rules = new AuthorizationRuleCollection();
			var denyRule = new AuthorizationRule(resources, resourceType, PermissionType.Edit, deniedIdentities);
            var empty = new object();
			rules.Add(denyRule);

			Assert.IsFalse(rules.IsAuthorized(false, string.Empty, ref empty, resourceName, resourceType, currentIdentities, PermissionType.View), "it should not be authorized");
		}

		[TestMethod]
		public void when_checking_authorization_for_a_matching_resource_and_user_with_both_allow_and_deny_rules()
		{
			var currentIdentities = new string[] { "L1:DOMAIN1\\User1", "L1:DOMAIN1\\Group1", "Role1" };
			var resourceName = "Form1";
			var resources = new string[] { resourceName };
			var resourceType = SecurableType.Form;
			var allowedIdentities = new string[] { "L1:DOMAIN1\\UserA", "L1:DOMAIN1\\User1" };
			var deniedIdentities = new string[] { "L1:DOMAIN1\\GroupA", "L1:DOMAIN1\\Group1" };

			var rules = new AuthorizationRuleCollection();
			var allowRule = new AuthorizationRule(resources, resourceType, PermissionType.View, allowedIdentities);
			rules.Add(allowRule);
			var denyRule = new AuthorizationRule(resources, resourceType, PermissionType.Edit, deniedIdentities);
            var empty = new object();
			rules.Add(denyRule);

			Assert.IsFalse(rules.IsAuthorized(false, string.Empty, ref empty, resourceName, resourceType, currentIdentities, PermissionType.View), "it should not be authorized");
		}
	}
}
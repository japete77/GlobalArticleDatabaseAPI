using Core.Exceptions;
using GlobalArticleDatabaseAPI.Helpers;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using static GlobalArticleDatabaseAPI.Tests.Unit.SecurityTestDataAllowAccess;

namespace GlobalArticleDatabaseAPI.Tests.Unit
{
    public class SecurityCheck
    {
        public static string TestControllerName = "TestController";
        public static string TestActionName = "TestAction";

        [Fact]
        public void SecurityConfig_Exists_CanBeLoaded()
        {
            // if security config is not loaded an exception is raised
            SecurityHelper.Initialize();
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        public void SecurityConfig_DoNotExist_ExceptionRaised()
        {
            Assert.Throws<InternalException>(() => SecurityHelper.Initialize("NonExistingFile.json"));
        }

        [Theory]
        [Trait("Category", "UnitTest")]
        [ClassData(typeof(SecurityTestDataAllowAccess))]
        public void Controller_PermissionsAllowAccess_Success(string controllerName, string actionName, string securityConfig, string userRoles)
        {
            var roles = Split(userRoles);

            Assert.True(SecurityHelper.CheckSecurity(controllerName, actionName, roles, securityConfig));
        }

        [Theory]
        [Trait("Category", "UnitTest")]
        [ClassData(typeof(SecurityTestDataDenyAccess))]
        public void Controller_PermissionsDenyAccess_Success(string controllerName, string actionName, string securityConfig, string userRoles)
        {
            var roles = Split(userRoles);

            Assert.False(SecurityHelper.CheckSecurity(controllerName, actionName, roles, securityConfig));
        }

        private List<string> Split(string data)
        {
            var result = new List<string>();
            result.AddRange(data.Split(','));
            return result;
        }
    }

    internal class SecurityTestDataAllowAccess : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, OnlyInController, UserRole };
            yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, OnlyInAction, UserRole };
            yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, OverrideInAction, UserRole };
            yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, Controller2RolesOr, UserRole };
            yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, Action2RolesOr, UserRole };
            yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, OverrideInAction2RolesOr, UserRole };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        const string UserRole = "RoleA";
        const string UserInvalidRole = "RoleB";

        private readonly string OnlyInController = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""roles"": [""{UserRole}""],
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}""
        }}
      ]
    }},
  ]
}}
";

        private readonly string OnlyInAction = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{UserRole}""]
        }} 
      ]
    }}
  ]
}}
";

        private readonly string OverrideInAction = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""roles"": [""{UserInvalidRole}""],
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{UserRole}""]
        }}
      ]
    }}
  ]
}}
";
        private readonly string Controller2RolesOr = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""roles"": [""{UserRole}"",""{UserInvalidRole}""],
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}""
        }}
      ]
    }}
  ]
}}
";

        private readonly string Action2RolesOr = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{UserRole}"",""{UserInvalidRole}""]
        }}
      ]
    }}
  ]
}}
";

        private readonly string OverrideInAction2RolesOr = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""roles"": [""{UserInvalidRole}""],
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{UserRole}"",""{UserInvalidRole}""]
        }}
      ]
    }}
  ]
}}
";

        internal class SecurityTestDataDenyAccess : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, OnlyInControllerInvalidRole, UserRole };
                yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, OnlyInActionInvalidRole, UserRole };
                yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, OverrideInActionInvalidRole, UserRole };
                yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, NoRoles, UserRole };
                yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, Controller2RolesAnd, UserRole };
                yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, Action2RolesAnd, UserRole };
                yield return new object[4] { SecurityCheck.TestControllerName, SecurityCheck.TestActionName, OverrideInActionInvalid2RoleAnd, UserRole };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            const string UserRole = "RoleA";
            const string UserInvalidRole = "RoleB";
            const string OtherRole = "RoleC";

            private readonly string OnlyInControllerInvalidRole = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""roles"": [""{UserInvalidRole}""],
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}""
        }}
      ]
    }}
  ]
}}
";

            private readonly string OnlyInActionInvalidRole = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{UserInvalidRole}""]
        }}
      ]
    }}
  ]
}}
";

            private readonly string OverrideInActionInvalidRole = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""roles"": [""{UserInvalidRole}""],
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{OtherRole}""]
        }}
      ]
    }}
  ]
}}
";

            private readonly string NoRoles = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}""          
        }}
      ]
    }}
  ]
}}
";

            private readonly string Controller2RolesAnd = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""roles"": [""{UserInvalidRole}""],
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}""          
        }}
      ]
    }},
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""roles"": [""{UserRole}""],
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}""          
        }}
      ]
    }}
  ]
}}
";

            private readonly string Action2RolesAnd = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{UserRole}""]
        }}
      ]
    }},
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{UserInvalidRole}""]
        }}
      ]
    }}
  ]
}}
";

            private readonly string OverrideInActionInvalid2RoleAnd = $@"
{{
  ""controllers"": [
    {{
      ""name"": ""{SecurityCheck.TestControllerName}"",
      ""roles"": [""{UserInvalidRole}""],
      ""actions"": [
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{UserRole}""]
        }},
        {{
          ""name"": ""{SecurityCheck.TestActionName}"",
          ""roles"": [""{OtherRole}""]
        }}
      ]
    }}
  ]
}}
";
        }
    }
}

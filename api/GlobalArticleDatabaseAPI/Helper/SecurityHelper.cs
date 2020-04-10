using Core.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlobalArticleDatabase.Helpers
{
    public class SecurityHelper
    {
        private static Security _security;

        public static void Initialize(string securityDoc = null)
        {
            try
            {
                if (securityDoc == null)
                {
                    securityDoc = ResourcesHelper.GetResource(Constants.Security.SecurityDocResource);
                }

                _security = StringToSecurity(securityDoc);
            }
            catch (Exception ex)
            {
                throw new InternalException(ExceptionCodes.INTERNAL_ERROR_LOADING_SECURITY_CONFIG, "Error loading security config file for controllers and actions", ex);
            }
        }

        public static bool CheckSecurity(string controllerName, string actionName, List<string> userRoles, string securityDoc = null)
        {
            bool securityPass = false;

            Security security = _security;

            if (!string.IsNullOrEmpty(securityDoc))
            {
                security = StringToSecurity(securityDoc);
            }

            if (security != null && security.Controllers != null)
            {
                bool controllerHasRoles = false;
                bool actionHasRoles = false;

                bool allControllersInRole = true;
                bool allActionsInRole = true;

                // We can have multiple security definitions for the same controller so and AND operation will be applied
                var controllers = security.Controllers
                    .Where(w => w.Name == controllerName)
                    .ToList();

                foreach (var c in controllers)
                {
                    if (c.Roles == null)
                    {
                        allControllersInRole = false;
                    }
                    else
                    {
                        controllerHasRoles = controllerHasRoles || c.Roles.Count() > 0;

                        allControllersInRole = allControllersInRole &&
                            c.Roles != null &&
                            c.Roles.Count() > 0 &&
                            c.Roles.Any(w => userRoles.Contains(w));
                    }

                    if (c.Actions != null)
                    {
                        var actions = c.Actions.Where(w => w.Name == actionName).ToList();

                        foreach (var a in actions)
                        {
                            if (a.Roles == null)
                            {
                                allActionsInRole = false;
                            }
                            else
                            {
                                actionHasRoles = actionHasRoles || a.Roles.Count() > 0;

                                allActionsInRole = allActionsInRole &&
                                    a.Roles != null &&
                                    a.Roles.Count() > 0 &&
                                    a.Roles.Any(w => userRoles.Contains(w));
                            }
                        }
                    }
                }

                securityPass = (controllerHasRoles && allControllersInRole && !actionHasRoles) ||
                    (actionHasRoles && allActionsInRole);
            }

            return securityPass;
        }

        public static bool IsAnonymous(string controllerName, string actionName)
        {
            return CheckSecurity(controllerName, actionName, new List<string> { Constants.Security.Anonymous });
        }

        private static Security StringToSecurity(string securityDoc)
        {
            return JsonConvert.DeserializeObject<Security>(securityDoc);
        }
    }

    internal class Security
    {
        public List<Controller> Controllers { get; set; }
    }

    internal class Controller
    {
        public string Name { get; set; }
        public List<string> Roles { get; set; }
        public List<Action> Actions { get; set; }
    }

    internal class Action
    {
        public string Name { get; set; }
        public List<string> Roles { get; set; }
    }
}

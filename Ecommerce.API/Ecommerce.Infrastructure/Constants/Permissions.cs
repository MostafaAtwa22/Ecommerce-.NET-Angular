namespace Ecommerce.Infrastructure.Constants
{
    public static class Permissions
    {
        public static string ClaimType = "Permission";
        public static string ClaimValue = "Permissions";

        public static List<string> GeneratePermissionsList(string module)
            => new List<string>()
            {
                $"{ClaimValue}.{module}.{CRUD.Read}",
                $"{ClaimValue}.{module}.{CRUD.Create}",
                $"{ClaimValue}.{module}.{CRUD.Update}",
                $"{ClaimValue}.{module}.{CRUD.Delete}"
            };

        public static List<string> GenerateAllPermissions()
        {
            var allPermissions = new List<string>();

            var modules = GetAllModules();

            foreach (var module in modules)
            {
                allPermissions.AddRange(GeneratePermissionsList(module));
            }

            return allPermissions;
        }

        public static List<string> GetAllModules()
        {
            var baseApiControllerType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == "BaseApiController"
                                && t.Namespace == "Ecommerce.API.Controllers");

            if (baseApiControllerType == null)
                return new List<string>();

            var assembly = baseApiControllerType.Assembly;

            return assembly.GetTypes()
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && t.IsSubclassOf(baseApiControllerType)
                            && t.Namespace == "Ecommerce.API.Controllers")
                .Select(t => t.Name.Replace("Controller", ""))
                .ToList();
        }
    }
}
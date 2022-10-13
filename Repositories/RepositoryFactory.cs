using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories
{
    public static class RepositoryFactory
    {
        private static Dictionary<Type, Type> registeredRepositories = new Dictionary<Type, Type>();

        public static ModelRepository<T> Create<T>(bool isReadonly) where T: BaseModel, new()
        {
            if (!registeredRepositories.ContainsKey(typeof(T)))
                return new ModelRepository<T>(isReadonly);

            var repoType = registeredRepositories[typeof(T)];
            var constructor = repoType.GetConstructor(new Type[] { typeof(bool) });
            if (constructor == null)
                return new ModelRepository<T>(isReadonly);

            return (ModelRepository<T>)constructor.Invoke(new object[] { isReadonly });
        }

        public static void RegisterRepositoryClass<T>(Type repoType) where T: BaseModel
        {
            registeredRepositories[typeof(T)] = repoType;
        }
    }
}

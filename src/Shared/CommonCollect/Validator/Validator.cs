using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommonCollect.Validator
{
    public static class Validator
    {
        private static readonly object Locker = new object();

        private static ConcurrentDictionary<string, IValidator> _cacheValidators;

        public static void Register()
        {
            lock (Locker)
            {
                if (_cacheValidators != null)
                {
                    return;
                }
                _cacheValidators = new ConcurrentDictionary<string, IValidator>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (var i = 0; i < assemblies.Length; i++)
                {
                    var list = (from x in assemblies[i].GetTypes()
                                where x.IsPublic && x.BaseType != null && x.BaseType!.Name == typeof(AbstractValidator<>).Name && x.BaseType!.FullName != null
                                select x).ToList();
                    if (list.Count == 0)
                    {
                        continue;
                    }
                    foreach (var item in list)
                    {
                        var type = item.BaseType!.GetGenericArguments().FirstOrDefault();
                        _cacheValidators.TryAdd(type.FullName, (IValidator)Activator.CreateInstance(item));
                    }
                }
            }
        }

        internal static bool IsValid<T>(this T request, out string msg) where T : class
        {
            msg = string.Empty;
            if (request == null)
            {
                return true;
            }
            if (_cacheValidators == null || !_cacheValidators.TryGetValue(request.GetType().FullName, out var value))
            {
                return true;
            }
            var val = value.Validate((IValidationContext)(object)new ValidationContext<T>(request));
            if (!val.IsValid)
            {
                msg = val.Errors[0].ErrorMessage;
                return false;
            }
            return true;
        }
    }
}
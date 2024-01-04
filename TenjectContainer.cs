using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Te.DI
{
    public class TenjectContainer
    {
        private readonly Dictionary<Type, object> _bindings = new();

        public TenjectContainer(bool bindSelf = false)
        {
            if (bindSelf)
            {
                BindInstance(this);
            }
        }

        #region Bind

        public void BindInstance<TObjectType>(TObjectType instance) where TObjectType : class
        {
            BindInstance<TObjectType, TObjectType>(instance);
        }

        public void BindInstance<TObjectType, TInterfaceType>(TObjectType instance) where TObjectType : class, TInterfaceType
        {
            BindInstance(typeof(TInterfaceType), instance);
        }

        public void BindInstance(Type bindType, object instance)
        {
            if (instance == null)
            {
                throw new ArgumentException($"Can't bind null instance to type \"{bindType}\"");
            }

            if (_bindings.ContainsKey(bindType))
            {
                throw new ArgumentException($"Type \"{bindType}\" is already bound");
            }

            _bindings.Add(bindType, instance);

            ResolveInstance(instance, bindType);
        }

        public TObjectType BindNew<TObjectType>() where TObjectType : class
        {
            return BindNew<TObjectType, TObjectType>();
        }

        public TObjectType BindNew<TObjectType, TInterfaceType>() where TObjectType : class, TInterfaceType
        {
            return (TObjectType)BindNew(typeof(TInterfaceType), typeof(TObjectType));
        }

        public object BindNew(Type interfaceType, Type objectType)
        {
            var instance = MakeInstance(objectType);

            BindInstance(interfaceType, instance);

            return instance;
        }

        #endregion

        #region Resolve

        public void ResolveInstance<TObjectType>(TObjectType instance) where TObjectType : class
        {
            ResolveInstance(instance, typeof(TObjectType));
        }

        public void ResolveInstance<TObjectType>(TObjectType instance, Type bindType) where TObjectType : class
        {
            InjectMethods(instance, bindType);
            InjectFields(instance, bindType);
            InjectProperties(instance, bindType);
        }

        public TObjectType ResolveNew<TObjectType>() where TObjectType : class
        {
            return (TObjectType)ResolveNew(typeof(TObjectType));
        }

        public object ResolveNew(Type objectType)
        {
            var instance = MakeInstance(objectType);
            ResolveInstance(instance, objectType);
            return instance;
        }

        #endregion

        #region GetBinding

        public TObjectType GetBinding<TObjectType>() where TObjectType : class
        {
            return (TObjectType)GetBinding(typeof(TObjectType));
        }

        public object GetBinding(Type type)
        {
            return _bindings.TryGetValue(type, out var instance) ? instance : null;
        }

        #endregion

        #region MakeInstance

        private TObjectType MakeInstance<TObjectType>() where TObjectType : class
        {
            return (TObjectType)MakeInstance(typeof(TObjectType));
        }

        private object MakeInstance(Type objectType)
        {
            var constructors = objectType.GetConstructors();
            var injectConstructor = constructors.FirstOrDefault(ctor => ctor.IsDefined(InjectAttribute.Type));

            if (injectConstructor != null)
            {
                var argumentTypes = injectConstructor
                    .GetParameters()
                    .Select(parameterInfo => parameterInfo.ParameterType);
                var arguments = argumentTypes.Select(GetBinding).ToArray();

                return injectConstructor.Invoke(arguments);
            }

            var constructor = constructors.FirstOrDefault();
            if (constructor != null)
            {
                return constructor.Invoke(null);
            }

            throw new ArgumentException($"No default ctor or [Inject]-marked ctor were found for \"{objectType}\" type");
        }

        #endregion

        #region Inject

        private void InjectMethods<TObjectType>(TObjectType instance, Type type) where TObjectType : class
        {
            if (instance == null)
            {
                throw new ArgumentException($"Can't inject methods for null instance of \"{type}\" type");
            }

            var methods = type.GetRuntimeMethods().Where(methodInfo => methodInfo.IsDefined(InjectAttribute.Type));
            foreach (var method in methods)
            {
                var argumentTypes = method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType);
                var arguments = argumentTypes.Select(GetBinding).ToArray();
                method.Invoke(instance, arguments);
            }
        }

        private void InjectFields<TObjectType>(TObjectType instance, Type type) where TObjectType : class
        {
            if (instance == null)
            {
                throw new ArgumentException($"Can't inject fields for null instance of \"{type}\" type");
            }

            var fields = type.GetRuntimeFields().Where(fieldInfo => fieldInfo.IsDefined(InjectAttribute.Type));
            foreach (var field in fields)
            {
                field.SetValue(instance, GetBinding(field.FieldType));
            }
        }

        private void InjectProperties<TObjectType>(TObjectType instance, Type type) where TObjectType : class
        {
            if (instance == null)
            {
                throw new ArgumentException($"Can't inject properties for null instance of \"{type}\" type");
            }

            var properties = type
                .GetRuntimeProperties()
                .Where(propertyInfo => propertyInfo.CanWrite && propertyInfo.IsDefined(InjectAttribute.Type));

            foreach (var property in properties)
            {
                property.SetValue(instance, GetBinding(property.PropertyType));
            }
        }

        #endregion
    }
}
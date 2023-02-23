using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace BlackJackAOP
{
    [NonInterceptable]
    internal class ExpressionInterceptorProvider : InterceptorProviderBase, IInterceptorRegistry
    {
        #region Fields
        private readonly IConventionalInterceptorFactory _conventionalInterceptorFactory;
        private readonly Dictionary<Type, List<Func<Sortable<InvokeDelegate>>>> _interceptorsAccessors4Type = new();
        private readonly Dictionary<Tuple<int,MethodInfo>, List<Func<Sortable<InvokeDelegate>>>> _interceptorsAccessors4Method = new();
        private readonly Dictionary<Tuple<int,MethodInfo>, List<Sortable<InvokeDelegate>>> _interceptors = new();
        private readonly HashSet<Type> _suppressedTypes = new();
        private readonly HashSet<MethodInfo> _suppressedMethods = new();
        #endregion

        #region Constructors
        public ExpressionInterceptorProvider(IConventionalInterceptorFactory conventionalInterceptorFactory, IOptions<InterceptionOptions> optionsAccessor) : base(conventionalInterceptorFactory)
        {
            _conventionalInterceptorFactory = conventionalInterceptorFactory ?? throw new ArgumentNullException(nameof(conventionalInterceptorFactory));
            var registrations = (optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor))).Value.InterceptorRegistrations;
            registrations?.Invoke(this);
        }


        #endregion

        #region Public methods
        public override bool CanIntercept(Type targetType, MethodInfo method, out bool suppressed)
        {
            Guard.ArgumentNotNull(targetType);
            Guard.ArgumentNotNull(method);
            if (_suppressedTypes.Contains(targetType) || _suppressedMethods.Contains(method))
            {
                suppressed = true;
                return false;
            }
            suppressed = false;
            var a = _interceptorsAccessors4Type.ContainsKey(targetType) || _interceptorsAccessors4Method.ContainsKey(new Tuple<int, MethodInfo>(method.MetadataToken, method));
            if (a == false)
            {

            }
                
            return _interceptorsAccessors4Type.ContainsKey(targetType) || _interceptorsAccessors4Method.ContainsKey(new Tuple<int,MethodInfo>(method.MetadataToken,method));
        }
        public IInterceptorRegistry<TInterceptor> For<TInterceptor>(params object[] arguments)
            => new InterceptorRegistry<TInterceptor>(_interceptorsAccessors4Type, _interceptorsAccessors4Method, () => _conventionalInterceptorFactory.CreateInterceptor(typeof(TInterceptor), arguments));
        public override IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method)
        {
            Guard.ArgumentNotNull(targetType);
            Guard.ArgumentNotNull(method);

            var key =new Tuple<int,MethodInfo>(method.MetadataToken, method);
            if (_interceptors.TryGetValue(key, out var interceptors))
            {
                return interceptors;
            }

            if (_interceptorsAccessors4Type.TryGetValue(targetType, out var interceptorAccessors))
            {
                interceptors = interceptorAccessors.Select(it => it()).ToList();
            }

            if (_interceptorsAccessors4Method.TryGetValue(key, out interceptorAccessors))
            {
                if (interceptors is null)
                {
                    interceptors = interceptorAccessors.Select(it => it()).ToList();
                }
                else
                {
                    interceptors.AddRange(interceptorAccessors.Select(it => it()));
                }
            }

            if (interceptors?.Any() ?? false)
            {
                _interceptors[key] = interceptors;
                return interceptors;
            }

            return Enumerable.Empty<Sortable<InvokeDelegate>>();
        }

        public IInterceptorRegistry SupressGetMethod<TTarget>(Expression<Func<TTarget, object?>> propertyAccessor)
        {
            var property = MemberUtilities.GetProperty(propertyAccessor);
            var method = property.GetMethod;
            if (method is not null)
            {
                _suppressedMethods.Add(method);
                return this;
            }
            throw new ArgumentException($"{typeof(TTarget)}的{property.Name}属性没有get访问器。", nameof(propertyAccessor));
        }
        public IInterceptorRegistry SupressMethod<TTarget>(Expression<Action<TTarget>> methodCall)
        {
            if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));
            var method = MemberUtilities.GetMethod(methodCall);
            _suppressedMethods.Add(method);
            return this;
        }
        public IInterceptorRegistry SupressMethods(params MethodInfo[] methods)
        {
            Array.ForEach(methods, it => _suppressedMethods.Add(it));
            return this;
        }
        public IInterceptorRegistry SupressProperty<TTarget>(Expression<Func<TTarget, object?>> propertyAccessor)
        {
            var property = MemberUtilities.GetProperty(propertyAccessor);
            var method = property.GetMethod;
            if (method is not null)
            {
                _suppressedMethods.Add(method);
            }
            method = property.SetMethod;
            if (method is not null)
            {
                _suppressedMethods.Add(method);
            }
            return this;
        }
        public IInterceptorRegistry SupressSetMethod<TTarget>(Expression<Func<TTarget, object?>> propertyAccessor)
        {
            var property = MemberUtilities.GetProperty(propertyAccessor);
            var method = property.SetMethod;
            if (method is not null)
            {
                _suppressedMethods.Add(method);
                return this;
            }
            throw new ArgumentException($"{typeof(TTarget)}的{property.Name}属性没有set访问器。", nameof(propertyAccessor));
        }
        public IInterceptorRegistry SupressType<TTarget>()
        {
            _suppressedTypes.Add(typeof(TTarget));
            return this;
        }
        public IInterceptorRegistry SupressTypes(params Type[] types)
        {
            Array.ForEach(types, it => _suppressedTypes.Add(it));
            return this;
        }
        #endregion

        #region Private methods
        private sealed class InterceptorRegistry<TInterceptor> : IInterceptorRegistry<TInterceptor>
        {
            private readonly Dictionary<Type, List<Func<Sortable<InvokeDelegate>>>> _interceptorAccessors4Type;
            private readonly Dictionary<Tuple<int,MethodInfo>, List<Func<Sortable<InvokeDelegate>>>> _interceptorAccessors4Method;
            private readonly Func<InvokeDelegate> _interceptorFatory;
            public InterceptorRegistry(Dictionary<Type, List<Func<Sortable<InvokeDelegate>>>> interceptorAccessors4Type, Dictionary<Tuple<int, MethodInfo>, List<Func<Sortable<InvokeDelegate>>>> interceptorAccessors4Method, Func<InvokeDelegate> interceptorFatory)
            {
                _interceptorAccessors4Type = interceptorAccessors4Type;
                _interceptorAccessors4Method = interceptorAccessors4Method;
                //var lazy = new Lazy<InvokeDelegate>(() => interceptorFatory());
                //_interceptorFatory = () => lazy.Value;
                _interceptorFatory = interceptorFatory;
            }

            public IInterceptorRegistry<TInterceptor> ToMethod<TTarget>(int order, Expression<Action<TTarget>> methodCall)
            {
                if (methodCall == null)
                {
                    throw new ArgumentNullException(nameof(methodCall));
                }

                var method = MemberUtilities.GetMethod(methodCall);
                return ToMethods<TTarget>(order, method);
            }

            public IInterceptorRegistry<TInterceptor> ToGetMethod<TTarget>(int order, Expression<Func<TTarget, object?>> propertyAccessor)
            {
                if (propertyAccessor == null)
                {
                    throw new ArgumentNullException(nameof(propertyAccessor));
                }
                var property = MemberUtilities.GetProperty(propertyAccessor);
                var getMethod = property.GetMethod;
                if (getMethod is null)
                {
                    throw new ArgumentException($"{typeof(TTarget)}的{property.Name}属性没有get访问器。", nameof(propertyAccessor));
                }
                return ToMethods<TTarget>(order, getMethod);
            }

            public IInterceptorRegistry<TInterceptor> ToSetMethod<TTarget>(int order, Expression<Func<TTarget, object?>> propertyAccessor)
            {
                if (propertyAccessor == null)
                {
                    throw new ArgumentNullException(nameof(propertyAccessor));
                }
                var property = MemberUtilities.GetProperty(propertyAccessor);
                var setMethod = property.SetMethod;
                if (setMethod is null)
                {
                    throw new ArgumentException($"{typeof(TTarget)}的{property.Name}属性没有set访问器。", nameof(propertyAccessor));
                }
                return ToMethods<TTarget>(order, setMethod);
            }

            public IInterceptorRegistry<TInterceptor> ToProperty<TTarget>(int order, Expression<Func<TTarget, object?>> propertyAccessor)
            {
                if (propertyAccessor == null)
                {
                    throw new ArgumentNullException(nameof(propertyAccessor));
                }
                var property = MemberUtilities.GetProperty(propertyAccessor);
                var method = property.GetMethod;
                var valid = false;
                if (method is not null && MemberUtilities.IsInterfaceOrVirtualMethod(method))
                {
                    ToMethods<TTarget>(order, method);
                    valid = true;
                }

                method = property.SetMethod;
                if (method is not null && MemberUtilities.IsInterfaceOrVirtualMethod(method))
                {
                    ToMethods<TTarget>(order, method);
                    valid = true;
                }

                if (!valid)
                {
                    throw new InterceptionException($"不能对{typeof(TTarget)}的{property.Name}属性进行拦截，必须是虚方法或者接口的实现方法。");
                }

                return this;
            }           

            public IInterceptorRegistry<TInterceptor> ToMethods<TTarget>(int order, params MethodInfo[] methods)
            {
                var targetType = typeof(TTarget);
                if (!targetType.IsPublic && !targetType.IsNestedPublic)
                {
                    throw new InterceptionException($"{targetType}必须是公开的类型。");
                }

                foreach (var method in methods)
                {
                    if (!MemberUtilities.IsInterfaceOrVirtualMethod(method))
                    {
                        throw new InterceptionException($"不能对{targetType}的{method.Name}方法进行拦截，必须是虚方法或者接口的实现方法。");
                    }
                }

                foreach (var method in methods)
                {
                    var key =new Tuple<int,MethodInfo>(method.MetadataToken,method);
                    var list = _interceptorAccessors4Method.TryGetValue(key, out var value)
                        ? value
                        : _interceptorAccessors4Method[key] = new List<Func<Sortable<InvokeDelegate>>>();
                    list.Add(() => new Sortable<InvokeDelegate>(order, _interceptorFatory()));
                }
                return this;
            }

            public IInterceptorRegistry<TInterceptor> ToAllMethods<TTarget>(int order)
            {
                var targetType = typeof(TTarget);
                if (!targetType.IsPublic && !targetType.IsNested)
                {
                    throw new InterceptionException($"{targetType}必须是公开的类型。");
                }
                var list = _interceptorAccessors4Type.TryGetValue(targetType, out var value)
                   ? value
                   : _interceptorAccessors4Type[targetType] = new List<Func<Sortable<InvokeDelegate>>>();
                list.Add(() => new Sortable<InvokeDelegate>(order, _interceptorFatory()));
                return this;
            }
        }
        #endregion
    }
}

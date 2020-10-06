using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveBinderGenerators
{
    static class Consts
    {
        public static string ReactiveProperty { get; } = "Reactive.Bindings.ReactiveProperty<{0}>";
        public static string MakeReactivePropertyOf(string type) => string.Format(ReactiveProperty, type);
        public static string MarkerAttributeNamespace { get; } = nameof(ReactiveBinderGenerators);
        public static string MarkerAttributeName { get; } = "BindFromAttribute";
        public static string MarkerAttributeFullName { get; } = $"{MarkerAttributeNamespace}.{MarkerAttributeName}";
        public static string MarkerAttributeCode { get; } = $@"using System;
namespace {MarkerAttributeNamespace}
{{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    class {MarkerAttributeName} : Attribute 
    {{
        public BindFromAttribute(Type fromType)
        {{
            FromType = fromType;
        }}

        public Type FromType {{ get; }}
    }}
}}";
    }

}

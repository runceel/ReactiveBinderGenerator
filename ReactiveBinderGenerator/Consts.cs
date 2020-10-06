using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveBinderGenerators
{
    static class Consts
    {
        public static string MarkerAttributeCode { get; } = $@"using System;
namespace ReactiveBinderGenerators
{{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    class BindFromAttribute : Attribute 
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

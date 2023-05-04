// <copyright file="TypeHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using System.Reflection.Emit;

namespace OnlineSales.Infrastructure
{
    public static class TypeHelper
    {
        public static Type CompileTypeForSelectStatement(PropertyInfo[] types)
        {
            var tb = GetTypeBuilder();
            // ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
            foreach (var field in types)
            {
                CreateProperty(tb, field.Name, field.PropertyType);
            }

            var objectType = tb.CreateType();
            return objectType;
        }

        private static TypeBuilder GetTypeBuilder()
        {
            var typeSignature = "MyDynamicType";
            var an = new AssemblyName(typeSignature);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeAttributes =
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout;

            var tb = moduleBuilder.DefineType(typeSignature, typeAttributes, null);
            return tb;
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            var fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var getPropMthdBldr = tb.DefineMethod(propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            var methodName = propertyName;
            var methodAttributes = MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig;

            var setPropMthdBldr = tb.DefineMethod(methodName, methodAttributes, null, new[] { propertyType });

            var setIl = setPropMthdBldr.GetILGenerator();
            var modifyProperty = setIl.DefineLabel();
            var exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }
}

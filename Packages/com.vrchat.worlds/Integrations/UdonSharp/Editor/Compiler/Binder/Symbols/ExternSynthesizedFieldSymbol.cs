using System;
using Microsoft.CodeAnalysis;
using UdonSharp.Compiler.Udon;

namespace UdonSharp.Compiler.Symbols
{
    internal class ExternSynthesizedFieldSymbol : ExternFieldSymbol
    {
        public ExternSynthesizedFieldSymbol(IFieldSymbol templateSymbol, AbstractPhaseContext context, string fieldName, TypeSymbol containingType, TypeSymbol valueType)
            : base(templateSymbol, context)
        {
            ExternGetSignature = BuildExternSignature(containingType, valueType, fieldName, CompilerUdonInterface.FieldAccessorType.Get);
            ExternSetSignature = BuildExternSignature(containingType, valueType, fieldName, CompilerUdonInterface.FieldAccessorType.Set);
        }

        private string BuildExternSignature(TypeSymbol containingType, TypeSymbol valueType, string fieldName, CompilerUdonInterface.FieldAccessorType accessorType)
        {
            Type fieldSourceType = containingType.UdonType.SystemType;

            fieldSourceType = UdonSharpUtils.RemapBaseType(fieldSourceType);

            string memberNamespace = CompilerUdonInterface.SanitizeTypeName(fieldSourceType.FullName ?? fieldSourceType.Namespace + fieldSourceType.Name);

            if (accessorType == CompilerUdonInterface.FieldAccessorType.Get)
            {
                fieldName = $"__get_{fieldName}";
            }
            else
            {
                fieldName = $"__set_{fieldName}";
            }

            string returnStr = $"__{CompilerUdonInterface.GetUdonTypeName(valueType)}";

            string finalFunctionSig = $"{memberNamespace}.{fieldName}{returnStr}";

            return finalFunctionSig;
        }
    }
}
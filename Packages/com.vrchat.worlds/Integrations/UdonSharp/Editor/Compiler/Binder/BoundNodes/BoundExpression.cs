
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using UdonSharp.Compiler.Emit;
using UdonSharp.Compiler.Symbols;
using UdonSharp.Compiler.Udon;
using UdonSharp.Core;
using UdonSharp.Localization;

namespace UdonSharp.Compiler.Binder
{
    internal abstract class BoundExpression : BoundNode
    {
        public bool IsConstant => ConstantValue != null;
        public virtual IConstantValue ConstantValue => null;

        /// <summary>
        /// The user type of Value that this expression will emit when EmitValue is called
        /// </summary>
        public abstract TypeSymbol ValueType { get; }
        
        public bool IsThis { get; protected set; }

        protected BoundExpression SourceExpression { get; }

        protected BoundExpression(SyntaxNode node, BoundExpression sourceExpression = null)
            : base(node)
        {
            SourceExpression = sourceExpression;
        }

        /// <summary>
        /// All expressions must instead implement EmitValue since they will always evaluate to something
        /// </summary>
        /// <param name="context"></param>
        public override void Emit(EmitContext context)
        {
            context.EmitValue(this);
        }

        public abstract Value EmitValue(EmitContext context);
        
        protected virtual void ReleaseCowValuesImpl(EmitContext context) {}
        
        public void ReleaseCowReferences(EmitContext context)
        {
            ReleaseCowValuesImpl(context);
            context.ReleaseCowValues(this);
            SourceExpression?.ReleaseCowReferences(context);
        }

        protected ExternFieldSymbol SetupExternAccessor(SyntaxNode node, AbstractPhaseContext context, ExternFieldSymbol externFieldAccessor,
            BoundExpression sourceExpression, CompilerUdonInterface.FieldAccessorType accessorType,
            Func<TypeSymbol, ExternFieldSymbol> synthesizedFieldSymbolFactory)
        {
            string sig = accessorType == CompilerUdonInterface.FieldAccessorType.Get ? externFieldAccessor.ExternGetSignature : externFieldAccessor.ExternSetSignature;

            if (!CompilerUdonInterface.IsExposedToUdon(sig))
            {
                ExternFieldSymbol externAlternateAccessor = FindAlternateAccessor(context, externFieldAccessor, sourceExpression, accessorType, synthesizedFieldSymbolFactory);
                if (externAlternateAccessor == null)
                {
                    throw new NotExposedException(LocStr.CE_UdonFieldNotExposed, node, $"{externFieldAccessor.RoslynSymbol?.ToDisplayString() ?? externFieldAccessor.ToString()}, sig: {sig}");
                }
                else
                {
                    return externAlternateAccessor;
                }
            }

            return externFieldAccessor;
        }

        protected ExternMethodSymbol SetupExternMethodSymbol(SyntaxNode node, AbstractPhaseContext context, ExternMethodSymbol methodSymbol,
            BoundExpression instanceExpression, BoundExpression[] parameterExpressions)
        {
            if (!CompilerUdonInterface.IsExposedToUdon(methodSymbol.ExternSignature))
            {
                ExternMethodSymbol externAlternateMethodSymbol = FindAlternateInvocation(context, methodSymbol, instanceExpression, parameterExpressions);
                if (externAlternateMethodSymbol == null)
                {
                    throw new NotExposedException(LocStr.CE_UdonMethodNotExposed, node, $"{methodSymbol.RoslynSymbol?.ToDisplayString() ?? methodSymbol.ToString()}, sig: {methodSymbol.ExternSignature}");
                }
                else
                {
                    return externAlternateMethodSymbol;
                }
            }

            return methodSymbol;
        }

        private static ExternFieldSymbol FindAlternateAccessor(AbstractPhaseContext context, ExternFieldSymbol originalFieldSymbol,
            BoundExpression sourceExpression, CompilerUdonInterface.FieldAccessorType accessorType,
            Func<TypeSymbol, ExternFieldSymbol> synthesizedFieldSymbolFactory)
        {
            if (originalFieldSymbol.IsStatic) return null;

            List<TypeSymbol> candidates = new List<TypeSymbol>();
            FindCandidateAlternateTypes(context, candidates, sourceExpression?.ValueType ?? originalFieldSymbol.ContainingType);

            foreach (TypeSymbol candidate in candidates)
            {
                ExternFieldSymbol externFieldSymbol = synthesizedFieldSymbolFactory(candidate);
                string sig = accessorType == CompilerUdonInterface.FieldAccessorType.Get ? externFieldSymbol.ExternGetSignature : externFieldSymbol.ExternSetSignature;
                if (CompilerUdonInterface.IsExposedToUdon(sig))
                {
                    return externFieldSymbol;
                }
            }

            return null;
        }

        private static ExternMethodSymbol FindAlternateInvocation(AbstractPhaseContext context,
            MethodSymbol originalSymbol, BoundExpression instanceExpression, BoundExpression[] parameterExpressions)
        {
            if (originalSymbol.IsStatic || originalSymbol.IsConstructor) return null;

            List<TypeSymbol> candidates = new List<TypeSymbol>();
            FindCandidateAlternateTypes(context, candidates, instanceExpression.ValueType);

            TypeSymbol[] paramTypes = parameterExpressions.Select(ex => ex.ValueType).ToArray();

            foreach (TypeSymbol candidate in candidates)
            {
                ExternMethodSymbol externMethodSymbol = new ExternSynthesizedMethodSymbol(context, originalSymbol.Name, candidate, paramTypes, originalSymbol.ReturnType, false, false);
                if (CompilerUdonInterface.IsExposedToUdon(externMethodSymbol.ExternSignature))
                {
                    return externMethodSymbol;
                }
            }

            return null;
        }

        private static void FindCandidateAlternateTypes(AbstractPhaseContext context, List<TypeSymbol> candidates, TypeSymbol ty)
        {
            foreach (var intf in ty.RoslynSymbol.AllInterfaces)
            {
                candidates.Add(context.GetTypeSymbol(intf));
            }

            while (ty != null)
            {
                candidates.Add(ty);
                ty = ty.BaseType;
            }
        }
    }

    internal class BoundConstantExpression : BoundAccessExpression
    {
        public override IConstantValue ConstantValue { get; }

        public TypeSymbol ConstantType { get; }

        public override TypeSymbol ValueType => ConstantType;

        public BoundConstantExpression(IConstantValue constantValue, TypeSymbol constantType, SyntaxNode node)
            :base(node, null)
        {
            ConstantValue = constantValue;
            ConstantType = constantType;
        }

        public BoundConstantExpression(object constantValue, TypeSymbol typeSymbol)
            :base(null, null)
        {
            ConstantType = typeSymbol;

            Type targetType = typeSymbol.UdonType.SystemType;

            if (typeSymbol.IsEnum && typeSymbol.IsExtern)
                constantValue = Enum.ToObject(targetType, constantValue);
            
            ConstantValue =
                (IConstantValue) Activator.CreateInstance(typeof(ConstantValue<>).MakeGenericType(typeSymbol.UdonType.SystemType),
                    constantValue);
        }
        
        public BoundConstantExpression(object constantValue, TypeSymbol typeSymbol, SyntaxNode node)
            :base(null, null)
        {
            ConstantType = typeSymbol;

            Type targetType = typeSymbol.UdonType.SystemType;

            if (typeSymbol.IsEnum && typeSymbol.IsExtern)
                constantValue = Enum.ToObject(targetType, constantValue);
            
            ConstantValue =
                (IConstantValue) Activator.CreateInstance(typeof(ConstantValue<>).MakeGenericType(typeSymbol.UdonType.SystemType),
                    constantValue);
        }

        public override Value EmitValue(EmitContext context)
        {
            return context.GetConstantValue(ConstantType, ConstantValue.Value);
        }

        public override string ToString()
        {
            return $"BoundConstantExpression<{ConstantValue.GetType().GetGenericArguments()[0]}>: " + ConstantValue.Value?.ToString() ?? "null";
        }

        public override Value EmitSet(EmitContext context, BoundExpression valueExpression)
        {
            throw new InvalidOperationException("Cannot set value on a constant value");
        }
    }
}

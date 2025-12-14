
using Microsoft.CodeAnalysis;
using UdonSharp.Compiler.Emit;
using UdonSharp.Compiler.Symbols;
using UdonSharp.Compiler.Udon;

namespace UdonSharp.Compiler.Binder
{
    internal sealed class BoundAssignmentExpression : BoundExpression
    {
        private BoundAccessExpression TargetExpression { get; }

        public override TypeSymbol ValueType => SourceExpression.ValueType;

        public BoundAssignmentExpression(SyntaxNode node, AbstractPhaseContext context, Symbol symbol,
            BoundAccessExpression assignmentTarget, BoundExpression instanceExpression, BoundExpression assignmentSource)
            : base(node, assignmentSource)
        {
            if (symbol is ExternFieldSymbol externFieldSymbol)
            {
                // This is assignment, so check for set only
                // Passing originalSymbol.RoslynSymbol to synthesized fields to inherit attributes (static, const, etc.)
                TypeSymbol assignmentTargetTypeSymbol = assignmentTarget.ValueType;
                ExternFieldSymbol externAlternateFieldSymbol = SetupExternAccessor(node, context, externFieldSymbol, instanceExpression, CompilerUdonInterface.FieldAccessorType.Set,
                    (candidate) => new ExternSynthesizedFieldSymbol(externFieldSymbol.RoslynSymbol, context, externFieldSymbol.Name, candidate, assignmentTargetTypeSymbol));

                // Was an alternate found?
                if (externAlternateFieldSymbol != externFieldSymbol)
                {
                    // Ensure the emitted assembly points at the correct left-hand type.
                    assignmentTarget = BoundFieldAccessExpression.BindFieldAccess(context, node, externAlternateFieldSymbol, instanceExpression);
                }
            }

            TargetExpression = assignmentTarget;
        }

        public override Value EmitValue(EmitContext context)
        {
            return context.EmitSet(TargetExpression, SourceExpression);
        }
    }
}

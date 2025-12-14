
using Microsoft.CodeAnalysis;
using UdonSharp.Compiler.Udon;

namespace UdonSharp.Compiler.Symbols
{
    internal class ExternFieldSymbol : FieldSymbol, IExternAccessor
    {
        public ExternFieldSymbol(IFieldSymbol sourceSymbol, AbstractPhaseContext context)
            : base(sourceSymbol, context)
        {
            Type = context.GetTypeSymbol(sourceSymbol.Type);
        }

        public override bool IsExtern => true;

        public override bool IsBound => true;

        private string _externSetSignature;

        public string ExternSetSignature
        {
            get
            {
                if (_externSetSignature == null)
                {
                    _externSetSignature = CompilerUdonInterface.GetUdonAccessorName(this, CompilerUdonInterface.FieldAccessorType.Set);
                }

                return _externSetSignature;
            }
            protected set
            {
                _externSetSignature = value;
            }
        }

        private string _externGetSignature;

        public string ExternGetSignature
        {
            get
            {
                if (_externGetSignature == null)
                {
                    _externGetSignature = CompilerUdonInterface.GetUdonAccessorName(this, CompilerUdonInterface.FieldAccessorType.Get);
                }

                return _externGetSignature;
            }
            protected set
            {
                _externGetSignature = value;
            }
        }
    }
}

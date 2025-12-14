using System;
using Unity.IL2CPP.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VRC.Udon.Common.Attributes;
using VRC.Udon.Common.Delegates;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Security.Interfaces;
using VRC.Udon.Wrapper.Modules;

#pragma warning disable CS0618 // Type or member is obsolete
// ReSharper disable NotAccessedField.Local
// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantNameQualifier
// ReSharper disable StringLiteralTypo

[assembly: UdonWrapperModule(typeof(ExternUnityEngineInput))]
namespace VRC.Udon.Wrapper.Modules
{
    [ExcludeFromCodeCoverage]
    public class ExternUnityEngineInput : IUdonWrapperModule
    {
        public string Name => "UnityEngineInput";
        
        public HashSet<Func<bool>> FreezeInputFuncs = new HashSet<Func<bool>>();
        private enum FreezeInputState
        {
            NotFrozen = 0,
            JustFrozen = 1,
            Frozen = 2,
            JustUnfrozen = 3
        }
        
        private int _lastFreezeInputCheckFrame = -1;
        private FreezeInputState _lastFreezeState = FreezeInputState.NotFrozen;
        private bool _lastShouldFreeze = false;
        bool FreezeInput(out FreezeInputState frozenState) {
            if (_lastFreezeInputCheckFrame == UnityEngine.Time.frameCount)
            {
                frozenState = _lastFreezeState;
                return _lastShouldFreeze;
            }
                
            _lastFreezeInputCheckFrame = UnityEngine.Time.frameCount;
            bool shouldFreeze = FreezeInputFuncs.Count > 0; //if no FreezeInputFuncs, should never freeze
            foreach (var func in FreezeInputFuncs) //return false if ANY func returns false
            {
                if (!func())
                {
                    shouldFreeze = false;
                    break;
                }
            }
                
            if (shouldFreeze && !_lastShouldFreeze)
                _lastFreezeState = FreezeInputState.JustFrozen;
            else if (!shouldFreeze && _lastShouldFreeze)
                _lastFreezeState = FreezeInputState.JustUnfrozen;
            else if (shouldFreeze)
                _lastFreezeState = FreezeInputState.Frozen;
            else
                _lastFreezeState = FreezeInputState.NotFrozen;
        
            _lastShouldFreeze = shouldFreeze;
            frozenState = _lastFreezeState;
            return shouldFreeze;
        }
        
        private readonly Lazy<Dictionary<string, int>> _parameterCounts;
        private readonly Lazy<Dictionary<string, UdonExternDelegate>> _functionDelegates;
        private readonly IUdonComponentGetter _componentGetter;
        private readonly IUdonSecurityFilter<UnityEngine.Object> _filter;
        
        public ExternUnityEngineInput(IUdonComponentGetter componentGetter, IUdonSecurityFilter<UnityEngine.Object> filter)
        {
            _componentGetter = componentGetter;
            _filter = filter;
            _parameterCounts = new Lazy<Dictionary<string, int>>(() => new Dictionary<string, int>
            {
                {"__Equals__SystemObject__SystemBoolean", 3},
                {"__GetAxisRaw__SystemString__SystemSingle", 2},
                {"__GetAxis__SystemString__SystemSingle", 2},
                {"__GetButtonDown__SystemString__SystemBoolean", 2},
                {"__GetButtonUp__SystemString__SystemBoolean", 2},
                {"__GetButton__SystemString__SystemBoolean", 2},
                {"__GetHashCode__SystemInt32", 2},
                {"__GetJoystickNames__SystemStringArray", 1},
                {"__GetKeyDown__SystemString__SystemBoolean", 2},
                {"__GetKeyDown__UnityEngineKeyCode__SystemBoolean", 2},
                {"__GetKeyUp__SystemString__SystemBoolean", 2},
                {"__GetKeyUp__UnityEngineKeyCode__SystemBoolean", 2},
                {"__GetKey__SystemString__SystemBoolean", 2},
                {"__GetKey__UnityEngineKeyCode__SystemBoolean", 2},
                {"__GetMouseButtonDown__SystemInt32__SystemBoolean", 2},
                {"__GetMouseButtonUp__SystemInt32__SystemBoolean", 2},
                {"__GetMouseButton__SystemInt32__SystemBoolean", 2},
                {"__GetType__SystemType", 2},
                {"__ToString__SystemString", 2},
                {"__ctor____UnityEngineInput", 1},
                {"__get_anyKeyDown__SystemBoolean", 1},
                {"__get_anyKey__SystemBoolean", 1},
                {"__get_imeIsSelected__SystemBoolean", 1},
                {"__get_inputString__SystemString", 1},
            });
            
            _functionDelegates = new Lazy<Dictionary<string, UdonExternDelegate>>(() => new Dictionary<string, UdonExternDelegate>()
            {
                {"__Equals__SystemObject__SystemBoolean", __Equals__SystemObject__SystemBoolean},
                {"__GetAxisRaw__SystemString__SystemSingle", __GetAxisRaw__SystemString__SystemSingle},
                {"__GetAxis__SystemString__SystemSingle", __GetAxis__SystemString__SystemSingle},
                {"__GetButtonDown__SystemString__SystemBoolean", __GetButtonDown__SystemString__SystemBoolean},
                {"__GetButtonUp__SystemString__SystemBoolean", __GetButtonUp__SystemString__SystemBoolean},
                {"__GetButton__SystemString__SystemBoolean", __GetButton__SystemString__SystemBoolean},
                {"__GetHashCode__SystemInt32", __GetHashCode__SystemInt32},
                {"__GetJoystickNames__SystemStringArray", __GetJoystickNames__SystemStringArray},
                {"__GetKeyDown__SystemString__SystemBoolean", __GetKeyDown__SystemString__SystemBoolean},
                {"__GetKeyDown__UnityEngineKeyCode__SystemBoolean", __GetKeyDown__UnityEngineKeyCode__SystemBoolean},
                {"__GetKeyUp__SystemString__SystemBoolean", __GetKeyUp__SystemString__SystemBoolean},
                {"__GetKeyUp__UnityEngineKeyCode__SystemBoolean", __GetKeyUp__UnityEngineKeyCode__SystemBoolean},
                {"__GetKey__SystemString__SystemBoolean", __GetKey__SystemString__SystemBoolean},
                {"__GetKey__UnityEngineKeyCode__SystemBoolean", __GetKey__UnityEngineKeyCode__SystemBoolean},
                {"__GetMouseButtonDown__SystemInt32__SystemBoolean", __GetMouseButtonDown__SystemInt32__SystemBoolean},
                {"__GetMouseButtonUp__SystemInt32__SystemBoolean", __GetMouseButtonUp__SystemInt32__SystemBoolean},
                {"__GetMouseButton__SystemInt32__SystemBoolean", __GetMouseButton__SystemInt32__SystemBoolean},
                {"__GetType__SystemType", __GetType__SystemType},
                {"__ToString__SystemString", __ToString__SystemString},
                {"__ctor____UnityEngineInput", __ctor____UnityEngineInput},
                {"__get_anyKeyDown__SystemBoolean", __get_anyKeyDown__SystemBoolean},
                {"__get_anyKey__SystemBoolean", __get_anyKey__SystemBoolean},
                {"__get_imeIsSelected__SystemBoolean", __get_imeIsSelected__SystemBoolean},
                {"__get_inputString__SystemString", __get_inputString__SystemString},
            });
        }
        
        public int GetExternFunctionParameterCount(string externFunctionSignature)
        {
           if(_parameterCounts.Value.TryGetValue(externFunctionSignature, out int numParameters))
           {
               return numParameters;
           }
        
           throw new System.NotSupportedException($"Function '{externFunctionSignature}' is not implemented yet");
        }
        
        public UdonExternDelegate GetExternFunctionDelegate(string externFunctionSignature)
        {
            if(_functionDelegates.Value.TryGetValue(externFunctionSignature, out UdonExternDelegate externDelegate))
            {
                return externDelegate;
            }
        
            throw new System.NotSupportedException($"Function '{externFunctionSignature}' is not implemented yet");
        }
        
        
        private void __Equals__SystemObject__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Object var_0 = heap.GetHeapVariable<System.Object>(parameterAddresses[0]);
            #if !UDON_DISABLE_SECURITY
            _filter.ApplyFilter(ref var_0);
            #endif
            
            System.Object var_1 = heap.GetHeapVariable<System.Object>(parameterAddresses[1]);
            #if !UDON_DISABLE_SECURITY
            _filter.ApplyFilter(ref var_1);
            #endif
            
            System.Boolean var_2 = var_0.Equals(var_1);
            
            heap.SetHeapVariable(parameterAddresses[2], var_2);
        }
        
        private void __GetAxisRaw__SystemString__SystemSingle(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String var_0 = heap.GetHeapVariable<System.String>(parameterAddresses[0]);
            System.Single var_1 = FreezeInput(out _) ? 0f : UnityEngine.Input.GetAxisRaw(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetAxis__SystemString__SystemSingle(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String var_0 = heap.GetHeapVariable<System.String>(parameterAddresses[0]);
            System.Single var_1 = FreezeInput(out _) ? 0f : UnityEngine.Input.GetAxis(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetButtonDown__SystemString__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String var_0 = heap.GetHeapVariable<System.String>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out var freezeState) ? false : UnityEngine.Input.GetButtonDown(var_0) || (freezeState == FreezeInputState.JustUnfrozen && UnityEngine.Input.GetButton(var_0));
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetButtonUp__SystemString__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String var_0 = heap.GetHeapVariable<System.String>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out var freezeState) ? freezeState == FreezeInputState.JustFrozen && (UnityEngine.Input.GetButton(var_0) || UnityEngine.Input.GetButtonUp(var_0)) : UnityEngine.Input.GetButtonUp(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetButton__SystemString__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String var_0 = heap.GetHeapVariable<System.String>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out _) ? false : UnityEngine.Input.GetButton(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetHashCode__SystemInt32(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Object var_0 = heap.GetHeapVariable<System.Object>(parameterAddresses[0]);
            #if !UDON_DISABLE_SECURITY
            _filter.ApplyFilter(ref var_0);
            #endif
            
            System.Int32 var_1 = var_0.GetHashCode();
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetJoystickNames__SystemStringArray(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String[] var_0 = UnityEngine.Input.GetJoystickNames();
            
            heap.SetHeapVariable(parameterAddresses[0], var_0);
        }
        
        private void __GetKeyDown__SystemString__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String var_0 = heap.GetHeapVariable<System.String>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out var freezeState) ? false : UnityEngine.Input.GetKeyDown(var_0) || (freezeState == FreezeInputState.JustUnfrozen && UnityEngine.Input.GetKey(var_0));
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetKeyDown__UnityEngineKeyCode__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            UnityEngine.KeyCode var_0 = heap.GetHeapVariable<UnityEngine.KeyCode>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out var freezeState) ? false : UnityEngine.Input.GetKeyDown(var_0) || (freezeState == FreezeInputState.JustUnfrozen && UnityEngine.Input.GetKey(var_0));
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetKeyUp__SystemString__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String var_0 = heap.GetHeapVariable<System.String>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out var freezeState) ? freezeState == FreezeInputState.JustFrozen && (UnityEngine.Input.GetKey(var_0) || UnityEngine.Input.GetKeyUp(var_0)) : UnityEngine.Input.GetKeyUp(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetKeyUp__UnityEngineKeyCode__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            UnityEngine.KeyCode var_0 = heap.GetHeapVariable<UnityEngine.KeyCode>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out var freezeState) ? freezeState == FreezeInputState.JustFrozen && (UnityEngine.Input.GetKey(var_0) || UnityEngine.Input.GetKeyUp(var_0)) : UnityEngine.Input.GetKeyUp(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetKey__SystemString__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String var_0 = heap.GetHeapVariable<System.String>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out _) ? false : UnityEngine.Input.GetKey(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetKey__UnityEngineKeyCode__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            UnityEngine.KeyCode var_0 = heap.GetHeapVariable<UnityEngine.KeyCode>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out _) ? false : UnityEngine.Input.GetKey(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetMouseButtonDown__SystemInt32__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Int32 var_0 = heap.GetHeapVariable<System.Int32>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out var freezeState) ? false : UnityEngine.Input.GetMouseButtonDown(var_0) || (freezeState == FreezeInputState.JustUnfrozen && UnityEngine.Input.GetMouseButton(var_0));
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetMouseButtonUp__SystemInt32__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Int32 var_0 = heap.GetHeapVariable<System.Int32>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out var freezeState) ? freezeState == FreezeInputState.JustFrozen && (UnityEngine.Input.GetMouseButton(var_0) || UnityEngine.Input.GetMouseButtonUp(var_0)) : UnityEngine.Input.GetMouseButtonUp(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetMouseButton__SystemInt32__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Int32 var_0 = heap.GetHeapVariable<System.Int32>(parameterAddresses[0]);
            System.Boolean var_1 = FreezeInput(out _) ? false : UnityEngine.Input.GetMouseButton(var_0);
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __GetType__SystemType(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Object var_0 = heap.GetHeapVariable<System.Object>(parameterAddresses[0]);
            #if !UDON_DISABLE_SECURITY
            _filter.ApplyFilter(ref var_0);
            #endif
            
            System.Type var_1 = var_0.GetType();
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __ToString__SystemString(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Object var_0 = heap.GetHeapVariable<System.Object>(parameterAddresses[0]);
            #if !UDON_DISABLE_SECURITY
            _filter.ApplyFilter(ref var_0);
            #endif
            
            System.String var_1 = var_0.ToString();
            
            heap.SetHeapVariable(parameterAddresses[1], var_1);
        }
        
        private void __ctor____UnityEngineInput(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            UnityEngine.Input var_0 = new UnityEngine.Input();
            heap.SetHeapVariable(parameterAddresses[0], var_0);
        }
        
        private void __get_anyKeyDown__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Boolean var_0 = FreezeInput(out var freezeState) ? false : FreezeInput(out _) ? false : UnityEngine.Input.anyKeyDown || (freezeState == FreezeInputState.JustUnfrozen && FreezeInput(out _) ? false : UnityEngine.Input.anyKey);
            
            heap.SetHeapVariable(parameterAddresses[0], var_0);
        }
        
        private void __get_anyKey__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Boolean var_0 = FreezeInput(out _) ? false : UnityEngine.Input.anyKey;
            
            heap.SetHeapVariable(parameterAddresses[0], var_0);
        }
        
        private void __get_imeIsSelected__SystemBoolean(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.Boolean var_0 = UnityEngine.Input.imeIsSelected;
            
            heap.SetHeapVariable(parameterAddresses[0], var_0);
        }
        
        private void __get_inputString__SystemString(IUdonHeap heap, Span<uint> parameterAddresses)
        {
            System.String var_0 = FreezeInput(out _) ? "" : UnityEngine.Input.inputString;
            
            heap.SetHeapVariable(parameterAddresses[0], var_0);
        }
        
    }
}

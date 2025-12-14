/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.PenOptimizationUtility
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.Udon;

    public enum MaterialPropertyType
    {
        None,
        Float,
        Integer,
        Color,
        Vector,
        Texture,
        //Matrix
    }

    [AddComponentMenu("Pen Optimization Utility/MaterialProperty Overwriter")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MaterialPropertyOverwriter : UdonSharpBehaviour
    {
        public bool executeOnActive = true;
        public int materialIndex = 0;
        [SerializeField]
        private string[] _propertyName = new string[0];
        [SerializeField]
        private MaterialPropertyType[] _propertyType = new MaterialPropertyType[0];
        [SerializeField]
        private float[] _propertyFloat = new float[0];
        [SerializeField]
        private Color[] _propertyColor = new Color[0];
        [SerializeField]
        private Texture[] _propertyTexture = new Texture[0];

        private Matrix4x4[] _propertyMatrix4x4 = new Matrix4x4[0];

        private MeshRenderer _mesh;
        private MaterialPropertyBlock _mpb;
        private int[] _propertyID = new int[0];

        private void OnValidate()
        {
            var length = _propertyName.Length;
            if (length == 0)
            {
                _propertyType = new MaterialPropertyType[0];
                _propertyFloat = new float[0];
                _propertyColor = new Color[0];
                _propertyTexture = new Texture[0];
                _propertyMatrix4x4 = new Matrix4x4[0];
            }
            else
            {
                if (length != _propertyType.Length)
                {
                    var tmpType = new MaterialPropertyType[length];
                    System.Array.Copy(_propertyType, tmpType, Mathf.Min(_propertyType.Length, length));
                    _propertyType = tmpType;
                }
                if (length != _propertyFloat.Length)
                {
                    var tmpValue = new float[length];
                    System.Array.Copy(_propertyFloat, tmpValue, Mathf.Min(_propertyFloat.Length, length));
                    _propertyFloat = tmpValue;
                }
                if (length != _propertyColor.Length)
                {
                    var tmpValue = new Color[length];
                    System.Array.Copy(_propertyColor, tmpValue, Mathf.Min(_propertyColor.Length, length));
                    _propertyColor = tmpValue;
                }
                if (length != _propertyTexture.Length)
                {
                    var tmpTexture = new Texture[length];
                    System.Array.Copy(_propertyTexture, tmpTexture, Mathf.Min(_propertyTexture.Length, length));
                    _propertyTexture = tmpTexture;
                }
                if (length != _propertyMatrix4x4.Length)
                {
                    var tmpMatrix = new Matrix4x4[length];
                    System.Array.Copy(_propertyMatrix4x4, tmpMatrix, Mathf.Min(_propertyMatrix4x4.Length, length));
                    _propertyMatrix4x4 = tmpMatrix;
                }
            }
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _mesh = GetComponent<MeshRenderer>();
            _mpb = new MaterialPropertyBlock();

            _propertyID = new int[_propertyName.Length];
            for (int i = 0; i < _propertyID.Length; i++)
            {
                _propertyID[i] = VRCShader.PropertyToID(_propertyName[i]);
            }

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            if (executeOnActive) { _SetPropertyToMesh(); }
        }

        public void _ClearPropertyAll()
        {
            _propertyName = new string[0];
            _propertyID = new int[0];
            _propertyType = new MaterialPropertyType[0];
            _propertyFloat = new float[0];
            _propertyColor = new Color[0];
            _propertyTexture = new Texture[0];
            _propertyMatrix4x4 = new Matrix4x4[0];
        }

        public void _AddFloatProperty(string name, float value)
        {
            var last = CreateElementToPropertyArray();
            _propertyName[last] = name;
            _propertyID[last] = VRCShader.PropertyToID(name);
            _propertyType[last] = MaterialPropertyType.Float;
            _propertyFloat[last] = value;
        }
        public void _AddIntegerProperty(string name, int value)
        {
            var last = CreateElementToPropertyArray();
            _propertyName[last] = name;
            _propertyID[last] = VRCShader.PropertyToID(name);
            _propertyType[last] = MaterialPropertyType.Integer;
            _propertyFloat[last] = value;
        }
        public void _AddColorProperty(string name, Color value)
        {
            var last = CreateElementToPropertyArray();
            _propertyName[last] = name;
            _propertyID[last] = VRCShader.PropertyToID(name);
            _propertyType[last] = MaterialPropertyType.Color;
            _propertyColor[last] = value;
        }
        public void _AddVectorProperty(string name, Vector4 value)
        {
            var last = CreateElementToPropertyArray();
            _propertyName[last] = name;
            _propertyID[last] = VRCShader.PropertyToID(name);
            _propertyType[last] = MaterialPropertyType.Vector;
            _propertyColor[last] = value;
        }
        public void _AddTextureProperty(string name, Texture value)
        {
            var last = CreateElementToPropertyArray();
            _propertyName[last] = name;
            _propertyID[last] = VRCShader.PropertyToID(name);
            _propertyType[last] = MaterialPropertyType.Texture;
            _propertyTexture[last] = value;
        }
        /* public void _AddMatrix4x4Property(string name, Matrix4x4 value)
        {
            var last = CreateElementToPropertyArray();
            _propertyName[last] = name;
            _propertyID[last] = VRCShader.PropertyToID(name);
            _propertyType[last] = MaterialPropertyType.Matrix4x4;
            _propertyMatrix4x4[last] = value;
        } */

        public void _SetSharedMaterial(Material sharedMaterial)
        {
            _SetSharedMaterial(sharedMaterial, materialIndex);
        }
        public void _SetSharedMaterial(Material sharedMaterial, int slot)
        {
            Initialize();
            if (!_mesh) { return; }

            var meshMaterials = _mesh.sharedMaterials;
            meshMaterials[slot] = sharedMaterial;
            _mesh.sharedMaterials = meshMaterials;
        }

        public void _SetPropertyToMesh()
        {
            Initialize();
            if (!_mesh) { return; }

            _mesh.GetPropertyBlock(_mpb, materialIndex);
            for (int i = 0; i < _propertyName.Length; i++)
            {
                if (_propertyName[i] == "") { continue; }
                if (_propertyType[i] == MaterialPropertyType.None) { continue; }

                switch (_propertyType[i])
                {
                    case MaterialPropertyType.Float: _mpb.SetFloat(_propertyID[i], _propertyColor[i].r); break;
                    case MaterialPropertyType.Integer: _mpb.SetInteger(_propertyID[i], (int)_propertyColor[i].r); break;
                    case MaterialPropertyType.Color: _mpb.SetColor(_propertyID[i], _propertyColor[i]); break;
                    case MaterialPropertyType.Vector: _mpb.SetVector(_propertyID[i], (Vector4)_propertyColor[i]); break;
                    case MaterialPropertyType.Texture: _mpb.SetTexture(_propertyID[i], _propertyTexture[i]); break;
                    //case MaterialPropertyType.Matrix: _mpb.SetMatrix(propertyID[i], propertyValue[i]); break;
                }
            }
            _mesh.SetPropertyBlock(_mpb, materialIndex);
        }

        private int CreateElementToPropertyArray()
        {
            var result = _propertyName.Length;

            var tmp_propertyName = new string[_propertyName.Length + 1];
            System.Array.Copy(_propertyName, tmp_propertyName, _propertyName.Length);
            _propertyName = tmp_propertyName;

            var tmp_propertyID = new int[_propertyID.Length + 1];
            System.Array.Copy(_propertyID, tmp_propertyID, _propertyID.Length);
            _propertyID = tmp_propertyID;

            var tmp_propertyType = new MaterialPropertyType[_propertyType.Length + 1];
            System.Array.Copy(_propertyType, tmp_propertyType, _propertyType.Length);
            _propertyType = tmp_propertyType;

            var tmp_propertyFloat = new float[_propertyFloat.Length + 1];
            System.Array.Copy(_propertyFloat, tmp_propertyFloat, _propertyFloat.Length);
            _propertyFloat = tmp_propertyFloat;

            var tmp_propertyColor = new Color[_propertyColor.Length + 1];
            System.Array.Copy(_propertyColor, tmp_propertyColor, _propertyColor.Length);
            _propertyColor = tmp_propertyColor;

            var tmp_propertyTexture = new Texture[_propertyTexture.Length + 1];
            System.Array.Copy(_propertyTexture, tmp_propertyTexture, _propertyTexture.Length);
            _propertyTexture = tmp_propertyTexture;

            var tmp_propertyMatrix4x4 = new Matrix4x4[_propertyMatrix4x4.Length + 1];
            System.Array.Copy(_propertyMatrix4x4, tmp_propertyMatrix4x4, _propertyMatrix4x4.Length);
            _propertyMatrix4x4 = tmp_propertyMatrix4x4;

            return result;
        }
    }
}

/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/


namespace MimyLab.FukuroUdon
{
    using UdonSharp;

    public enum RotationLimitType
    {
        Rotate,
        EulerAngles
    }

    public enum AimLimitType
    {
        Angle,
        Polar
    }
    
    public abstract class LimitedConstraint : UdonSharpBehaviour
    {
    }
}
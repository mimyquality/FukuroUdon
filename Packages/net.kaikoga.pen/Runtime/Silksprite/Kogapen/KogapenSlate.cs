using System.Globalization;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Silksprite.Kogapen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KogapenSlate : UdonSharpBehaviour
    {
        [SerializeField] internal KogapenSync sync;

        [SerializeField] internal KogapenSwatch swatch;
        
        // NOTE: InputField and Slider values are in sRGB
        [SerializeField] internal InputField colorCodeInput;
        [SerializeField] internal Slider redSlider;
        [SerializeField] internal Slider greenSlider;
        [SerializeField] internal Slider blueSlider;
        [SerializeField] internal Image redBackground;
        [SerializeField] internal Image greenBackground;
        [SerializeField] internal Image blueBackground;

        [PublicAPI]
        public void Kogapen_ClearLocal() => sync._Kogapen_Clear(false);

        [PublicAPI]
        public void Kogapen_ClearGlobal() => sync._Kogapen_Clear(true);

        [PublicAPI]
        public void Kogapen_RespawnAll() => sync.Kogapen_RespawnAll();


        [PublicAPI]
        public void Kogapen_ReadSliderValues()
        {
            var color = new Color(redSlider.value, greenSlider.value, blueSlider.value);
            if (colorCodeInput)
            {
                Color32 color32 = color;
                colorCodeInput.text = $"{color32.r:X2}{color32.g:X2}{color32.b:X2}";
            }
            _SetColor(color);
        }

        [PublicAPI]
        public void Kogapen_ReadInputValue()
        {
            Color color;
            if (string.IsNullOrWhiteSpace(colorCodeInput.text))
            {
                color = Color.white;
            }
            else
            {
                var text = $"{colorCodeInput.text}00000";
                if (!int.TryParse(text.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var red)) return;
                if (!int.TryParse(text.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var green)) return;
                if (!int.TryParse(text.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var blue)) return;
                color = new Color32((byte)red, (byte)green, (byte)blue, 0xff);
            }
            if (redSlider) redSlider.value = color.r;
            if (greenSlider) greenSlider.value = color.g;
            if (blueSlider) blueSlider.value = color.b;
            _SetColor(color);
        }

        void _SetColor(Color color)
        {
            if (redBackground)
            {
                var redColor = color;
                redColor.r = 1f;
                redBackground.color = redColor;
            }
            if (greenBackground)
            {
                var greenColor = color;
                greenColor.g = 1f;
                greenBackground.color = greenColor;
            }
            if (blueBackground)
            {
                var blueColor = color;
                blueColor.b = 1f;
                blueBackground.color = blueColor;
            }

            if (swatch)
            {
                swatch.color = color.linear; // Swatch requires linear color, which conforms to LineRenderers
                swatch._RefreshColors();
            }
        }
    }
}

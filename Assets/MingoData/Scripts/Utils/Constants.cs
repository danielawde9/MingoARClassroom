using UnityEngine;

namespace MingoData.Scripts.Utils
{
    public struct Color32
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color32(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        
        public Color ToUnityColor()
        {
            return new Color(R / 255f, G / 255f, B / 255f, A / 255f);
        }
    }

    public static class Constants
    {
        public const float InitialDistanceScale = 1f / 10000000f;
        public const float InitialSizeScale = 1f / 10000f;
        public const float InitialSunSizeScale = 1f / 1000000f;
        //public const float initialSizeScale = 1f / 1000000f;
        public const float InitialTimeScale = 1f;
        public const float MinDistance = 1f / 100000000f;
        public const float MaxTime = 2000000f;
        public const float MaxSize = 1f / 10000f;
        public const float MinSize = 1f / 1000000f;
        public const float MaxDistance = 1f / 5000000f;
        public const string LangAR = "Arabic";
        public const string LangEn = "English";
        public const string SelectedPlanets = "SelectedPlanets";
        public const string SelectedLanguage = "SelectedLanguage";
        
        public static readonly Color ColorBlue = UtilsFns.CreateHexToColor("#00a8ff").ToUnityColor();
        public static readonly Color ColorViolet = UtilsFns.CreateHexToColor("#9c88ff").ToUnityColor();
        public static readonly Color ColorYellow = UtilsFns.CreateHexToColor("#fbc531").ToUnityColor();
        public static readonly Color ColorGreen = UtilsFns.CreateHexToColor("#4cd137").ToUnityColor();
        public static readonly Color ColorPaleBlue = UtilsFns.CreateHexToColor("#487eb0").ToUnityColor();
        public static readonly Color ColorRed = UtilsFns.CreateHexToColor("#e84118").ToUnityColor();
        public static readonly Color ColorWhite = UtilsFns.CreateHexToColor("#f5f6fa").ToUnityColor();
        public static readonly Color ColorGrey = UtilsFns.CreateHexToColor("#7f8fa6").ToUnityColor();
        public static readonly Color ColorDarkBlue = UtilsFns.CreateHexToColor("#273c75").ToUnityColor();
        public static readonly Color ColorDarkGrey = UtilsFns.CreateHexToColor("#353b48").ToUnityColor();
    }
}

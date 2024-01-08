using System.Diagnostics;
using System.Globalization;
using System.Windows.Media;

namespace ProjectLauncher.Functions
{
    public struct RTColor
    {
        public RTColor(byte r, byte g, byte b, byte a)
        {
            this.r = r / 255f;
            this.g = g / 255f;
            this.b = b / 255f;
            this.a = a / 255f;
        }

        public RTColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public RTColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            a = 1f;
        }

        public float r;
        public float g;
        public float b;
        public float a;

        public string Hex6 => HexR + HexG + HexB;
        public string Hex8 => HexR + HexG + HexB + HexA;

        public string HexR => r.ToString("X2");
        public string HexG=> g.ToString("X2");
        public string HexB => b.ToString("X2");
        public string HexA => a.ToString("X2");

        public int R => (int)(r * 255);
        public int G => (int)(g * 255);
        public int B => (int)(b * 255);
        public int A => (int)(a * 255);

        public Color FormsColor => Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B);

        public static RTColor White => new(1f, 1f, 1f, 1f);
        public static RTColor Black => new(0f, 0f, 0f, 1f);
        public static RTColor Transparent => new(0f, 0f, 0f, 0f);

        public static RTColor Gray => new(0.5f, 0.5f, 0.5f);

        public static RTColor Red => new(1f, 0f, 0f);
        public static RTColor Green => new(0f, 1f, 0f);
        public static RTColor Blue => new(0f, 0f, 1f);

        public override string ToString() => $"Color ({r}, {g}, {b}, {a})";

        public static RTColor FromHex(string hex)
        {
            hex = hex.Replace("#", "");
            if (hex.Length < 6 || hex.Length > 6 && hex.Length != 8)
            {
                Debug.WriteLine("Hex code is not the correct length.");
                return Red;
            }

            return new RTColor(
                byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
                hex.Length == 8 ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber) : byte.MaxValue);
        }
    }
}

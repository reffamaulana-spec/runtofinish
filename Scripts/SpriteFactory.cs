using UnityEngine;

public static class SpriteFactory
{
    public const int LayerW = 1920;
    public const int LayerH = 512;
    public const int SurfW = 2048;
    public const int SurfH = 192;
    public const int StripH = 48;

    static Sprite square;
    static Sprite circle;
    static Sprite triangle;

    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                t.SetPixel(0, 0, Color.white);
                t.filterMode = FilterMode.Point;
                t.Apply();
                square = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
            return square;
        }
    }

    public static Sprite Circle
    {
        get
        {
            if (circle == null)
            {
                const int n = 256;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
                var px = new Color32[n * n];
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        float dx = (x + 0.5f - n / 2f) / (n / 2f);
                        float dy = (y + 0.5f - n / 2f) / (n / 2f);
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        float a = Mathf.Clamp01((1f - d) * (n / 2f));
                        px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                    }
                }
                t.SetPixels32(px);
                t.filterMode = FilterMode.Bilinear;
                t.wrapMode = TextureWrapMode.Clamp;
                t.Apply();
                circle = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            }
            return circle;
        }
    }

    public static Sprite Triangle
    {
        get
        {
            if (triangle == null)
            {
                const int n = 256;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
                var px = new Color32[n * n];
                for (int y = 0; y < n; y++)
                {
                    float half = (1f - (y + 0.5f) / n) * (n / 2f);
                    for (int x = 0; x < n; x++)
                    {
                        float a = Mathf.Clamp01(half - Mathf.Abs(x + 0.5f - n / 2f));
                        px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                    }
                }
                t.SetPixels32(px);
                t.filterMode = FilterMode.Bilinear;
                t.wrapMode = TextureWrapMode.Clamp;
                t.Apply();
                triangle = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            }
            return triangle;
        }
    }

    static Sprite star;

    public static Sprite Star
    {
        get
        {
            if (star == null)
            {
                const int n = 256;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
                var px = new Color32[n * n];
                float outer = n / 2f - 3f;
                float inner = outer * 0.42f;
                float c36 = Mathf.Cos(36f * Mathf.Deg2Rad);
                float s36 = Mathf.Sin(36f * Mathf.Deg2Rad);
                float ix = inner * c36;
                float iy = inner * s36;
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        float dx = x + 0.5f - n / 2f;
                        float dy = y + 0.5f - n / 2f;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        float ang = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg;
                        if (ang < 0f) ang += 360f;
                        float a72 = ang % 72f;
                        float phi = (a72 > 36f ? 72f - a72 : a72) * Mathf.Deg2Rad;
                        float den = Mathf.Cos(phi) - Mathf.Sin(phi) * (ix - outer) / iy;
                        float rb = den > 0.0001f ? outer / den : inner;
                        float al = Mathf.Clamp01(rb - dist + 0.5f);
                        px[y * n + x] = new Color32(255, 255, 255, (byte)(al * 255f));
                    }
                }
                t.SetPixels32(px);
                t.filterMode = FilterMode.Bilinear;
                t.wrapMode = TextureWrapMode.Clamp;
                t.Apply();
                star = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            }
            return star;
        }
    }

    public static Sprite Gradient(Color top, Color bottom)
    {
        const int n = 256;
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
        {
            Color c = Color.Lerp(bottom, top, y / (float)(n - 1));
            Color32 c32 = c;
            for (int x = 0; x < n; x++) px[y * n + x] = c32;
        }
        t.SetPixels32(px);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
    }

    static Color Mul(Color c, float f)
    {
        return new Color(Mathf.Clamp01(c.r * f), Mathf.Clamp01(c.g * f), Mathf.Clamp01(c.b * f), 1f);
    }

    static float Hash(int n)
    {
        unchecked
        {
            n = (n << 13) ^ n;
            int m = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
            return 1f - m / 1073741824f;
        }
    }

    static float H01(int n)
    {
        return Mathf.Abs(Hash(n));
    }

    public static Sprite Layer(int kind, Color c, int seed, Color accent, float bright)
    {
        const int w = LayerW;
        const int h = LayerH;
        var px = new Color32[w * h];
        switch (kind)
        {
            case 0: Mountains(px, w, h, c, accent, seed); break;
            case 1: Hills(px, w, h, c, seed); break;
            case 2: Trees(px, w, h, c, seed); break;
            case 3: Buildings(px, w, h, c, accent, bright, seed); break;
            case 4: Stands(px, w, h, c, bright, seed); break;
            default: Boards(px, w, h, bright, seed); break;
        }
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.SetPixels32(px);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Repeat;
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1f);
    }

    static void Mountains(Color32[] px, int w, int h, Color c, Color acc, int seed)
    {
        var hs = new float[w];
        for (int x = 0; x < w; x++)
        {
            float a = 2f * Mathf.PI * x / w;
            float v = 0.10f + 0.42f * (1f - Mathf.Abs(Mathf.Sin(a * 3f + seed))) + 0.22f * (1f - Mathf.Abs(Mathf.Sin(a * 7f + seed * 2f))) + 0.03f * Mathf.Sin(a * 41f + seed);
            hs[x] = Mathf.Clamp(v, 0.05f, 0.98f);
        }
        for (int x = 0; x < w; x++)
        {
            int hh = Mathf.RoundToInt(hs[x] * h);
            float slope = hs[(x + 1) % w] - hs[(x + w - 1) % w];
            float light = 1f + Mathf.Clamp(-slope * 60f, -0.18f, 0.18f);
            float snow = 0f;
            for (int y = 0; y < hh; y++)
            {
                float t = y / (float)hh;
                Color col = Mul(c, light * (0.82f + 0.2f * t));
                if (snow > 0f && y > hh - snow) col = Mul(acc, light);
                px[y * w + x] = col;
            }
        }
    }

    static void Hills(Color32[] px, int w, int h, Color c, int seed)
    {
        for (int x = 0; x < w; x++)
        {
            float a = 2f * Mathf.PI * x / w;
            float v = 0.5f + 0.2f * Mathf.Sin(a * 3f + seed) + 0.12f * Mathf.Sin(a * 7f + seed * 2f) + 0.05f * Mathf.Sin(a * 13f + seed * 3f);
            int hh = Mathf.RoundToInt(Mathf.Clamp(v, 0.05f, 0.98f) * h);
            for (int y = 0; y < hh; y++)
            {
                float t = y / (float)hh;
                px[y * w + x] = Mul(c, 0.78f + 0.3f * t);
            }
        }
    }

    static void Trees(Color32[] px, int w, int h, Color c, int seed)
    {
        const int cell = 160;
        Color trunk = new Color(0.28f, 0.17f, 0.08f);
        for (int x = 0; x < w; x++)
        {
            int i = x / cell;
            float tx = (x % cell) / (float)cell;
            float tri = 1f - Mathf.Abs(tx * 2f - 1f);
            float hv = 0.55f + 0.45f * H01(i + seed * 17);
            float bump = 1f + 0.10f * Mathf.Sin(tri * 22f + i);
            float hgt = Mathf.Clamp(0.10f + 0.88f * hv * Mathf.Pow(tri, 0.85f) * bump, 0.1f, 1f);
            int hh = Mathf.RoundToInt(hgt * h);
            for (int y = 0; y < hh; y++)
            {
                float t = y / (float)hh;
                Color col;
                if (y < 0.1f * h && Mathf.Abs(tx - 0.5f) < 0.05f) col = trunk;
                else
                {
                    col = Mul(c, 0.7f + 0.45f * t);
                    col = Mul(col, tx > 0.5f ? 1.12f : 0.92f);
                }
                px[y * w + x] = col;
            }
        }
    }

    static void Buildings(Color32[] px, int w, int h, Color c, Color acc, float bright, int seed)
    {
        const int cell = 192;
        for (int x = 0; x < w; x++)
        {
            int i = x / cell;
            int lx = x % cell;
            if (lx < 5 || lx >= cell - 5) continue;
            float hv = 0.32f + 0.58f * H01(i + seed * 31);
            int hh = Mathf.RoundToInt(hv * h);
            Color baseC = Mul(c, 0.92f + 0.16f * H01(i * 7 + seed));
            bool antenna = H01(i + 99) > 0.7f && Mathf.Abs(lx - cell / 2) < 2;
            for (int y = 0; y < hh; y++)
            {
                Color col = baseC;
                if (y > hh - 8) col = Mul(baseC, 1.15f);
                if (lx >= 14 && lx < cell - 16 && y >= 20 && y < hh - 26)
                {
                    int wx = (lx - 14) % 26;
                    int wy = (y - 20) % 34;
                    if (wx < 14 && wy < 22)
                    {
                        int wi = (lx - 14) / 26;
                        int wj = (y - 20) / 34;
                        float r = H01(i * 131 + wi * 17 + wj * 7 + seed);
                        col = r > Mathf.Lerp(0.5f, 0.85f, bright) ? acc : Mul(baseC, 0.78f);
                    }
                }
                px[y * w + x] = col;
            }
            if (antenna)
            {
                for (int y = hh; y < hh + 60 && y < h; y++) px[y * w + x] = Mul(baseC, 0.8f);
            }
        }
    }

    static void Stands(Color32[] px, int w, int h, Color c, float bright, int seed)
    {
        int top = Mathf.RoundToInt(0.62f * h);
        int canopy = 30;
        Color seat = Mul(c, 0.55f);
        for (int x = 0; x < w; x++)
        {
            int ci = x / 18;
            int cx = x % 18;
            for (int y = 0; y < top + canopy; y++)
            {
                Color col;
                if (y < 28)
                {
                    col = Mul(c, 0.7f);
                }
                else if (y >= top)
                {
                    col = Mul(c, 1.1f);
                    if (y >= top + 10 && y < top + 16) col = Mul(c, 1.6f);
                }
                else
                {
                    int row = (y - 28) / 34;
                    int ty = (y - 28) % 34;
                    float rowShade = 0.8f + 0.2f * (row / 9f);
                    col = seat;
                    if (ty < 20 && cx >= 3 && cx < 15)
                    {
                        float hue = Mathf.Abs(Hash(ci * 13 + row * 7 + seed)) % 1f;
                        col = Color.HSVToRGB(hue, 0.55f, Mathf.Clamp01(bright * 0.9f * rowShade));
                    }
                    else if (ty >= 20 && ty < 30)
                    {
                        int dx = cx - 9;
                        int dy = ty - 25;
                        if (dx * dx + dy * dy <= 20)
                        {
                            float sk = 0.45f + 0.5f * H01(ci * 5 + row * 3 + seed);
                            col = Mul(new Color(0.95f * sk + 0.05f, 0.72f * sk, 0.55f * sk), Mathf.Clamp01(bright * 1.05f));
                        }
                    }
                }
                px[y * w + x] = col;
            }
        }
    }

    static void Boards(Color32[] px, int w, int h, float bright, int seed)
    {
        const int cell = 160;
        int bh = Mathf.RoundToInt(0.22f * h);
        for (int x = 0; x < w; x++)
        {
            int i = x / cell;
            int lx = x % cell;
            float hue = Mathf.Abs(Hash(i * 5 + seed)) % 1f;
            Color panel = Color.HSVToRGB(hue, 0.7f, 0.85f * bright);
            for (int y = 0; y < bh; y++)
            {
                Color col = panel;
                if (lx < 4) col = Mul(panel, 0.4f);
                else if (y > bh - 10) col = Mul(panel, 0.55f);
                else if (y < 8) col = Mul(panel, 0.7f);
                else if (y >= bh / 2 - 8 && y < bh / 2 + 8 && lx >= 20 && lx < 140 && (lx / 10) % 2 == 0) col = Mul(Color.white, bright);
                px[y * w + x] = col;
            }
        }
    }

    public static Sprite Surface(int kind, Color c, int seed)
    {
        const int w = SurfW;
        const int h = SurfH;
        var px = new Color32[w * h];
        var rut = new float[h];
        for (int y = 0; y < h; y++)
        {
            float fy = y / (float)h;
            rut[y] = Mathf.Exp(-Mathf.Pow((fy - 0.3f) / 0.05f, 2f)) + Mathf.Exp(-Mathf.Pow((fy - 0.68f) / 0.05f, 2f));
        }
        for (int y = 0; y < h; y++)
        {
            float fy = y / (float)h;
            for (int x = 0; x < w; x++)
            {
                float n = H01((x / 2) * 977 + (y / 2) * 131 + seed);
                Color col;
                if (kind == 0)
                {
                    col = Mul(c, 0.84f + 0.3f * n);
                    col = Mul(col, 1f - 0.2f * rut[y]);
                    if (fy > 0.93f) col = Mul(col, 0.8f);
                }
                else if (kind == 1)
                {
                    col = Mul(c, 0.88f + 0.22f * n);
                    col = Mul(col, 1f - 0.12f * rut[y]);
                }
                else
                {
                    col = Mul(c, 0.92f + 0.16f * n);
                }
                px[y * w + x] = col;
            }
        }
        if (kind == 0)
        {
            for (int i = 0; i < 140; i++)
            {
                int cx = Mathf.FloorToInt(H01(i * 3 + seed) * w);
                int cy = Mathf.FloorToInt(H01(i * 3 + 1 + seed) * h);
                int r = 2 + Mathf.FloorToInt(3f * H01(i * 3 + 2 + seed));
                Color pc = Mul(c, 1.25f);
                Color sc = Mul(c, 0.6f);
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (dx * dx + dy * dy > r * r) continue;
                        int yy = cy + dy;
                        if (yy < 0 || yy >= h) continue;
                        int xx = ((cx + dx) % w + w) % w;
                        px[yy * w + xx] = dy < 0 ? sc : pc;
                    }
                }
            }
        }
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.SetPixels32(px);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Repeat;
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1f);
    }

    public static Sprite Strip(int kind, Color c, int seed)
    {
        const int w = SurfW;
        const int h = StripH;
        var px = new Color32[w * h];
        for (int x = 0; x < w; x++)
        {
            float tuft = 6f + 10f * H01((x / 6) * 31 + seed);
            for (int y = 0; y < h; y++)
            {
                float n = H01(x * 53 + y * 17 + seed);
                Color col;
                if (kind == 0)
                {
                    if (y > h * 0.62f + tuft) continue;
                    col = Mul(c, 0.8f + 0.35f * (y / (float)h) + 0.12f * n);
                }
                else if (kind == 1)
                {
                    col = Mul(c, 0.94f + 0.1f * n);
                    if (x % 128 < 3) col = Mul(c, 0.78f);
                    if (y < 7) col = Mul(c, 0.62f);
                    else if (y > h - 5) col = Mul(c, 1.12f);
                }
                else
                {
                    bool stripe = (x / 128) % 2 == 0;
                    col = Mul(c, (stripe ? 1.08f : 0.9f) + 0.06f * n);
                    if (y > h - 4) col = Mul(new Color(0.95f, 0.95f, 0.95f), 0.95f);
                }
                px[y * w + x] = col;
            }
        }
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.SetPixels32(px);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Repeat;
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1f);
    }
}

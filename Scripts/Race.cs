using System.Collections.Generic;
using UnityEngine;

public class Rig
{
    public Transform root, body, legF, legB, armF, armB, tail, scarf;
    public float phase;
}

public class Rival
{
    public string name;
    public Color color;
    public float meters, speed, offset, phase, laneY, laneScale, y, vy, stumbleT;
    public float finishTime = -1f;
    public bool grounded = true, decided, willMiss;
    public int nextHurdle;
    public Rig rig;
}

public class Look
{
    public Color skin, hair, top, bottom, accent;
    public int hairStyle;
    public int topStyle;
    public float scale = 1f;
    public float bodyW = 0.5f;
    public bool glasses, goggles, headband, beard, longLegs, scarf, wristbands, randomTop;
}

public class ShoeLook
{
    public Color upper, sole, accent;
    public float thick;
    public int style;
}

public class Race : MonoBehaviour
{
    public const float RunnerX = -5.5f;
    public const float FootY = -3.8f;
    public const float SpawnX = 11f;
    public const float Gravity = -30f;
    public const float BaseSpeed = 1000f / 235f;
    public const float UnitsPerSpeed = 9f / BaseSpeed;
    const float TileW = 20f;

    struct Theme
    {
        public Color skyTop, skyBot, sun, c1, c2, c3, accent, strip, ground;
        public int k1, k2, k3, stripKind, groundKind, lineKind, propKind;
        public float bright;
        public bool night;
        public int[] obst;
    }

    class Obstacle
    {
        public Transform t;
        public float x;
        public float hw;
        public float hh;
        public bool passed;
        public int lane = -1;
        public float yOff;
    }

    class ThemeSprites
    {
        public Sprite sky;
        public Sprite[] layers;
        public Sprite strip;
        public Sprite surf;
    }

    static readonly Theme[] Themes =
    {
        // 0 – Forest Track
        new Theme { skyTop = new Color(0.28f, 0.60f, 0.95f), skyBot = new Color(0.82f, 0.94f, 1f), sun = new Color(1f, 0.93f, 0.55f), c1 = new Color(0.52f, 0.67f, 0.64f), c2 = new Color(0.30f, 0.54f, 0.36f), c3 = new Color(0.10f, 0.34f, 0.18f), accent = Color.white, strip = new Color(0.28f, 0.62f, 0.26f), ground = new Color(0.55f, 0.38f, 0.23f), k1 = 0, k2 = 1, k3 = 2, stripKind = 0, groundKind = 0, lineKind = 0, propKind = 0, bright = 0.9f, night = false, obst = new[] { 0, 1 } },
        // 1 – City Sprint
        new Theme { skyTop = new Color(0.38f, 0.56f, 0.88f), skyBot = new Color(0.98f, 0.86f, 0.74f), sun = new Color(1f, 0.85f, 0.5f), c1 = new Color(0.62f, 0.67f, 0.78f), c2 = new Color(0.42f, 0.47f, 0.58f), c3 = new Color(0.24f, 0.27f, 0.35f), accent = new Color(1f, 0.92f, 0.65f), strip = new Color(0.64f, 0.64f, 0.68f), ground = new Color(0.25f, 0.26f, 0.29f), k1 = 3, k2 = 3, k3 = 3, stripKind = 1, groundKind = 1, lineKind = 1, propKind = 1, bright = 0.9f, night = false, obst = new[] { 2, 3 } },
        // 2 – Night Stadium
        new Theme { skyTop = new Color(0.02f, 0.03f, 0.12f), skyBot = new Color(0.10f, 0.13f, 0.32f), sun = new Color(0.95f, 0.95f, 0.85f), c1 = new Color(0.10f, 0.13f, 0.28f), c2 = new Color(0.18f, 0.16f, 0.30f), c3 = new Color(0.10f, 0.10f, 0.20f), accent = new Color(1f, 0.85f, 0.4f), strip = new Color(0.08f, 0.30f, 0.14f), ground = new Color(0.62f, 0.25f, 0.22f), k1 = 3, k2 = 4, k3 = 5, stripKind = 2, groundKind = 2, lineKind = 2, propKind = 2, bright = 0.5f, night = true, obst = new[] { 4 } },
        // 3 – Olympic Stadium
        new Theme { skyTop = new Color(0.33f, 0.64f, 0.96f), skyBot = new Color(0.90f, 0.96f, 1f), sun = new Color(1f, 0.97f, 0.8f), c1 = new Color(0.80f, 0.84f, 0.90f), c2 = new Color(0.62f, 0.68f, 0.80f), c3 = new Color(0.25f, 0.30f, 0.45f), accent = Color.white, strip = new Color(0.30f, 0.68f, 0.28f), ground = new Color(0.16f, 0.40f, 0.74f), k1 = 4, k2 = 4, k3 = 5, stripKind = 2, groundKind = 2, lineKind = 2, propKind = 3, bright = 0.95f, night = false, obst = new[] { 4, 5 } },
        // 4 – Desert Inferno (Legend) — langit oranye-merah, pasir bergelombang, matahari menyengat
        new Theme { skyTop = new Color(0.72f, 0.28f, 0.06f), skyBot = new Color(0.98f, 0.72f, 0.35f), sun = new Color(1f, 0.88f, 0.30f), c1 = new Color(0.78f, 0.58f, 0.32f), c2 = new Color(0.65f, 0.44f, 0.20f), c3 = new Color(0.50f, 0.32f, 0.12f), accent = new Color(1f, 0.75f, 0.20f), strip = new Color(0.85f, 0.60f, 0.28f), ground = new Color(0.72f, 0.52f, 0.22f), k1 = 1, k2 = 0, k3 = 0, stripKind = 0, groundKind = 0, lineKind = 0, propKind = 0, bright = 1.0f, night = false, obst = new[] { 0, 1, 3 } },
        // 5 – Neon City Dash (Super) — malam neon cyan-magenta, kota futuristik
        new Theme { skyTop = new Color(0.02f, 0.02f, 0.10f), skyBot = new Color(0.05f, 0.08f, 0.22f), sun = new Color(0.30f, 1.00f, 0.85f), c1 = new Color(0.08f, 0.22f, 0.40f), c2 = new Color(0.06f, 0.14f, 0.30f), c3 = new Color(0.04f, 0.08f, 0.20f), accent = new Color(0.15f, 0.90f, 1f), strip = new Color(0.80f, 0.15f, 0.90f), ground = new Color(0.08f, 0.10f, 0.20f), k1 = 3, k2 = 4, k3 = 5, stripKind = 1, groundKind = 1, lineKind = 2, propKind = 2, bright = 0.6f, night = true, obst = new[] { 2, 4, 5 } },
        // 6 – Cosmos Circuit (Elite) — orbit luar angkasa, bintang dan nebula, gravitasi rendah
        new Theme { skyTop = new Color(0.00f, 0.00f, 0.04f), skyBot = new Color(0.06f, 0.02f, 0.18f), sun = new Color(1f, 0.82f, 0.50f), c1 = new Color(0.18f, 0.10f, 0.32f), c2 = new Color(0.12f, 0.06f, 0.24f), c3 = new Color(0.08f, 0.04f, 0.16f), accent = new Color(0.60f, 0.30f, 1f), strip = new Color(0.40f, 0.80f, 1f), ground = new Color(0.10f, 0.08f, 0.22f), k1 = 4, k2 = 5, k3 = 5, stripKind = 2, groundKind = 2, lineKind = 2, propKind = 3, bright = 0.45f, night = true, obst = new[] { 3, 4, 5 } },
    };

    static readonly float[] GapMin = { 18f, 14f, 11f, 9f };
    static readonly float[] GapMax = { 28f, 22f, 17f, 13f };
    static readonly float[] ComboChance = { 0f, 0.2f, 0.35f, 0.5f };
    static readonly int[] MaxGroup = { 1, 2, 3, 3 };

    Transform root;
    SpriteRenderer skyR, sunR, glowR;
    readonly SpriteRenderer[] layerA = new SpriteRenderer[3];
    readonly SpriteRenderer[] layerB = new SpriteRenderer[3];
    SpriteRenderer stripA, stripB, surfA, surfB, lineTop, lineMid, lineBot;
    readonly float[] factors = { 0.05f, 0.15f, 0.4f };
    readonly float[] layerH = { 5f, 3.6f, 2.6f };
    readonly Transform[] dashes = new Transform[12];
    readonly Transform[] stars = new Transform[60];
    readonly Transform[] clouds = new Transform[5];
    readonly float[] cloudX = new float[5];
    readonly float[] cloudY = new float[5];
    readonly Transform[] props = new Transform[6];
    readonly SpriteRenderer[] dashRs = new SpriteRenderer[12];
    readonly Dictionary<int, ThemeSprites> cache = new Dictionary<int, ThemeSprites>();

    Transform runnerRoot, bodyT, legF, legB, armF, armB, tail, scarfT;
    SpriteRenderer[] runnerSR;
    Color[] runnerBase;
    bool blinked;

    Transform finishT;
    readonly List<Obstacle> obstacles = new List<Obstacle>();

    public bool racing;
    public bool finished;
    public float countdown;
    public float realTime;
    public float virtTime;
    public float meters;
    public float speedMS;
    public float stamina = 100f;
    public float stumbleT;
    public int target = 1000;
    public int hits;
    public int mode;
    public int taps;
    public int hurdlesPassed;
    public float tapRate;
    public float Effort;
    public float MaxSpeed { get { return maxSpeed; } }
    public float TapRef { get { return TapRefs[(int)diff]; } }
    static readonly float[] TapRefs = { 6f, 7f, 8f, 9f };
    static readonly float[] DiffGap = { 1.25f, 1f, 0.85f, 0.72f };
    readonly List<float> tapTimes = new List<float>();
    float staminaStat;
    int bandOffset = 300;
    float hurdleFirstS;
    public readonly List<Rival> rivals = new List<Rival>();
    static readonly float[] DiffRivalTap = { -0.07f, 0f, 0.04f, 0.08f };
    static readonly float[] DiffRivalH = { -0.06f, 0f, 0.03f, 0.06f };
    static readonly float[] MissP = { 0.22f, 0.14f, 0.08f, 0.04f };
    static readonly float[] LaneY = { 0.42f, -0.5f, -0.9f };
    static readonly float[] LaneScale = { 0.95f, 1.03f, 1.06f };
    static readonly int[] LaneBand = { 150, 450, 600 };
    static readonly Color[] TagColors = { new Color(1f, 0.35f, 0.35f), new Color(0.35f, 0.85f, 1f), new Color(1f, 0.9f, 0.3f) };
    public System.Action<int> OnEvent;
    public bool Grounded { get { return grounded; } }
    public Difficulty diff = Difficulty.Normal;

    float K = 1f;
    float scroll;
    float maxSpeed = BaseSpeed;
    float jumpV = 11.5f;
    float regen = 24f;
    float jumpCost = 20f;
    float stumbleDur = 1.2f;
    float stumbleFactor = 0.55f;
    float y;
    float vy;
    bool grounded = true;
    int jumpsUsed;
    float invT;
    float spawnCountdown;
    bool finishOn;
    float finishX;
    float phase;
    int themeIdx = -1;

    public float Progress
    {
        get { return target > 0 ? Mathf.Clamp01(meters / target) : 0f; }
    }

    public static Color[] TrackColors(int idx)
    {
        var t = Themes[Mathf.Clamp(idx, 0, Themes.Length - 1)];
        return new Color[] { t.skyTop, t.skyBot, t.strip, t.ground };
    }

    public static Look GetLook(string id)
    {
        var l = new Look();
        switch (id)
        {
            case "runner_maya":
                l.skin = new Color(0.82f, 0.60f, 0.44f);
                l.hair = new Color(0.35f, 0.20f, 0.10f);
                l.top = new Color(0.95f, 0.40f, 0.60f);
                l.bottom = new Color(0.20f, 0.15f, 0.35f);
                l.accent = new Color(1f, 0.85f, 0.2f);
                l.hairStyle = 1;
                l.topStyle = 1;
                l.scale = 0.95f;
                l.bodyW = 0.44f;
                l.wristbands = true;
                l.randomTop = true;
                break;
            case "runner_kenji":
                l.skin = new Color(0.96f, 0.82f, 0.68f);
                l.hair = new Color(0.08f, 0.08f, 0.10f);
                l.top = new Color(0.20f, 0.35f, 0.55f);
                l.bottom = new Color(0.10f, 0.10f, 0.14f);
                l.accent = new Color(0.2f, 0.2f, 0.25f);
                l.hairStyle = 2;
                l.topStyle = 2;
                l.longLegs = true;
                l.glasses = true;
                l.scale = 1.08f;
                l.bodyW = 0.46f;
                l.randomTop = true;
                break;
            case "runner_bima":
                l.skin = new Color(0.56f, 0.37f, 0.25f);
                l.hair = new Color(0.08f, 0.05f, 0.04f);
                l.top = new Color(0.20f, 0.60f, 0.30f);
                l.bottom = new Color(0.15f, 0.15f, 0.20f);
                l.accent = Color.white;
                l.hairStyle = 3;
                l.beard = true;
                l.topStyle = 0;
                l.scale = 1f;
                l.bodyW = 0.62f;
                l.wristbands = true;
                l.randomTop = true;
                break;
            case "runner_ace":
                l.skin = new Color(0.72f, 0.52f, 0.38f);
                l.hair = new Color(0.95f, 0.78f, 0.20f);
                l.top = new Color(0.08f, 0.08f, 0.10f);
                l.bottom = new Color(0.08f, 0.08f, 0.10f);
                l.accent = new Color(1f, 0.80f, 0.20f);
                l.hairStyle = 4;
                l.topStyle = 3;
                l.goggles = true;
                l.scarf = true;
                l.scale = 1.1f;
                l.bodyW = 0.5f;
                break;

            // Jonathan Sawe — kulit gelap Kenya, kepala dicukur, jersey merah-hijau Kenya, langsing & tungkai panjang
            case "runner_sawe":
                l.skin = new Color(0.28f, 0.18f, 0.12f);
                l.hair = new Color(0.08f, 0.06f, 0.05f);
                l.top = new Color(0.85f, 0.10f, 0.10f);
                l.bottom = new Color(0.06f, 0.22f, 0.06f);
                l.accent = new Color(1f, 0.85f, 0.10f);
                l.hairStyle = 3;              // dicukur pendek / botak
                l.topStyle = 2;              // singlet lengan
                l.scale = 1.05f;
                l.bodyW = 0.42f;             // tubuh ramping
                l.longLegs = true;
                l.headband = true;
                l.randomTop = true;
                break;

            // Jakob Ingebrigtsen — kulit terang Norwegia, rambut pirang, jersey biru-merah Norwegia, tinggi atletis
            case "runner_jakob":
                l.skin = new Color(0.96f, 0.86f, 0.74f);
                l.hair = new Color(0.92f, 0.80f, 0.40f);  // pirang keemasan
                l.top = new Color(0.08f, 0.22f, 0.72f);   // biru Norwegia
                l.bottom = new Color(0.75f, 0.10f, 0.10f);// merah Norwegia
                l.accent = new Color(0.90f, 0.90f, 0.95f);// putih
                l.hairStyle = 2;              // rambut bergelombang
                l.topStyle = 3;              // jersey ketat garis sisi
                l.scale = 1.12f;             // tinggi (1.80m)
                l.bodyW = 0.46f;
                l.longLegs = true;
                l.scarf = false;
                l.randomTop = false;
                break;

            // Reffa Fabyan — kulit sawo matang Indonesia, rambut hitam pendek rapi, jersey hitam-emas elite, build atletis sempurna
            case "runner_reffa":
                l.skin = new Color(0.68f, 0.48f, 0.32f);
                l.hair = new Color(0.06f, 0.05f, 0.04f);  // hitam legam
                l.top = new Color(0.06f, 0.06f, 0.08f);   // hitam
                l.bottom = new Color(0.06f, 0.06f, 0.08f);// hitam
                l.accent = new Color(1f, 0.78f, 0.12f);   // garis emas
                l.hairStyle = 0;              // pendek rapi
                l.topStyle = 3;              // jersey elite garis emas
                l.scale = 1.06f;
                l.bodyW = 0.50f;
                l.longLegs = true;
                l.goggles = true;            // kacamata sport elite
                l.headband = false;
                l.randomTop = false;
                break;
            default:
                l.skin = new Color(0.93f, 0.74f, 0.58f);
                l.hair = new Color(0.08f, 0.06f, 0.05f);
                l.top = new Color(0.85f, 0.20f, 0.20f);
                l.bottom = new Color(0.12f, 0.12f, 0.20f);
                l.accent = new Color(0.9f, 0.15f, 0.15f);
                l.hairStyle = 0;
                l.topStyle = 0;
                l.headband = true;
                l.randomTop = true;
                break;
        }
        return l;
    }

    public static ShoeLook GetShoe(string id)
    {
        var s = new ShoeLook();
        switch (id)
        {
            case "shoe_tempo":
                s.upper = new Color(0.20f, 0.50f, 0.95f);
                s.sole = Color.white;
                s.accent = Color.white;
                s.thick = 0.06f;
                s.style = 1;
                break;
            case "shoe_air":
                s.upper = new Color(1f, 0.55f, 0.10f);
                s.sole = Color.white;
                s.accent = new Color(0.4f, 0.9f, 1f);
                s.thick = 0.12f;
                s.style = 2;
                break;
            case "shoe_endure":
                s.upper = new Color(0.20f, 0.75f, 0.40f);
                s.sole = new Color(0.25f, 0.27f, 0.30f);
                s.accent = new Color(1f, 0.9f, 0.3f);
                s.thick = 0.14f;
                s.style = 3;
                break;
            case "shoe_blaze":
                s.upper = new Color(0.95f, 0.30f, 0.10f);
                s.sole = new Color(0.20f, 0.20f, 0.22f);
                s.accent = new Color(1f, 0.60f, 0.10f);
                s.thick = 0.10f;
                s.style = 1;
                break;
            case "shoe_carbon":
                s.upper = new Color(0.07f, 0.07f, 0.09f);
                s.sole = new Color(0.10f, 0.10f, 0.12f);
                s.accent = new Color(1f, 0.80f, 0.20f);
                s.thick = 0.08f;
                s.style = 4;
                break;
            case "shoe_phantom":
                s.upper = new Color(0.10f, 0.10f, 0.14f);
                s.sole = new Color(0.72f, 0.72f, 0.80f);
                s.accent = new Color(0.82f, 0.90f, 1f);
                s.thick = 0.07f;
                s.style = 4;
                break;
            case "shoe_storm":
                s.upper = new Color(0.10f, 0.35f, 0.90f);
                s.sole = new Color(0.08f, 0.08f, 0.12f);
                s.accent = new Color(0.35f, 0.85f, 1f);
                s.thick = 0.09f;
                s.style = 4;
                break;
            case "shoe_titanium":
                s.upper = new Color(0.55f, 0.58f, 0.65f);
                s.sole = new Color(0.30f, 0.32f, 0.36f);
                s.accent = new Color(1f, 0.78f, 0.20f);
                s.thick = 0.10f;
                s.style = 3;
                break;
            case "shoe_apex":
                s.upper = new Color(0.52f, 0.05f, 0.08f);
                s.sole = new Color(0.06f, 0.06f, 0.08f);
                s.accent = new Color(1f, 0.22f, 0.22f);
                s.thick = 0.07f;
                s.style = 4;
                break;
            case "shoe_cosmic":
                s.upper = new Color(0.38f, 0.12f, 0.75f);
                s.sole = new Color(0.06f, 0.04f, 0.14f);
                s.accent = new Color(0.70f, 0.50f, 1f);
                s.thick = 0.07f;
                s.style = 4;
                break;
            default:
                s.upper = new Color(0.92f, 0.92f, 0.95f);
                s.sole = new Color(0.55f, 0.58f, 0.65f);
                s.accent = new Color(0.7f, 0.72f, 0.8f);
                s.thick = 0.05f;
                s.style = 0;
                break;
        }
        return s;
    }

    static Color Shade(Color c, float f)
    {
        return new Color(c.r * f, c.g * f, c.b * f, 1f);
    }

    SpriteRenderer Add(Transform parent, Sprite sp, Color c, int order, Vector2 pos, Vector2 scale)
    {
        return AddR(parent, sp, c, order, pos, scale, 0f);
    }

    SpriteRenderer AddR(Transform parent, Sprite sp, Color c, int order, Vector2 pos, Vector2 scale, float rot)
    {
        var go = new GameObject("s");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rot);
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.color = c;
        sr.sortingOrder = order;
        return sr;
    }

    public void Build(Camera c)
    {
        root = new GameObject("World").transform;
        root.SetParent(transform, false);

        skyR = Add(root, null, Color.white, -100, Vector2.zero, new Vector2(20f, 10.6f));
        glowR = Add(root, SpriteFactory.Circle, Color.white, -91, new Vector2(5.5f, 3.1f), new Vector2(4.2f, 4.2f));
        sunR = Add(root, SpriteFactory.Circle, Color.white, -90, new Vector2(5.5f, 3.1f), new Vector2(1.7f, 1.7f));

        for (int i = 0; i < stars.Length; i++)
        {
            float sz = Random.Range(0.04f, 0.11f);
            var sr = Add(root, SpriteFactory.Circle, new Color(1f, 1f, 1f, Random.Range(0.5f, 1f)), -95, new Vector2(Random.Range(-8.5f, 8.5f), Random.Range(-0.5f, 4.8f)), new Vector2(sz, sz));
            stars[i] = sr.transform;
        }

        for (int i = 0; i < clouds.Length; i++)
        {
            var ct = new GameObject("cloud").transform;
            ct.SetParent(root, false);
            float sc = Random.Range(0.8f, 1.4f);
            Add(ct, SpriteFactory.Circle, new Color(1f, 1f, 1f, 0.85f), -92, new Vector2(0f, 0f), new Vector2(1.4f * sc, 0.8f * sc));
            Add(ct, SpriteFactory.Circle, new Color(1f, 1f, 1f, 0.85f), -92, new Vector2(0.8f * sc, 0.12f * sc), new Vector2(1.1f * sc, 0.7f * sc));
            Add(ct, SpriteFactory.Circle, new Color(1f, 1f, 1f, 0.85f), -92, new Vector2(-0.8f * sc, 0.05f * sc), new Vector2(1.0f * sc, 0.6f * sc));
            clouds[i] = ct;
            cloudX[i] = i * 7.2f;
            cloudY[i] = Random.Range(1.6f, 4.2f);
        }

        for (int i = 0; i < 3; i++)
        {
            Vector2 sc = new Vector2(TileW / SpriteFactory.LayerW, layerH[i] / SpriteFactory.LayerH);
            layerA[i] = Add(root, null, Color.white, -80 + i * 10, Vector2.zero, sc);
            layerB[i] = Add(root, null, Color.white, -80 + i * 10, Vector2.zero, sc);
        }

        Vector2 stripSc = new Vector2(TileW / SpriteFactory.SurfW, 0.4f / SpriteFactory.StripH);
        stripA = Add(root, null, Color.white, -52, Vector2.zero, stripSc);
        stripB = Add(root, null, Color.white, -52, Vector2.zero, stripSc);
        Vector2 surfSc = new Vector2(TileW / SpriteFactory.SurfW, 1.8f / SpriteFactory.SurfH);
        surfA = Add(root, null, Color.white, -50, Vector2.zero, surfSc);
        surfB = Add(root, null, Color.white, -50, Vector2.zero, surfSc);

        lineTop = Add(root, SpriteFactory.Square, new Color(1f, 1f, 1f, 0.85f), -45, new Vector2(0f, -3.45f), new Vector2(24f, 0.07f));
        lineMid = Add(root, SpriteFactory.Square, new Color(1f, 1f, 1f, 0.8f), -45, new Vector2(0f, -4.2f), new Vector2(24f, 0.06f));
        lineBot = Add(root, SpriteFactory.Square, new Color(1f, 1f, 1f, 0.85f), -45, new Vector2(0f, -4.95f), new Vector2(24f, 0.07f));
        for (int i = 0; i < dashes.Length; i++)
        {
            dashRs[i] = Add(root, SpriteFactory.Square, new Color(1f, 0.85f, 0.2f, 0.9f), -44, new Vector2(0f, -4.3f), new Vector2(1.2f, 0.08f));
            dashes[i] = dashRs[i].transform;
        }

        for (int i = 0; i < props.Length; i++)
        {
            props[i] = new GameObject("prop").transform;
            props[i].SetParent(root, false);
        }

        BuildFinish();
        SetTrack(0);
        ApplyScroll();
    }

    void BuildProps(Theme t)
    {
        for (int i = 0; i < props.Length; i++)
        {
            var p = props[i];
            for (int c = p.childCount - 1; c >= 0; c--) Destroy(p.GetChild(c).gameObject);
            float sc = 0.9f + Mathf.Abs(Mathf.Sin(i * 12.9898f)) * 0.35f;
            switch (t.propKind)
            {
                case 0:
                    Add(p, SpriteFactory.Square, new Color(0.35f, 0.22f, 0.12f), -40, new Vector2(0f, 0.6f * sc), new Vector2(0.35f * sc, 1.2f * sc));
                    Add(p, SpriteFactory.Triangle, new Color(0.08f, 0.32f, 0.16f), -39, new Vector2(0f, 1.7f * sc), new Vector2(1.9f * sc, 1.6f * sc));
                    Add(p, SpriteFactory.Triangle, new Color(0.10f, 0.38f, 0.19f), -38, new Vector2(0f, 2.5f * sc), new Vector2(1.55f * sc, 1.4f * sc));
                    Add(p, SpriteFactory.Triangle, new Color(0.13f, 0.44f, 0.22f), -37, new Vector2(0f, 3.2f * sc), new Vector2(1.15f * sc, 1.2f * sc));
                    break;
                case 1:
                    Add(p, SpriteFactory.Square, new Color(0.18f, 0.19f, 0.22f), -40, new Vector2(0f, 1.8f), new Vector2(0.12f, 3.6f));
                    Add(p, SpriteFactory.Square, new Color(0.18f, 0.19f, 0.22f), -40, new Vector2(0.45f, 3.55f), new Vector2(1.0f, 0.1f));
                    Add(p, SpriteFactory.Circle, new Color(1f, 0.95f, 0.7f), -39, new Vector2(0.95f, 3.48f), new Vector2(0.42f, 0.16f));
                    Add(p, SpriteFactory.Circle, new Color(1f, 0.92f, 0.6f, 0.16f), -41, new Vector2(0.95f, 3.2f), new Vector2(1.6f, 1.6f));
                    break;
                case 2:
                    Add(p, SpriteFactory.Square, new Color(0.30f, 0.32f, 0.38f), -40, new Vector2(0f, 3f), new Vector2(0.22f, 6f));
                    Add(p, SpriteFactory.Square, new Color(0.12f, 0.13f, 0.17f), -39, new Vector2(0f, 6.1f), new Vector2(1.5f, 0.55f));
                    for (int k = 0; k < 6; k++) Add(p, SpriteFactory.Circle, new Color(1f, 0.98f, 0.85f), -38, new Vector2(-0.55f + k * 0.22f, 6.1f), new Vector2(0.16f, 0.16f));
                    Add(p, SpriteFactory.Circle, new Color(0.85f, 0.9f, 1f, 0.18f), -41, new Vector2(0f, 6.1f), new Vector2(4.5f, 4.5f));
                    break;
                default:
                    Color[] fc = { new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.5f, 0.95f), new Color(0.2f, 0.75f, 0.35f), new Color(0.98f, 0.8f, 0.15f) };
                    Add(p, SpriteFactory.Square, new Color(0.95f, 0.95f, 0.97f), -40, new Vector2(0f, 2.2f), new Vector2(0.1f, 4.4f));
                    Add(p, SpriteFactory.Square, fc[i % 4], -39, new Vector2(0.5f, 3.95f), new Vector2(1.0f, 0.6f));
                    Add(p, SpriteFactory.Square, Shade(fc[i % 4], 0.8f), -38, new Vector2(0.5f, 3.75f), new Vector2(1.0f, 0.2f));
                    break;
            }
        }
    }

    ThemeSprites GetSprites(int idx)
    {
        ThemeSprites s;
        if (cache.TryGetValue(idx, out s)) return s;
        var t = Themes[idx];
        s = new ThemeSprites();
        s.sky = SpriteFactory.Gradient(t.skyTop, t.skyBot);
        s.layers = new Sprite[3];
        s.layers[0] = SpriteFactory.Layer(t.k1, t.c1, idx * 10 + 1, t.accent, t.bright);
        s.layers[1] = SpriteFactory.Layer(t.k2, t.c2, idx * 10 + 2, t.accent, t.bright);
        s.layers[2] = SpriteFactory.Layer(t.k3, t.c3, idx * 10 + 3, t.accent, t.bright);
        s.strip = SpriteFactory.Strip(t.stripKind, t.strip, idx * 10 + 4);
        s.surf = SpriteFactory.Surface(t.groundKind, t.ground, idx * 10 + 5);
        cache[idx] = s;
        return s;
    }

    public void SetTrack(int idx)
    {
        idx = Mathf.Clamp(idx, 0, Themes.Length - 1);
        if (idx == themeIdx) return;
        themeIdx = idx;
        var t = Themes[idx];
        var sp = GetSprites(idx);
        skyR.sprite = sp.sky;
        sunR.color = t.sun;
        glowR.color = new Color(t.sun.r, t.sun.g, t.sun.b, t.night ? 0.12f : 0.22f);
        foreach (var s in stars) s.gameObject.SetActive(t.night);
        foreach (var c in clouds) c.gameObject.SetActive(!t.night);
        for (int i = 0; i < 3; i++)
        {
            layerA[i].sprite = sp.layers[i];
            layerB[i].sprite = sp.layers[i];
        }
        stripA.sprite = sp.strip;
        stripB.sprite = sp.strip;
        surfA.sprite = sp.surf;
        surfB.sprite = sp.surf;
        lineTop.gameObject.SetActive(t.lineKind != 0);
        lineBot.gameObject.SetActive(t.lineKind != 0);
        lineMid.gameObject.SetActive(t.lineKind == 2);
        foreach (var d in dashes) d.gameObject.SetActive(t.lineKind == 1);
        BuildProps(t);
        ApplyScroll();
    }

    void ApplyScroll()
    {
        for (int i = 0; i < 3; i++)
        {
            float off = (scroll * factors[i]) % TileW;
            float py = -3.2f + layerH[i] / 2f;
            layerA[i].transform.localPosition = new Vector3(-off, py, 0f);
            layerB[i].transform.localPosition = new Vector3(-off + TileW, py, 0f);
        }
        float g = scroll % TileW;
        stripA.transform.localPosition = new Vector3(-g, -3.2f, 0f);
        stripB.transform.localPosition = new Vector3(-g + TileW, -3.2f, 0f);
        surfA.transform.localPosition = new Vector3(-g, -4.3f, 0f);
        surfB.transform.localPosition = new Vector3(-g + TileW, -4.3f, 0f);
        for (int i = 0; i < dashes.Length; i++)
        {
            float x = ((i * 3f - scroll) % 36f + 36f) % 36f - 18f;
            dashes[i].localPosition = new Vector3(x, -4.3f, 0f);
        }
        for (int i = 0; i < props.Length; i++)
        {
            float x = ((i * 8f - scroll) % 48f + 48f) % 48f - 24f;
            props[i].localPosition = new Vector3(x, -3.1f, 0f);
        }
        for (int i = 0; i < clouds.Length; i++)
        {
            float x = ((cloudX[i] - scroll * 0.03f - Time.time * 0.25f) % 36f + 36f) % 36f - 18f;
            clouds[i].localPosition = new Vector3(x, cloudY[i], 0f);
        }
    }

    void Capsule(Transform parent, Vector2 center, float w, float h, Color c, int order, List<SpriteRenderer> list)
    {
        float body = Mathf.Max(0.001f, h - w);
        list.Add(Add(parent, SpriteFactory.Square, c, order, center, new Vector2(w, body)));
        list.Add(Add(parent, SpriteFactory.Circle, c, order, new Vector2(center.x, center.y + body / 2f), new Vector2(w, w)));
        list.Add(Add(parent, SpriteFactory.Circle, c, order, new Vector2(center.x, center.y - body / 2f), new Vector2(w, w)));
    }

    Transform NewPivot(Transform parent, float x, float yy)
    {
        var p = new GameObject("pivot").transform;
        p.SetParent(parent, false);
        p.localPosition = new Vector3(x, yy, 0f);
        return p;
    }

    void ShoePart(Transform pivot, float yb, ShoeLook sh, int ro, float f, List<SpriteRenderer> list)
    {
        float th = sh.thick;
        if (sh.style == 4) list.Add(Add(pivot, SpriteFactory.Circle, new Color(sh.accent.r, sh.accent.g, sh.accent.b, 0.35f), ro, new Vector2(-0.1f, yb + 0.12f), new Vector2(0.4f, 0.2f)));
        list.Add(Add(pivot, SpriteFactory.Square, Shade(sh.sole, f), ro + 1, new Vector2(0.06f, yb + th / 2f), new Vector2(0.36f, th)));
        list.Add(Add(pivot, SpriteFactory.Circle, Shade(sh.sole, f), ro + 1, new Vector2(0.24f, yb + th / 2f), new Vector2(th, th)));
        list.Add(Add(pivot, SpriteFactory.Square, Shade(sh.upper, f), ro + 2, new Vector2(0.03f, yb + th + 0.06f), new Vector2(0.26f, 0.12f)));
        list.Add(Add(pivot, SpriteFactory.Circle, Shade(sh.upper, f), ro + 2, new Vector2(0.16f, yb + th + 0.06f), new Vector2(0.16f, 0.12f)));
        if (sh.style >= 1) list.Add(Add(pivot, SpriteFactory.Square, sh.accent, ro + 3, new Vector2(0.02f, yb + th + 0.075f), new Vector2(0.14f, 0.028f)));
        if (sh.style == 2) list.Add(Add(pivot, SpriteFactory.Circle, sh.accent, ro + 3, new Vector2(0.06f, yb + th / 2f), new Vector2(th * 0.7f, th * 0.7f)));
        if (sh.style == 3) list.Add(Add(pivot, SpriteFactory.Square, sh.accent, ro + 3, new Vector2(0.06f, yb + th * 0.5f), new Vector2(0.3f, 0.02f)));
        if (sh.style == 4) list.Add(Add(pivot, SpriteFactory.Square, sh.accent, ro + 3, new Vector2(0.06f, yb + th * 0.5f), new Vector2(0.34f, 0.02f)));
    }

    Transform Leg(Transform parent, float x, float f, int order, Look look, ShoeLook sh, List<SpriteRenderer> list)
    {
        bool tights = look.longLegs || look.topStyle == 3;
        int ro = order * 10;
        var pivot = NewPivot(parent, x, 0f);
        Color legCol = tights ? look.bottom : look.skin;
        Capsule(pivot, new Vector2(0f, -0.39f), 0.17f, 0.78f, Shade(legCol, f), ro, list);
        if (!tights) Capsule(pivot, new Vector2(0f, -0.12f), 0.21f, 0.34f, Shade(look.bottom, f), ro + 1, list);
        else if (look.topStyle == 3) list.Add(Add(pivot, SpriteFactory.Square, look.accent, ro + 1, new Vector2(0.03f, -0.35f), new Vector2(0.025f, 0.5f)));
        ShoePart(pivot, -0.78f, sh, ro + 2, f, list);
        return pivot;
    }

    Transform Arm(Transform parent, float x, float yy, float f, int order, Look look, List<SpriteRenderer> list)
    {
        bool sleeves = look.topStyle >= 2;
        int ro = order * 10;
        var pivot = NewPivot(parent, x, yy);
        Color armCol = sleeves ? look.top : look.skin;
        Capsule(pivot, new Vector2(0f, -0.27f), 0.13f, 0.56f, Shade(armCol, f), ro, list);
        if (look.wristbands) list.Add(Add(pivot, SpriteFactory.Square, look.accent, ro + 1, new Vector2(0f, -0.46f), new Vector2(0.15f, 0.07f)));
        list.Add(Add(pivot, SpriteFactory.Circle, Shade(look.skin, f), ro + 1, new Vector2(0f, -0.56f), new Vector2(0.12f, 0.12f)));
        return pivot;
    }

    public void ApplyLook(ShopItem runner, ShopItem shoe, bool randomTop)
    {
        if (runnerRoot != null) Destroy(runnerRoot.gameObject);
        var look = GetLook(runner.id);
        if (randomTop && look.randomTop) look.top = Color.HSVToRGB(Random.value, 0.75f, 0.92f);
        var sh = GetShoe(shoe.id);

        runnerRoot = new GameObject("Runner").transform;
        runnerRoot.SetParent(root, false);
        runnerRoot.position = new Vector3(RunnerX, FootY, 0f);
        runnerRoot.localScale = Vector3.one * look.scale;
        var list = new List<SpriteRenderer>();
        const float O = 0.78f;

        legB = Leg(runnerRoot, -0.03f, 0.78f, 8, look, sh, list);
        legB.localPosition = new Vector3(-0.03f, O, 0f);
        legF = Leg(runnerRoot, 0.03f, 1f, 13, look, sh, list);
        legF.localPosition = new Vector3(0.03f, O, 0f);

        bodyT = NewPivot(runnerRoot, 0f, O);
        armB = Arm(bodyT, -0.02f, 1.47f - O, 0.8f, 8, look, list);
        armF = Arm(bodyT, 0.03f, 1.47f - O, 1f, 14, look, list);

        float bw = look.bodyW;
        if (look.topStyle == 1)
        {
            Capsule(bodyT, new Vector2(0f, 0.95f - O), bw * 0.9f, 0.3f, look.skin, 100, list);
            Capsule(bodyT, new Vector2(0f, 1.3f - O), bw, 0.5f, look.top, 110, list);
        }
        else
        {
            Capsule(bodyT, new Vector2(0f, 1.15f - O), bw, 0.82f, look.top, 110, list);
        }
        list.Add(Add(bodyT, SpriteFactory.Square, look.bottom, 112, new Vector2(0f, 0.82f - O), new Vector2(bw * 0.95f, 0.2f)));
        if (look.topStyle == 3)
        {
            list.Add(Add(bodyT, SpriteFactory.Square, look.accent, 113, new Vector2(0.06f, 1.2f - O), new Vector2(0.05f, 0.62f)));
            list.Add(Add(bodyT, SpriteFactory.Circle, look.accent, 113, new Vector2(0.1f, 1.38f - O), new Vector2(0.12f, 0.12f)));
        }
        else if (look.topStyle != 1)
        {
            list.Add(Add(bodyT, SpriteFactory.Square, Color.white, 113, new Vector2(0.02f, 1.2f - O), new Vector2(0.22f, 0.17f)));
            list.Add(Add(bodyT, SpriteFactory.Square, new Color(0.15f, 0.15f, 0.2f), 114, new Vector2(0.02f, 1.2f - O), new Vector2(0.12f, 0.06f)));
        }

        if (look.scarf)
        {
            scarfT = NewPivot(bodyT, -0.1f, 1.62f - O);
            Capsule(scarfT, new Vector2(0f, -0.4f), 0.11f, 0.85f, look.accent, 90, list);
        }
        else scarfT = null;

        if (look.hairStyle == 1)
        {
            tail = NewPivot(bodyT, -0.18f, 1.95f - O);
            Capsule(tail, new Vector2(0f, -0.25f), 0.12f, 0.55f, look.hair, 108, list);
            list.Add(Add(tail, SpriteFactory.Circle, look.accent, 109, Vector2.zero, new Vector2(0.11f, 0.11f)));
        }
        else tail = null;

        Vector2 hc = new Vector2(0.04f, 1.86f - O);
        list.Add(Add(bodyT, SpriteFactory.Circle, look.hair, 118, new Vector2(hc.x - 0.04f, hc.y + 0.06f), new Vector2(0.5f, 0.5f)));
        list.Add(Add(bodyT, SpriteFactory.Circle, look.skin, 120, hc, new Vector2(0.5f, 0.5f)));
        list.Add(Add(bodyT, SpriteFactory.Circle, Shade(look.skin, 0.88f), 121, new Vector2(hc.x - 0.07f, hc.y - 0.03f), new Vector2(0.1f, 0.1f)));
        list.Add(Add(bodyT, SpriteFactory.Circle, new Color(0.08f, 0.06f, 0.06f), 126, new Vector2(hc.x + 0.1f, hc.y + 0.02f), new Vector2(0.06f, 0.06f)));
        list.Add(Add(bodyT, SpriteFactory.Square, Shade(look.skin, 0.7f), 126, new Vector2(hc.x + 0.12f, hc.y - 0.1f), new Vector2(0.09f, 0.018f)));

        if (look.hairStyle != 3)
        {
            list.Add(Add(bodyT, SpriteFactory.Circle, look.hair, 125, new Vector2(hc.x - 0.04f, hc.y + 0.17f), new Vector2(0.38f, 0.38f)));
        }
        if (look.hairStyle == 2)
        {
            list.Add(AddR(bodyT, SpriteFactory.Triangle, look.hair, 124, new Vector2(hc.x - 0.07f, hc.y + 0.36f), new Vector2(0.2f, 0.3f), 18f));
            list.Add(AddR(bodyT, SpriteFactory.Triangle, look.hair, 124, new Vector2(hc.x + 0.03f, hc.y + 0.4f), new Vector2(0.2f, 0.34f), 0f));
            list.Add(AddR(bodyT, SpriteFactory.Triangle, look.hair, 124, new Vector2(hc.x + 0.13f, hc.y + 0.35f), new Vector2(0.2f, 0.28f), -18f));
        }
        if (look.hairStyle == 4)
        {
            list.Add(AddR(bodyT, SpriteFactory.Triangle, look.hair, 124, new Vector2(hc.x + 0.1f, hc.y + 0.27f), new Vector2(0.26f, 0.2f), -75f));
        }
        if (look.beard)
        {
            list.Add(Add(bodyT, SpriteFactory.Circle, look.hair, 123, new Vector2(hc.x + 0.07f, hc.y - 0.12f), new Vector2(0.32f, 0.26f)));
        }
        if (look.headband)
        {
            list.Add(Add(bodyT, SpriteFactory.Square, look.accent, 130, new Vector2(hc.x, hc.y + 0.12f), new Vector2(0.46f, 0.08f)));
            list.Add(AddR(bodyT, SpriteFactory.Square, look.accent, 130, new Vector2(hc.x - 0.3f, hc.y + 0.1f), new Vector2(0.18f, 0.05f), 20f));
        }
        if (look.glasses)
        {
            list.Add(Add(bodyT, SpriteFactory.Square, new Color(0.05f, 0.05f, 0.08f), 131, new Vector2(hc.x + 0.13f, hc.y + 0.04f), new Vector2(0.2f, 0.09f)));
            list.Add(Add(bodyT, SpriteFactory.Square, new Color(0.05f, 0.05f, 0.08f), 131, new Vector2(hc.x - 0.02f, hc.y + 0.05f), new Vector2(0.14f, 0.025f)));
        }
        if (look.goggles)
        {
            list.Add(Add(bodyT, SpriteFactory.Square, look.accent, 131, new Vector2(hc.x, hc.y + 0.05f), new Vector2(0.52f, 0.06f)));
            list.Add(Add(bodyT, SpriteFactory.Square, new Color(0.3f, 0.8f, 1f, 0.9f), 132, new Vector2(hc.x + 0.13f, hc.y + 0.04f), new Vector2(0.22f, 0.13f)));
        }

        for (int i = 0; i < list.Count; i++) list[i].sortingOrder += bandOffset;
        runnerSR = list.ToArray();
        runnerBase = new Color[runnerSR.Length];
        for (int i = 0; i < runnerSR.Length; i++) runnerBase[i] = runnerSR[i].color;
        blinked = false;
        AnimateRunner(0f, 0f);
    }

    void BuildFinish()
    {
        finishT = new GameObject("Finish").transform;
        finishT.SetParent(root, false);
        Color pole = new Color(0.9f, 0.9f, 0.92f);
        Add(finishT, SpriteFactory.Square, pole, 6, new Vector2(-1.3f, 2f), new Vector2(0.18f, 4f));
        Add(finishT, SpriteFactory.Square, pole, 6, new Vector2(1.3f, 2f), new Vector2(0.18f, 4f));
        for (int c = 0; c < 9; c++)
        {
            for (int r = 0; r < 2; r++)
            {
                bool dark = (c + r) % 2 == 0;
                Add(finishT, SpriteFactory.Square, dark ? Color.black : Color.white, 7, new Vector2(-1.2f + c * 0.3f, 3.75f - r * 0.3f), new Vector2(0.3f, 0.3f));
            }
        }
        for (int r = 0; r < 7; r++)
        {
            for (int c = 0; c < 2; c++)
            {
                bool dark = (c + r) % 2 == 0;
                Add(finishT, SpriteFactory.Square, dark ? Color.black : Color.white, -43, new Vector2(-0.15f + c * 0.3f, 0.45f - r * 0.3f), new Vector2(0.3f, 0.3f));
            }
        }
        finishT.gameObject.SetActive(false);
    }

    Obstacle MakeObstacle(int kind, float x)
    {
        var go = new GameObject("Obstacle");
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(x, FootY, 0f);
        var p = go.transform;
        var o = new Obstacle { t = p, x = x };
        var shadow = new Color(0f, 0f, 0f, 0.28f);
        switch (kind)
        {
            case 0:
                {
                    Color bark = new Color(0.42f, 0.26f, 0.13f);
                    Add(p, SpriteFactory.Circle, shadow, 4, new Vector2(0f, 0.03f), new Vector2(2.4f, 0.22f));
                    Add(p, SpriteFactory.Square, bark, 5, new Vector2(0f, 0.3f), new Vector2(1.7f, 0.5f));
                    Add(p, SpriteFactory.Circle, Shade(bark, 0.85f), 5, new Vector2(-0.85f, 0.3f), new Vector2(0.5f, 0.5f));
                    Add(p, SpriteFactory.Circle, new Color(0.78f, 0.60f, 0.38f), 6, new Vector2(0.85f, 0.3f), new Vector2(0.5f, 0.5f));
                    Add(p, SpriteFactory.Circle, new Color(0.62f, 0.45f, 0.26f), 7, new Vector2(0.85f, 0.3f), new Vector2(0.34f, 0.34f));
                    Add(p, SpriteFactory.Circle, new Color(0.50f, 0.34f, 0.18f), 7, new Vector2(0.85f, 0.3f), new Vector2(0.16f, 0.16f));
                    for (int i = 0; i < 4; i++) Add(p, SpriteFactory.Square, Shade(bark, 0.7f), 6, new Vector2(-0.6f + i * 0.4f, 0.25f + (i % 2) * 0.1f), new Vector2(0.45f, 0.04f));
                    Add(p, SpriteFactory.Square, new Color(0.30f, 0.50f, 0.20f), 6, new Vector2(-0.3f, 0.56f), new Vector2(0.7f, 0.07f));
                    AddR(p, SpriteFactory.Square, bark, 5, new Vector2(-0.55f, 0.72f), new Vector2(0.5f, 0.1f), 35f);
                    Add(p, SpriteFactory.Circle, new Color(0.2f, 0.55f, 0.2f), 6, new Vector2(-0.42f, 0.92f), new Vector2(0.26f, 0.26f));
                    Add(p, SpriteFactory.Circle, new Color(0.25f, 0.62f, 0.25f), 6, new Vector2(-0.25f, 0.86f), new Vector2(0.22f, 0.22f));
                    o.hw = 0.85f;
                    o.hh = 0.5f;
                    break;
                }
            case 1:
                {
                    Color mud = new Color(0.27f, 0.17f, 0.09f);
                    Add(p, SpriteFactory.Circle, new Color(0.36f, 0.24f, 0.14f), 3, new Vector2(0f, 0.1f), new Vector2(2.3f, 0.42f));
                    Add(p, SpriteFactory.Circle, mud, 4, new Vector2(0f, 0.1f), new Vector2(2.0f, 0.32f));
                    Add(p, SpriteFactory.Circle, new Color(0.5f, 0.38f, 0.27f, 0.55f), 5, new Vector2(-0.35f, 0.15f), new Vector2(0.9f, 0.09f));
                    Add(p, SpriteFactory.Circle, mud, 5, new Vector2(-0.7f, 0.34f), new Vector2(0.16f, 0.16f));
                    Add(p, SpriteFactory.Circle, mud, 5, new Vector2(0.1f, 0.44f), new Vector2(0.2f, 0.2f));
                    Add(p, SpriteFactory.Circle, mud, 5, new Vector2(0.75f, 0.3f), new Vector2(0.14f, 0.14f));
                    o.hw = 0.9f;
                    o.hh = 0.2f;
                    break;
                }
            case 2:
                {
                    Add(p, SpriteFactory.Circle, new Color(0.34f, 0.35f, 0.38f), 2, new Vector2(0f, 0.08f), new Vector2(2.2f, 0.44f));
                    Add(p, SpriteFactory.Circle, new Color(0.13f, 0.13f, 0.15f), 3, new Vector2(0f, 0.08f), new Vector2(1.9f, 0.36f));
                    Add(p, SpriteFactory.Circle, new Color(0.03f, 0.03f, 0.04f), 4, new Vector2(0f, 0.08f), new Vector2(1.6f, 0.26f));
                    Add(p, SpriteFactory.Circle, new Color(0.45f, 0.46f, 0.5f), 4, new Vector2(-0.95f, 0.2f), new Vector2(0.09f, 0.09f));
                    Add(p, SpriteFactory.Circle, new Color(0.45f, 0.46f, 0.5f), 4, new Vector2(0.9f, 0.18f), new Vector2(0.08f, 0.08f));
                    AddR(p, SpriteFactory.Square, new Color(0.04f, 0.04f, 0.05f), 2, new Vector2(-1.25f, 0.12f), new Vector2(0.5f, 0.03f), 10f);
                    AddR(p, SpriteFactory.Square, new Color(0.04f, 0.04f, 0.05f), 2, new Vector2(1.3f, 0.08f), new Vector2(0.4f, 0.03f), -15f);
                    o.hw = 0.8f;
                    o.hh = 0.15f;
                    break;
                }
            case 3:
                {
                    Add(p, SpriteFactory.Circle, shadow, 4, new Vector2(0f, 0.03f), new Vector2(1.4f, 0.16f));
                    var pv = new GameObject("pv").transform;
                    pv.SetParent(p, false);
                    pv.localPosition = new Vector3(0f, 0.4f, 0f);
                    pv.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    Add(pv, SpriteFactory.Triangle, new Color(1f, 0.45f, 0.08f), 5, Vector2.zero, new Vector2(0.8f, 1f));
                    Add(pv, SpriteFactory.Square, Color.white, 6, new Vector2(0f, 0.05f), new Vector2(0.5f, 0.1f));
                    Add(pv, SpriteFactory.Square, new Color(0.15f, 0.15f, 0.15f), 5, new Vector2(0f, -0.5f), new Vector2(0.9f, 0.08f));
                    o.hw = 0.5f;
                    o.hh = 0.38f;
                    break;
                }
            case 4:
                {
                    Color post = new Color(0.85f, 0.86f, 0.90f);
                    Add(p, SpriteFactory.Square, post, 5, new Vector2(-0.62f, 0.5f), new Vector2(0.09f, 1f));
                    Add(p, SpriteFactory.Square, post, 5, new Vector2(0.62f, 0.5f), new Vector2(0.09f, 1f));
                    Add(p, SpriteFactory.Square, new Color(0.15f, 0.15f, 0.2f), 5, new Vector2(-0.62f, 0.03f), new Vector2(0.28f, 0.06f));
                    Add(p, SpriteFactory.Square, new Color(0.15f, 0.15f, 0.2f), 5, new Vector2(0.62f, 0.03f), new Vector2(0.28f, 0.06f));
                    for (int i = 0; i < 4; i++) Add(p, SpriteFactory.Square, i % 2 == 0 ? new Color(0.9f, 0.2f, 0.2f) : Color.white, 6, new Vector2(-0.42f + i * 0.28f, 0.9f), new Vector2(0.28f, 0.2f));
                    Add(p, SpriteFactory.Square, Color.white, 6, new Vector2(0f, 0.55f), new Vector2(1.2f, 0.07f));
                    o.hw = 0.5f;
                    o.hh = 0.85f;
                    break;
                }
            default:
                {
                    Color blue = new Color(0.15f, 0.45f, 0.85f);
                    Add(p, SpriteFactory.Circle, shadow, 4, new Vector2(0f, 0.03f), new Vector2(1.8f, 0.18f));
                    Add(p, SpriteFactory.Square, blue, 5, new Vector2(0f, 0.27f), new Vector2(1.2f, 0.5f));
                    Add(p, SpriteFactory.Circle, Shade(blue, 0.8f), 5, new Vector2(-0.6f, 0.27f), new Vector2(0.5f, 0.5f));
                    Add(p, SpriteFactory.Circle, new Color(0.27f, 0.62f, 0.95f), 6, new Vector2(0.6f, 0.27f), new Vector2(0.5f, 0.5f));
                    Add(p, SpriteFactory.Circle, new Color(0.17f, 0.45f, 0.80f), 7, new Vector2(0.6f, 0.27f), new Vector2(0.3f, 0.3f));
                    Add(p, SpriteFactory.Circle, new Color(0.06f, 0.20f, 0.40f), 8, new Vector2(0.6f, 0.27f), new Vector2(0.12f, 0.12f));
                    for (int i = 0; i < 5; i++) Add(p, SpriteFactory.Square, Shade(blue, 0.7f), 6, new Vector2(-0.45f + i * 0.22f, 0.27f), new Vector2(0.04f, 0.5f));
                    o.hw = 0.65f;
                    o.hh = 0.45f;
                    break;
                }
        }
        return o;
    }

    void AnimateRunner(float dt, float speed)
    {
        if (runnerRoot == null) return;
        runnerRoot.position = new Vector3(RunnerX, FootY + y, 0f);
        if (grounded)
        {
            phase += dt * speed * 2.4f;
            float s = Mathf.Sin(phase);
            legF.localRotation = Quaternion.Euler(0f, 0f, s * 50f);
            legB.localRotation = Quaternion.Euler(0f, 0f, -s * 50f);
            armF.localRotation = Quaternion.Euler(0f, 0f, -s * 55f);
            armB.localRotation = Quaternion.Euler(0f, 0f, s * 55f);
            bodyT.localRotation = Quaternion.Euler(0f, 0f, -5f);
            bodyT.localPosition = new Vector3(0f, 0.78f + Mathf.Abs(Mathf.Cos(phase)) * 0.03f, 0f);
            if (tail != null) tail.localRotation = Quaternion.Euler(0f, 0f, -35f + Mathf.Sin(phase * 2f) * 14f);
            if (scarfT != null) scarfT.localRotation = Quaternion.Euler(0f, 0f, -85f + Mathf.Sin(Time.time * 9f) * 7f);
        }
        else
        {
            legF.localRotation = Quaternion.Euler(0f, 0f, 55f);
            legB.localRotation = Quaternion.Euler(0f, 0f, -25f);
            armF.localRotation = Quaternion.Euler(0f, 0f, 150f);
            armB.localRotation = Quaternion.Euler(0f, 0f, 120f);
            bodyT.localRotation = Quaternion.Euler(0f, 0f, -8f);
            bodyT.localPosition = new Vector3(0f, 0.78f, 0f);
            if (tail != null) tail.localRotation = Quaternion.Euler(0f, 0f, -60f);
            if (scarfT != null) scarfT.localRotation = Quaternion.Euler(0f, 0f, -95f);
        }
    }

    void SetAlpha(float a)
    {
        for (int i = 0; i < runnerSR.Length; i++)
        {
            Color c = runnerBase[i];
            c.a = c.a * a;
            runnerSR[i].color = c;
        }
    }

    void ClearObstacles()
    {
        foreach (var o in obstacles) if (o.t != null) Destroy(o.t.gameObject);
        obstacles.Clear();
    }

    public int Rank()
    {
        int r = 1;
        foreach (var v in rivals) if (v.meters > meters + 0.001f) r++;
        return r;
    }

    void ClearRivals()
    {
        foreach (var v in rivals) if (v.rig != null && v.rig.root != null) Destroy(v.rig.root.gameObject);
        rivals.Clear();
    }

    Rig MakeRig(ShopItem runner, ShopItem shoe, int band)
    {
        var sRoot = runnerRoot; var sBody = bodyT; var sLF = legF; var sLB = legB; var sAF = armF; var sAB = armB;
        var sTail = tail; var sScarf = scarfT; var sSR = runnerSR; var sBase = runnerBase; bool sBlink = blinked;
        runnerRoot = null;
        bandOffset = band;
        ApplyLook(runner, shoe, true);
        bandOffset = 300;
        var rig = new Rig { root = runnerRoot, body = bodyT, legF = legF, legB = legB, armF = armF, armB = armB, tail = tail, scarf = scarfT };
        runnerRoot = sRoot; bodyT = sBody; legF = sLF; legB = sLB; armF = sAF; armB = sAB;
        tail = sTail; scarfT = sScarf; runnerSR = sSR; runnerBase = sBase; blinked = sBlink;
        return rig;
    }

    void CreateRivals(ShopItem player)
    {
        ClearRivals();
        if (mode == 0) return;
        var pool = new List<ShopItem>();
        foreach (var it in Catalog.Runners) if (it.id != player.id) pool.Add(it);
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int pi = Random.Range(0, pool.Count);
            var ri = pool[pi];
            pool.RemoveAt(pi);
            var shoe = Catalog.Shoes[Random.Range(0, Catalog.Shoes.Count)];
            var rv = new Rival { name = ri.name.ToUpper(), color = TagColors[i], offset = i * 0.035f, phase = Random.value * 6.28f, laneY = LaneY[i], laneScale = LaneScale[i] };
            rv.rig = MakeRig(ri, shoe, LaneBand[i]);
            rv.rig.root.localScale = rv.rig.root.localScale * rv.laneScale;
            AddR(rv.rig.root, SpriteFactory.Triangle, rv.color, LaneBand[i] + 140, new Vector2(0f, 2.9f), new Vector2(0.34f, 0.28f), 180f);
            rivals.Add(rv);
        }
        PlaceRivals(0f);
    }

    void AnimateRival(Rival v, float dt, float speed)
    {
        var r = v.rig;
        if (r == null || r.root == null) return;
        if (v.grounded)
        {
            r.phase += dt * speed * 2.4f;
            float sn = Mathf.Sin(r.phase);
            r.legF.localRotation = Quaternion.Euler(0f, 0f, sn * 50f);
            r.legB.localRotation = Quaternion.Euler(0f, 0f, -sn * 50f);
            r.armF.localRotation = Quaternion.Euler(0f, 0f, -sn * 55f);
            r.armB.localRotation = Quaternion.Euler(0f, 0f, sn * 55f);
            r.body.localRotation = Quaternion.Euler(0f, 0f, v.stumbleT > 0f ? 12f : -5f);
            r.body.localPosition = new Vector3(0f, 0.78f + Mathf.Abs(Mathf.Cos(r.phase)) * 0.03f, 0f);
            if (r.tail != null) r.tail.localRotation = Quaternion.Euler(0f, 0f, -35f + Mathf.Sin(r.phase * 2f) * 14f);
            if (r.scarf != null) r.scarf.localRotation = Quaternion.Euler(0f, 0f, -85f + Mathf.Sin(Time.time * 9f) * 7f);
        }
        else
        {
            r.legF.localRotation = Quaternion.Euler(0f, 0f, 55f);
            r.legB.localRotation = Quaternion.Euler(0f, 0f, -25f);
            r.armF.localRotation = Quaternion.Euler(0f, 0f, 150f);
            r.armB.localRotation = Quaternion.Euler(0f, 0f, 120f);
            r.body.localRotation = Quaternion.Euler(0f, 0f, -8f);
            r.body.localPosition = new Vector3(0f, 0.78f, 0f);
            if (r.tail != null) r.tail.localRotation = Quaternion.Euler(0f, 0f, -60f);
            if (r.scarf != null) r.scarf.localRotation = Quaternion.Euler(0f, 0f, -95f);
        }
    }

    void PlaceRivals(float dt)
    {
        float rate = UnitsPerSpeed / K;
        foreach (var v in rivals)
        {
            if (v.rig == null || v.rig.root == null) continue;
            float x = RunnerX + (v.meters - meters) * rate;
            v.rig.root.position = new Vector3(x, FootY + v.laneY + v.y, 0f);
            AnimateRival(v, dt, v.speed);
        }
    }

    void TickRivals(float dt)
    {
        if (rivals.Count == 0) return;
        int d = (int)diff;
        float rate = UnitsPerSpeed / K;
        float finishScroll = target * UnitsPerSpeed / K;
        float dur = Mathf.Max(1f, target / (maxSpeed * K));
        float gap = HurdleGap();
        foreach (var v in rivals)
        {
            if (v.stumbleT > 0f) v.stumbleT -= dt;
            float lead = mode == 1 ? Mathf.Lerp(0.98f, 0.88f, Mathf.Clamp01(dur / 90f)) + DiffRivalTap[d] : 1f + DiffRivalH[d];
            float ratio = lead - v.offset + 0.03f * Mathf.Sin(realTime * 0.6f + v.phase);
            float rub = Mathf.Clamp((meters - v.meters) / Mathf.Max(60f, target * 0.05f), -1f, 1f) * 0.035f;
            float goal = maxSpeed * (ratio + rub);
            if (v.stumbleT > 0f) goal *= 0.3f;
            v.speed = Mathf.MoveTowards(v.speed, goal, (goal < v.speed ? 40f : 10f) * dt);
            v.meters += v.speed * K * dt;
            if (v.finishTime < 0f && v.meters >= target) v.finishTime = virtTime;

            if (mode == 2)
            {
                float R = v.meters * rate;
                float S = hurdleFirstS + v.nextHurdle * gap;
                if (S + 12f < finishScroll)
                {
                    float dist = S - R;
                    if (!v.decided && dist > 0f && dist < 0.38f * v.speed * UnitsPerSpeed)
                    {
                        v.decided = true;
                        v.willMiss = Random.value < MissP[d];
                        if (!v.willMiss && v.grounded)
                        {
                            v.vy = 11.5f;
                            v.grounded = false;
                        }
                    }
                    if (dist <= 0.3f)
                    {
                        if (v.willMiss) v.stumbleT = 1.2f;
                        v.nextHurdle++;
                        v.decided = false;
                        v.willMiss = false;
                    }
                }
            }

            if (!v.grounded)
            {
                v.vy += Gravity * dt;
                v.y += v.vy * dt;
                if (v.y <= 0f)
                {
                    v.y = 0f;
                    v.vy = 0f;
                    v.grounded = true;
                }
            }
        }
        PlaceRivals(dt);
    }

    public void Begin(int targetMeters, Difficulty d, ShopItem shoe, ShopItem runner, ShopItem track, float upSpeed, float upJump, float upStamina, float upRecovery)
    {
        ClearObstacles();
        ClearRivals();
        finishT.gameObject.SetActive(false);
        SetTrack(track.theme);
        ApplyLook(runner, shoe, true);
        target = targetMeters;
        diff = d;
        K = Mathf.Max(1f, target / (BaseSpeed * (mode == 1 ? 90f : 150f)));
        maxSpeed = BaseSpeed * (1f + shoe.speed + runner.speed + upSpeed);
        jumpV = 11.5f * (1f + shoe.jump + runner.jump + upJump);
        float st = shoe.stamina + runner.stamina + upStamina;
        staminaStat = st;
        tapTimes.Clear();
        taps = 0;
        tapRate = 0f;
        Effort = 0f;
        hurdlesPassed = 0;
        regen = 24f * (1f + st);
        jumpCost = 20f * (1f - Mathf.Min(0.6f, st) * 0.5f);
        stumbleDur = 1.2f * (1f - upRecovery);
        stumbleFactor = 0.3f + upRecovery * 0.75f;
        stamina = 100f;
        scroll = 0f;
        meters = 0f;
        realTime = 0f;
        virtTime = 0f;
        speedMS = 0f;
        stumbleT = 0f;
        invT = 0f;
        hits = 0;
        y = 0f;
        vy = 0f;
        grounded = true;
        jumpsUsed = 0;
        finishOn = false;
        finished = false;
        spawnCountdown = mode == 1 ? 1e9f : (mode == 2 ? HurdleGap() * 0.8f : 12f);
        hurdleFirstS = HurdleGap() * 0.8f + (SpawnX - RunnerX);
        countdown = 3f;
        racing = true;
        Random.InitState(System.Environment.TickCount);
        CreateRivals(runner);
        AnimateRunner(0f, 0f);
        ApplyScroll();
    }

    public void Stop()
    {
        ClearRivals();
        racing = false;
        countdown = 0f;
        ClearObstacles();
        finishT.gameObject.SetActive(false);
        y = 0f;
        vy = 0f;
        grounded = true;
        if (runnerSR != null) SetAlpha(1f);
    }

    public void Idle(float dt)
    {
        if (racing) return;
        scroll += 3f * dt;
        AnimateRunner(dt, 4.5f);
        ApplyScroll();
    }

    public void Tap()
    {
        if (!racing || countdown > 0f || finished || mode != 1) return;
        taps++;
        tapTimes.Add(realTime);
    }

    float HurdleGap()
    {
        float g;
        if (target <= 100) g = 17f;
        else if (target <= 400) g = 24f;
        else if (target <= 800) g = 30f;
        else if (target <= 1000) g = 32f;
        else if (target <= 3000) g = 38f;
        else if (target <= 5000) g = 42f;
        else g = 46f;
        return Mathf.Max(14f, g * DiffGap[(int)diff]);
    }

    public void TryJump()
    {
        if (mode == 1) return;
        if (!racing || countdown > 0f || finished) return;
        if (grounded) DoJump(1f);
        else if (jumpsUsed < 2) DoJump(0.85f);
    }

    void DoJump(float f)
    {
        if (OnEvent != null) OnEvent(grounded ? 0 : 1);
        bool weak = stamina < jumpCost;
        stamina = Mathf.Max(0f, stamina - jumpCost);
        vy = jumpV * f * (weak ? 0.6f : 1f);
        grounded = false;
        jumpsUsed++;
    }

    void SpawnGroup()
    {
        if (mode == 2)
        {
            float gx = SpawnX - Mathf.Max(0f, -spawnCountdown);
            obstacles.Add(MakeObstacle(4, gx));
            for (int i = 0; i < rivals.Count; i++)
            {
                var lo = MakeObstacle(4, gx);
                lo.lane = i;
                lo.yOff = rivals[i].laneY;
                lo.t.position = new Vector3(gx, FootY + lo.yOff, 0f);
                obstacles.Add(lo);
            }
            spawnCountdown += HurdleGap();
            return;
        }
        int d = (int)diff;
        int[] kinds = Themes[themeIdx].obst;
        int n = 1;
        if (Random.value < ComboChance[d]) n = Random.Range(2, MaxGroup[d] + 1);
        float x = SpawnX;
        float len = 0f;
        for (int i = 0; i < n; i++)
        {
            obstacles.Add(MakeObstacle(kinds[Random.Range(0, kinds.Length)], x));
            float sp = Random.Range(2.9f, 3.4f);
            x += sp;
            len += sp;
        }
        spawnCountdown = Random.Range(GapMin[d], GapMax[d]) + len;
    }

    public void Tick(float dt)
    {
        if (!racing) return;
        if (countdown > 0f)
        {
            countdown -= dt;
            AnimateRunner(dt, 0f);
            PlaceRivals(0f);
            ApplyScroll();
            return;
        }

        realTime += dt;
        if (stumbleT > 0f) stumbleT -= dt;
        if (invT > 0f) invT -= dt;

        float factor = stumbleT > 0f ? stumbleFactor : 1f;
        if (stamina < 15f) factor *= 0.93f;
        float decel = 40f;
        float accel = 12f;
        if (mode == 1)
        {
            while (tapTimes.Count > 0 && tapTimes[0] < realTime - 1f) tapTimes.RemoveAt(0);
            tapRate = tapTimes.Count;
            Effort = Mathf.Clamp(tapRate / TapRef, 0f, 1.15f);
            float curve = Effort <= 0.02f ? 0f : Mathf.Pow(Mathf.Min(Effort, 1f), 0.85f) + Mathf.Max(0f, Effort - 1f) * 0.6f;
            factor = curve * Mathf.Lerp(0.85f, 1f, Mathf.Clamp01(stamina / 30f));
            decel = 3.5f;
            accel = 9f;
        }
        float goal = maxSpeed * factor;
        speedMS = Mathf.MoveTowards(speedMS, goal, (goal < speedMS ? decel : accel) * dt);
        float du = speedMS * UnitsPerSpeed * dt;
        scroll += du;
        meters = Mathf.Min(target, scroll * K / UnitsPerSpeed);
        virtTime = realTime * K;

        if (!grounded)
        {
            vy += Gravity * dt;
            y += vy * dt;
            if (y <= 0f)
            {
                y = 0f;
                vy = 0f;
                grounded = true;
                jumpsUsed = 0;
                if (OnEvent != null) OnEvent(2);
            }
        }
        else if (mode != 1)
        {
            stamina = Mathf.Min(100f, stamina + regen * dt);
        }
        if (mode == 1 && grounded)
        {
            if (Effort > 0.6f) stamina = Mathf.Max(0f, stamina - (Effort - 0.6f) * 12f / (1f + staminaStat) * dt);
            else stamina = Mathf.Min(100f, stamina + regen * 0.5f * dt);
        }

        float finishScroll = target * UnitsPerSpeed / K;
        float ahead = SpawnX - RunnerX;

        if (!finishOn && scroll + ahead >= finishScroll)
        {
            finishOn = true;
            finishX = SpawnX - (scroll + ahead - finishScroll);
            finishT.gameObject.SetActive(true);
        }

        if (finishOn)
        {
            finishX -= du;
            finishT.position = new Vector3(finishX, FootY, 0f);
            if (finishX <= RunnerX)
            {
                meters = target;
                racing = false;
                finished = true;
            }
        }
        else
        {
            spawnCountdown -= du;
            if (mode != 1 && spawnCountdown <= 0f && scroll + ahead + 12f < finishScroll) SpawnGroup();
        }

        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            var o = obstacles[i];
            o.x -= du;
            o.t.position = new Vector3(o.x, FootY + o.yOff, 0f);
            if (!o.passed && o.x < RunnerX - o.hw - 0.3f)
            {
                o.passed = true;
                if (mode == 2 && o.lane < 0) hurdlesPassed++;
            }
            if (o.x < -13f)
            {
                Destroy(o.t.gameObject);
                obstacles.RemoveAt(i);
                continue;
            }
            if (o.lane < 0 && invT <= 0f && Mathf.Abs(o.x - RunnerX) < 0.22f + o.hw && y < o.hh)
            {
                stumbleT = stumbleDur;
                invT = stumbleDur + 0.6f;
                hits++;
                stamina = Mathf.Max(0f, stamina - 15f);
                if (OnEvent != null) OnEvent(3);
            }
        }

        if (invT > 0f)
        {
            SetAlpha(Mathf.Sin(realTime * 40f) > 0f ? 1f : 0.3f);
            blinked = true;
        }
        else if (blinked)
        {
            SetAlpha(1f);
            blinked = false;
        }

        TickRivals(dt);
        AnimateRunner(dt, speedMS);
        ApplyScroll();
    }
}

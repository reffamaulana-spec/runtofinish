using System.Collections.Generic;
using UnityEngine;

public partial class Game : MonoBehaviour
{
    enum S { Login, Menu, Select, Prep, Race, Paused, Result, Shop, Missions, Profile, Train }

    static readonly int[] Targets = { 100, 400, 800, 1000, 3000, 5000, 10000 };
    static readonly string[] TargetNames = { "100 M", "400 M", "800 M", "1 KM", "3 KM", "5 KM", "10 KM" };
    static readonly string[] DiffNames = { "EASY", "NORMAL", "HARD", "ELITE" };
    static readonly string[] DiffInfo = { "Rintangan jarang", "Rintangan standar", "Rintangan padat", "Kombinasi rapat" };
    static readonly string[] DiffLock = { "", "", "Selesaikan 3 balapan", "Level 10 atau lari 10 KM" };
    static readonly string[] ModeNames = { "KLASIK", "LOMBA LARI", "HURDLES" };
    static readonly string[] ModeInfo = { "Lompati rintangan acak di lintasan", "Tap kencang, balapan lawan 3 AI", "Hindari hurdle, lawan 3 AI" };
    static readonly float[] ModeMult = { 1f, 1.2f, 1.1f };
    static readonly string[] TapInfo = { "Penuh di 6 tap/detik", "Penuh di 7 tap/detik", "Penuh di 8 tap/detik", "Penuh di 9 tap/detik" };
    static readonly string[] HurdleInfo = { "Hurdle renggang", "Hurdle standar", "Hurdle rapat", "Hurdle sangat rapat" };
    static readonly Color[] ModeColors = { new Color(1f, 0.55f, 0.08f), new Color(0.20f, 0.72f, 0.38f), new Color(0.25f, 0.55f, 0.95f) };
    static readonly string[] UpNames = { "KECEPATAN", "LOMPATAN", "STAMINA", "PEMULIHAN" };
    static readonly string[] UpDesc =
    {
        "Kecepatan lari lebih tinggi",
        "Lompatan lebih tinggi",
        "Stamina lebih awet dan cepat pulih",
        "Tersandung lebih singkat dan ringan"
    };
    static readonly float[] UpPer = { 0.025f, 0.015f, 0.03f, 0.04f };
    static readonly Color[] DiffColors =
    {
        new Color(0.20f, 0.72f, 0.38f),
        new Color(0.95f, 0.72f, 0.12f),
        new Color(0.90f, 0.27f, 0.27f),
        new Color(0.62f, 0.32f, 0.90f)
    };
    static readonly Color Orange = new Color(1f, 0.55f, 0.08f);
    static readonly Color Blue = new Color(0.25f, 0.55f, 0.95f);
    static readonly Color Slate = new Color(0.30f, 0.38f, 0.56f);
    static readonly Color Green = new Color(0.20f, 0.72f, 0.38f);
    static readonly Color Gold = new Color(1f, 0.82f, 0.18f);
    static readonly Color Purple = new Color(0.62f, 0.40f, 1f);
    static readonly Color Gray = new Color(0.74f, 0.78f, 0.88f);
    static readonly Color Glass = new Color(0.06f, 0.09f, 0.18f, 0.88f);
    static readonly Color[] UpColors = { Orange, Blue, Green, Purple };
    static readonly Rect PauseRect = new Rect(1176f, 10f, 88f, 58f);

    S state = S.Login;
    S lastState = S.Menu;
    float stateT;
    float fade = 1f;
    Race race;
    Camera mainCam;
    Camera bgCam;
    GUIStyle lbl;
    GUIStyle box16;
    GUIStyle box8;
    Texture2D gradTex;
    bool spaceHeld;
    int selTarget = 3;
    int selMode;
    float tapFlash;
    int rMode;
    float rRatio;
    float rTapAvg;
    int rPassed;
    int rRank = 1;
    int rTotal = 1;
    static readonly float[] RankMult = { 1.25f, 1.1f, 1f, 1f };
    readonly HashSet<KeyCode> heldKeys = new HashSet<KeyCode>();
    Difficulty selDiff = Difficulty.Normal;
    int shopTab;
    int shopPage;
    string toast;
    float toastT;
    float flashT;
    int flashIdx = -1;
    float shakeT;
    float stepT;
    int lastCount = -1;
    int starPlayed;

    float rMeters;
    float rTime;
    bool rPB;
    float rPrevBest;
    int rCoins;
    int rXp;
    int rUc;
    int rLvBefore;
    int rLvAfter;
    int rHits;

    ShopItem Shoe { get { return Catalog.Find(Catalog.Shoes, Store.D.shoe); } }
    ShopItem RunnerItem { get { return Catalog.Find(Catalog.Runners, Store.D.runner); } }
    ShopItem Track { get { return Catalog.Find(Catalog.Tracks, Store.D.track); } }

    float UpBonus(int kind)
    {
        return UpShown(kind, Store.UpLevel(kind));
    }

    float UpShown(int kind, int lv)
    {
        float v = lv * UpPer[kind];
        return kind == 3 ? Mathf.Min(v, 0.6f) : v;
    }

    void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        QualitySettings.antiAliasing = 4;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        Store.Load();

        foreach (var c in Camera.allCameras) c.enabled = false;

        var bg = new GameObject("BgCamera");
        bg.transform.SetParent(transform, false);
        bgCam = bg.AddComponent<Camera>();
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = Color.black;
        bgCam.cullingMask = 0;
        bgCam.depth = -100;

        var mc = new GameObject("MainCamera");
        mc.transform.SetParent(transform, false);
        mc.transform.position = new Vector3(0f, 0f, -10f);
        mainCam = mc.AddComponent<Camera>();
        mainCam.orthographic = true;
        mainCam.orthographicSize = 5f;
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = Color.black;
        mainCam.allowMSAA = true;
        mainCam.depth = 0;

        race = gameObject.AddComponent<Race>();
        race.OnEvent = RaceEvent;
        race.Build(mainCam);
        race.SetTrack(Track.theme);
        race.ApplyLook(RunnerItem, Shoe, false);
    }

    void UpdateCamRect()
    {
        float aspect = Screen.width / (float)Screen.height;
        float target = 16f / 9f;
        if (aspect > target)
        {
            float w = target / aspect;
            mainCam.rect = new Rect((1f - w) / 2f, 0f, w, 1f);
        }
        else
        {
            float h = aspect / target;
            mainCam.rect = new Rect(0f, (1f - h) / 2f, 1f, h);
        }
    }

    void Update()
    {
        UpdateCamRect();
        stateT += Time.unscaledDeltaTime;
        if (toastT > 0f) toastT -= Time.unscaledDeltaTime;
        if (flashT > 0f) flashT -= Time.unscaledDeltaTime;
        if (downArmT > 0f) downArmT -= Time.unscaledDeltaTime;
        if (tapFlash > 0f) tapFlash -= Time.unscaledDeltaTime * 6f;
        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        Sfx.MusicVolume(state == S.Race ? 0.1f : 0.2f);
        if (shakeT > 0f) shakeT -= Time.unscaledDeltaTime;
        Vector2 sh = shakeT > 0f ? Random.insideUnitCircle * shakeT * 0.5f : Vector2.zero;
        mainCam.transform.position = new Vector3(sh.x, sh.y, -10f);
        if (state == S.Race)
        {
            race.Tick(dt);
            RaceSounds(dt);
            if (race.finished) Finish();
        }
        else if (state != S.Paused)
        {
            race.Idle(dt);
        }
    }

    void RaceEvent(int e)
    {
        if (e == 0) Sfx.Play(Sfx.Id.Jump, 0.7f);
        else if (e == 1) Sfx.Play(Sfx.Id.Jump2, 0.7f);
        else if (e == 2) Sfx.Play(Sfx.Id.Land, 0.6f);
        else if (e == 3)
        {
            Sfx.Play(Sfx.Id.Hit, 1f);
            shakeT = 0.45f;
        }
    }

    void RaceSounds(float dt)
    {
        if (race.countdown > 0f)
        {
            int c = Mathf.CeilToInt(race.countdown);
            if (c != lastCount)
            {
                lastCount = c;
                Sfx.Play(Sfx.Id.Beep, 0.8f, 1f);
            }
            return;
        }
        if (lastCount > 0)
        {
            lastCount = 0;
            Sfx.Play(Sfx.Id.Go, 0.9f);
        }
        if (race.mode == 1 || !race.racing || !race.Grounded) return;
        stepT -= dt;
        if (stepT <= 0f)
        {
            stepT = Mathf.Lerp(0.42f, 0.24f, Mathf.Clamp01(race.speedMS / 8f));
            Sfx.Play(Sfx.Id.Step, 0.3f, 0.9f + Random.value * 0.25f);
        }
    }

    void OnApplicationFocus(bool focus)
    {
        if (!focus && state == S.Race) state = S.Paused;
    }

    void Reward(out int coins, out int xp, out int uc)
    {
        float m = Targets[selTarget];
        float mult = Store.DiffMult(selDiff) * (1f + Track.reward) * ModeMult[selMode] * (selMode == 0 ? 1f : RankMult[Mathf.Clamp(rRank - 1, 0, 3)]);
        coins = Mathf.RoundToInt(m / 5f * mult);
        xp = Mathf.RoundToInt(Mathf.Max(10f, m / 10f) * mult);
        uc = Mathf.Max(1, Mathf.RoundToInt(m / 50f * mult));
    }

    void BeginRace()
    {
        race.mode = selMode;
        rRank = 1;
        heldKeys.Clear();
        race.Begin(Targets[selTarget], selDiff, Shoe, RunnerItem, Track, UpBonus(0), UpBonus(1), UpBonus(2), UpBonus(3));
        spaceHeld = false;
        lastCount = -1;
        stepT = 0f;
        state = S.Race;
    }

    void Finish()
    {
        var d = Store.D;
        rMeters = race.target;
        rTime = race.virtTime;
        rHits = race.hits;
        rMode = selMode;
        rRank = selMode == 0 ? 1 : race.Rank();
        rTotal = race.rivals.Count + 1;
        rRatio = (rMeters / Mathf.Max(0.1f, rTime)) / Mathf.Max(0.1f, race.MaxSpeed);
        rTapAvg = race.taps / Mathf.Max(0.1f, race.realTime);
        rPassed = race.hurdlesPassed;
        rPrevBest = Store.GetBest(race.target, selMode);
        rPB = rPrevBest <= 0f || rTime < rPrevBest;
        if (rPB) Store.SetBest(race.target, rTime, selMode);
        int uc;
        Reward(out rCoins, out rXp, out uc);
        rUc = uc + (rPB ? 5 : 0);
        rLvBefore = Store.Level(d.xp);
        d.coins += rCoins;
        d.xp += rXp;
        d.ucoins += rUc;
        d.totalMeters += rMeters;
        d.races++;
        if (race.target >= 10000) d.done10k = true;
        Store.RegisterRace(rMeters, selDiff, rPB);
        Store.Save();
        rLvAfter = Store.Level(d.xp);
        race.Stop();
        starPlayed = 0;
        Sfx.Play(Sfx.Id.Finish);
        state = S.Result;
    }

    void ToMenu()
    {
        race.Stop();
        race.SetTrack(Track.theme);
        state = S.Menu;
    }

    void Toast(string s)
    {
        toast = s;
        toastT = 2f;
        Sfx.Play(Sfx.Id.Reward, 0.8f);
    }

    Texture2D RoundTex(int n, int r)
    {
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, r, n - r);
                float cy = Mathf.Clamp(y + 0.5f, r, n - r);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                float a = Mathf.Clamp01(r - d + 0.5f);
                px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        t.SetPixels32(px);
        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return t;
    }

    GUIStyle BoxStyle(Texture2D tex, int border)
    {
        var s = new GUIStyle();
        s.normal.background = tex;
        s.border = new RectOffset(border, border, border, border);
        s.margin = new RectOffset(0, 0, 0, 0);
        s.padding = new RectOffset(0, 0, 0, 0);
        return s;
    }

    void EnsureStyles()
    {
        if (lbl != null) return;
        lbl = new GUIStyle(GUI.skin.label);
        lbl.wordWrap = false;
        lbl.clipping = TextClipping.Overflow;
        lbl.padding = new RectOffset(0, 0, 0, 0);
        box16 = BoxStyle(RoundTex(64, 20), 20);
        box8 = BoxStyle(RoundTex(32, 10), 10);
        gradTex = new Texture2D(1, 128, TextureFormat.RGBA32, false);
        for (int y = 0; y < 128; y++)
        {
            Color c = Color.Lerp(new Color(0.12f, 0.05f, 0.24f, 0.82f), new Color(0.03f, 0.07f, 0.20f, 0.90f), y / 127f);
            gradTex.SetPixel(0, y, c);
        }
        gradTex.wrapMode = TextureWrapMode.Clamp;
        gradTex.filterMode = FilterMode.Bilinear;
        gradTex.Apply();
    }

    void Tint(Color c)
    {
        GUI.color = new Color(c.r, c.g, c.b, c.a * fade);
    }

    void Round(Rect r, Color c)
    {
        Tint(c);
        GUI.Box(r, GUIContent.none, (r.height < 44f || r.width < 44f) ? box8 : box16);
        GUI.color = Color.white;
    }

    void Flat(Rect r, Color c)
    {
        Tint(c);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void Sparkles()
    {
        for (int i = 0; i < 26; i++)
        {
            float h1 = Frac(i * 0.618f);
            float h2 = Frac(i * 0.754f);
            float h3 = Frac(i * 0.569f);
            float x = h1 * 1280f + Mathf.Sin(Time.unscaledTime * 0.8f + i) * 14f;
            float y = 720f - ((Time.unscaledTime * (18f + h2 * 30f) + h3 * 760f) % 760f);
            float sz = 3f + h2 * 5f;
            float a = 0.18f + 0.25f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 1.5f + i));
            Disc(x, y, sz, new Color(1f, 0.9f, 0.6f, a));
        }
    }

    void Backdrop(float strength)
    {
        GUI.color = new Color(1f, 1f, 1f, strength * fade);
        GUI.DrawTexture(new Rect(0f, 0f, 1280f, 720f), gradTex);
        GUI.color = Color.white;
        Sparkles();
    }

    void Card(Rect r, Color accent, bool stripe)
    {
        Round(new Rect(r.x - 2f, r.y - 2f, r.width + 4f, r.height + 4f), new Color(1f, 1f, 1f, 0.12f));
        Round(r, Glass);
        if (stripe) Round(new Rect(r.x + 10f, r.y + 12f, 5f, r.height - 24f), accent);
    }

    void T(Rect r, string s, int size, Color c, TextAnchor a = TextAnchor.MiddleCenter, bool outline = true)
    {
        lbl.fontSize = size;
        lbl.alignment = a;
        lbl.fontStyle = FontStyle.Bold;
        if (outline)
        {
            float d = size >= 40 ? 3f : 2f;
            lbl.normal.textColor = new Color(0.02f, 0.03f, 0.10f, 0.85f * fade);
            GUI.Label(new Rect(r.x - d, r.y, r.width, r.height), s, lbl);
            GUI.Label(new Rect(r.x + d, r.y, r.width, r.height), s, lbl);
            GUI.Label(new Rect(r.x, r.y - d, r.width, r.height), s, lbl);
            GUI.Label(new Rect(r.x, r.y + d, r.width, r.height), s, lbl);
            if (size >= 40)
            {
                GUI.Label(new Rect(r.x - d, r.y - d, r.width, r.height), s, lbl);
                GUI.Label(new Rect(r.x + d, r.y + d, r.width, r.height), s, lbl);
                GUI.Label(new Rect(r.x + d, r.y - d, r.width, r.height), s, lbl);
                GUI.Label(new Rect(r.x - d, r.y + d, r.width, r.height), s, lbl);
            }
        }
        lbl.normal.textColor = new Color(c.r, c.g, c.b, c.a * fade);
        GUI.Label(r, s, lbl);
    }

    bool B(Rect r, string s, Color c, int size = 28, bool on = true)
    {
        int id = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        bool hover = r.Contains(e.mousePosition);
        bool clicked = false;
        if (on)
        {
            if (e.type == EventType.MouseDown && e.button == 0 && hover)
            {
                GUIUtility.hotControl = id;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl == id)
            {
                GUIUtility.hotControl = 0;
                e.Use();
                clicked = hover;
                if (clicked) Sfx.Play(Sfx.Id.Click, 0.6f);
            }
        }
        if (e.type == EventType.Repaint)
        {
            bool down = on && GUIUtility.hotControl == id && hover;
            Color face = on ? c : new Color(0.33f, 0.35f, 0.42f);
            if (on && hover && !down) face = Color.Lerp(face, Color.white, 0.18f);
            Color edge = new Color(face.r * 0.45f, face.g * 0.45f, face.b * 0.5f, 1f);
            float lip = Mathf.Min(7f, r.height * 0.2f);
            float dy = down ? lip - 2f : 0f;
            Round(new Rect(r.x, r.y + lip, r.width, r.height - lip), edge);
            var fr = new Rect(r.x, r.y + dy, r.width, r.height - lip);
            Round(fr, face);
            Round(new Rect(fr.x + 8f, fr.y + 4f, fr.width - 16f, Mathf.Max(6f, fr.height * 0.38f)), new Color(1f, 1f, 1f, 0.16f));
            if (!string.IsNullOrEmpty(s)) T(fr, s, size, on ? Color.white : new Color(0.7f, 0.72f, 0.78f));
        }
        return clicked;
    }

    void Bar(Rect r, float f, Color c)
    {
        Round(r, new Color(0f, 0f, 0f, 0.55f));
        f = Mathf.Clamp01(f);
        if (f <= 0.001f) return;
        float w = Mathf.Max(r.height, r.width * f);
        Round(new Rect(r.x, r.y, w, r.height), c);
        Round(new Rect(r.x + 4f, r.y + 2f, Mathf.Max(4f, w - 8f), Mathf.Max(3f, r.height * 0.3f)), new Color(1f, 1f, 1f, 0.28f));
    }

    void Disc(float x, float y, float sz, Color c)
    {
        Tint(c);
        GUI.DrawTexture(new Rect(x, y, sz, sz), SpriteFactory.Circle.texture);
        GUI.color = Color.white;
    }

    void RotAt(float cx, float cy, float deg)
    {
        GUI.matrix = GUI.matrix * Matrix4x4.TRS(new Vector3(cx, cy, 0f), Quaternion.Euler(0f, 0f, deg), Vector3.one) * Matrix4x4.TRS(new Vector3(-cx, -cy, 0f), Quaternion.identity, Vector3.one);
    }

    void DrawTri(float cx, float cy, float s, float rot, Color c)
    {
        var m = GUI.matrix;
        RotAt(cx, cy, rot);
        Tint(c);
        GUI.DrawTexture(new Rect(cx - s / 2f, cy - s / 2f, s, s), SpriteFactory.Triangle.texture);
        GUI.color = Color.white;
        GUI.matrix = m;
    }

    void Coin(float x, float y, float sz)
    {
        Disc(x, y, sz, new Color(0.72f, 0.46f, 0.04f));
        Disc(x + sz * 0.07f, y + sz * 0.07f, sz * 0.86f, Gold);
        Disc(x + sz * 0.27f, y + sz * 0.27f, sz * 0.46f, new Color(1f, 0.94f, 0.55f));
    }

    void Gem(float x, float y, float sz)
    {
        var m = GUI.matrix;
        RotAt(x + sz / 2f, y + sz / 2f, 45f);
        Flat(new Rect(x + sz * 0.17f, y + sz * 0.17f, sz * 0.66f, sz * 0.66f), Purple);
        Flat(new Rect(x + sz * 0.3f, y + sz * 0.3f, sz * 0.24f, sz * 0.24f), new Color(0.92f, 0.88f, 1f, 0.95f));
        GUI.matrix = m;
    }

    void Chip(float x, float y, float w, float h, int kind, string txt, Color c)
    {
        Round(new Rect(x, y, w, h), new Color(0f, 0f, 0f, 0.35f));
        float ic = h - 14f;
        if (kind == 0) Coin(x + 8f, y + 7f, ic);
        else if (kind == 1) Badge(x + 8f, y + 7f, ic, Blue, "XP", Mathf.RoundToInt(ic * 0.38f));
        else Gem(x + 6f, y + 5f, ic + 4f);
        T(new Rect(x + h, y, w - h - 6f, h), txt, Mathf.RoundToInt(h * 0.5f), c, TextAnchor.MiddleLeft, false);
    }

    void Badge(float x, float y, float sz, Color c, string txt, int size)
    {
        Disc(x - 3f, y - 3f, sz + 6f, new Color(1f, 1f, 1f, 0.9f));
        Disc(x, y, sz, c);
        T(new Rect(x, y, sz, sz), txt, size, Color.white);
    }

    void StatIcon(int kind, float x, float y, float sz, Color c)
    {
        Disc(x - 3f, y - 3f, sz + 6f, new Color(1f, 1f, 1f, 0.9f));
        Disc(x, y, sz, c);
        float cx = x + sz / 2f;
        float cy = y + sz / 2f;
        switch (kind)
        {
            case 0:
                DrawTri(cx - sz * 0.15f, cy, sz * 0.32f, 90f, Color.white);
                DrawTri(cx + sz * 0.14f, cy, sz * 0.32f, 90f, Color.white);
                break;
            case 1:
                DrawTri(cx, cy - sz * 0.1f, sz * 0.42f, 0f, Color.white);
                Flat(new Rect(cx - sz * 0.07f, cy + sz * 0.04f, sz * 0.14f, sz * 0.22f), Color.white);
                break;
            case 2:
                Round(new Rect(cx - sz * 0.26f, cy - sz * 0.15f, sz * 0.46f, sz * 0.3f), Color.white);
                Round(new Rect(cx - sz * 0.22f, cy - sz * 0.11f, sz * 0.38f, sz * 0.22f), Shade(c, 0.5f));
                Round(new Rect(cx - sz * 0.22f, cy - sz * 0.11f, sz * 0.26f, sz * 0.22f), Color.white);
                Flat(new Rect(cx + sz * 0.2f, cy - sz * 0.06f, sz * 0.06f, sz * 0.12f), Color.white);
                break;
            default:
                Flat(new Rect(cx - sz * 0.22f, cy - sz * 0.06f, sz * 0.44f, sz * 0.12f), Color.white);
                Flat(new Rect(cx - sz * 0.06f, cy - sz * 0.22f, sz * 0.12f, sz * 0.44f), Color.white);
                break;
        }
    }

    static Color Shade(Color c, float f)
    {
        return new Color(c.r * f, c.g * f, c.b * f, c.a);
    }

    void Portrait(float x, float y, float sz, ShopItem runner, Color bg)
    {
        var l = Race.GetLook(runner.id);
        Disc(x - 3f, y - 3f, sz + 6f, new Color(1f, 1f, 1f, 0.9f));
        Disc(x, y, sz, bg);
        Round(new Rect(x + sz * 0.22f, y + sz * 0.66f, sz * 0.56f, sz * 0.27f), l.top);
        if (l.topStyle == 3) Flat(new Rect(x + sz * 0.47f, y + sz * 0.66f, sz * 0.06f, sz * 0.27f), l.accent);
        if (l.hairStyle == 1) Disc(x + sz * 0.14f, y + sz * 0.3f, sz * 0.2f, l.hair);
        if (l.hairStyle != 3) Disc(x + sz * 0.27f, y + sz * 0.1f, sz * 0.46f, l.hair);
        Disc(x + sz * 0.3f, y + sz * 0.2f, sz * 0.4f, l.skin);
        if (l.hairStyle != 3) Disc(x + sz * 0.31f, y + sz * 0.11f, sz * 0.34f, l.hair);
        if (l.hairStyle == 2)
        {
            DrawTri(x + sz * 0.38f, y + sz * 0.09f, sz * 0.14f, 0f, l.hair);
            DrawTri(x + sz * 0.5f, y + sz * 0.07f, sz * 0.16f, 0f, l.hair);
            DrawTri(x + sz * 0.6f, y + sz * 0.1f, sz * 0.13f, 15f, l.hair);
        }
        if (l.beard) Disc(x + sz * 0.36f, y + sz * 0.45f, sz * 0.28f, l.hair);
        Disc(x + sz * 0.43f, y + sz * 0.37f, sz * 0.05f, new Color(0.08f, 0.06f, 0.06f));
        Disc(x + sz * 0.55f, y + sz * 0.37f, sz * 0.05f, new Color(0.08f, 0.06f, 0.06f));
        if (l.headband) Flat(new Rect(x + sz * 0.32f, y + sz * 0.27f, sz * 0.38f, sz * 0.05f), l.accent);
        if (l.glasses) Flat(new Rect(x + sz * 0.4f, y + sz * 0.34f, sz * 0.26f, sz * 0.07f), new Color(0.05f, 0.05f, 0.08f));
        if (l.goggles)
        {
            Flat(new Rect(x + sz * 0.3f, y + sz * 0.33f, sz * 0.4f, sz * 0.05f), l.accent);
            Flat(new Rect(x + sz * 0.4f, y + sz * 0.32f, sz * 0.26f, sz * 0.1f), new Color(0.3f, 0.8f, 1f, 0.9f));
        }
    }

    void ShoeIcon(float x, float y, float w, ShopItem it)
    {
        var s = Race.GetShoe(it.id);
        float h = w * 0.62f;
        Round(new Rect(x, y, w, h), new Color(1f, 1f, 1f, 0.1f));
        float th = Mathf.Max(8f, s.thick * w * 1.7f);
        if (s.style == 4) Round(new Rect(x + w * 0.02f, y + h * 0.7f, w * 0.5f, h * 0.2f), new Color(s.accent.r, s.accent.g, s.accent.b, 0.4f));
        Round(new Rect(x + w * 0.08f, y + h * 0.78f - th / 2f, w * 0.84f, th), s.sole);
        Round(new Rect(x + w * 0.12f, y + h * 0.22f, w * 0.52f, h * 0.56f - th * 0.5f), s.upper);
        Round(new Rect(x + w * 0.46f, y + h * 0.44f, w * 0.42f, h * 0.34f - th * 0.4f), s.upper);
        if (s.style >= 1) Flat(new Rect(x + w * 0.22f, y + h * 0.5f, w * 0.38f, h * 0.06f), s.accent);
        if (s.style == 2) Disc(x + w * 0.3f, y + h * 0.78f - th * 0.35f, th * 0.7f, s.accent);
        if (s.style >= 3) Flat(new Rect(x + w * 0.16f, y + h * 0.78f - 2f, w * 0.72f, 3f), s.accent);
    }

    void TrackSwatch(float x, float y, float w, float h, int theme)
    {
        var c = Race.TrackColors(theme);
        Round(new Rect(x - 2f, y - 2f, w + 4f, h + 4f), new Color(1f, 1f, 1f, 0.5f));
        Flat(new Rect(x, y, w, h * 0.36f), c[0]);
        Flat(new Rect(x, y + h * 0.36f, w, h * 0.2f), c[1]);
        Flat(new Rect(x, y + h * 0.56f, w, h * 0.14f), c[2]);
        Flat(new Rect(x, y + h * 0.7f, w, h * 0.3f), c[3]);
        Flat(new Rect(x, y + h * 0.78f, w, 2f), new Color(1f, 1f, 1f, 0.7f));
    }

    void ItemIcon(int tab, ShopItem it, float x, float y, float sz, Color accent)
    {
        if (tab == 0) ShoeIcon(x - 4f, y + sz * 0.15f, sz * 1.15f, it);
        else if (tab == 1) Portrait(x, y, sz, it, accent);
        else TrackSwatch(x, y + sz * 0.05f, sz * 1.15f, sz * 0.8f, it.theme);
    }

    void Currency(float y)
    {
        Round(new Rect(720f, y, 240f, 56f), new Color(0.04f, 0.06f, 0.14f, 0.85f));
        Coin(732f, y + 5f, 46f);
        T(new Rect(784f, y, 166f, 56f), Store.D.coins.ToString(), 30, Gold, TextAnchor.MiddleRight, false);
        Round(new Rect(970f, y, 240f, 56f), new Color(0.04f, 0.06f, 0.14f, 0.85f));
        Gem(980f, y + 4f, 48f);
        T(new Rect(1034f, y, 166f, 56f), Store.D.ucoins.ToString(), 30, new Color(0.8f, 0.7f, 1f), TextAnchor.MiddleRight, false);
    }

    static float Frac(float v)
    {
        return v - Mathf.Floor(v);
    }

    void StarShape(float cx, float cy, float size, Color c)
    {
        Tint(new Color(0f, 0f, 0f, 0.35f));
        GUI.DrawTexture(new Rect(cx - size / 2f + 2f, cy - size / 2f + 4f, size, size), SpriteFactory.Star.texture);
        Tint(c);
        GUI.DrawTexture(new Rect(cx - size / 2f, cy - size / 2f, size, size), SpriteFactory.Star.texture);
        GUI.color = Color.white;
    }

    void Confetti()
    {
        if (stateT > 8f) return;
        for (int i = 0; i < 60; i++)
        {
            float h1 = Frac(i * 0.6180339f);
            float h2 = Frac(i * 0.7548777f);
            float h3 = Frac(i * 0.5698403f);
            float x = h1 * 1280f + Mathf.Sin(stateT * 2f + i) * 20f;
            float y = -20f + (stateT * (120f + h2 * 180f) + h3 * 900f) % 760f;
            var m = GUI.matrix;
            RotAt(x, y, stateT * 200f * h2 + i * 30f);
            Flat(new Rect(x, y, 11f, 6f), Color.HSVToRGB(h3, 0.8f, 1f));
            GUI.matrix = m;
        }
    }

    void OnGUI()
    {
        EnsureStyles();
        if (state != lastState)
        {
            lastState = state;
            stateT = 0f;
        }
        bool anim = state != S.Race && state != S.Paused;
        float k = Mathf.Clamp01(stateT / 0.3f);
        float ease = 1f - (1f - k) * (1f - k);
        fade = anim ? ease : 1f;
        float slide = anim ? (1f - ease) * 24f : 0f;

        float s = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        if (s < 0.2f || Screen.width < 160 || Screen.height < 90) return;
        float ox = (Screen.width - 1280f * s) / 2f;
        float oy = (Screen.height - 720f * s) / 2f;
        GUI.matrix = Matrix4x4.TRS(new Vector3(ox, oy + slide * s, 0f), Quaternion.identity, new Vector3(s, s, 1f));

        HandleInput(Event.current);

        switch (state)
        {
            case S.Menu: DrawMenu(); break;
            case S.Select: Stage(Orange, 0.9f); DrawSelect(); break;
            case S.Prep: Stage(Blue, 0.9f); DrawPrep(); break;
            case S.Race: DrawHud(); break;
            case S.Paused: DrawHud(); DrawPause(); break;
            case S.Result: Stage(Gold, 0.9f); DrawResult(); break;
            case S.Login: DrawLogin(); break;
            case S.Shop: DrawShop(); break;
            case S.Missions: DrawMissions(); break;
            case S.Profile: DrawProfile(); break;
            case S.Train: DrawTrain(); break;
        }

        if (toastT > 0f && !string.IsNullOrEmpty(toast))
        {
            Round(new Rect(400f, 640f, 480f, 56f), new Color(0.05f, 0.08f, 0.16f, 0.95f));
            T(new Rect(400f, 640f, 480f, 56f), toast, 24, Color.white, TextAnchor.MiddleCenter, false);
        }
    }

    void HandleInput(Event e)
    {
        if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Space) spaceHeld = false;
        if (e.type == EventType.KeyUp) heldKeys.Remove(e.keyCode);
        if (state == S.Race && race.mode == 1)
        {
            bool tap = false;
            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.Escape && e.keyCode != KeyCode.None && heldKeys.Add(e.keyCode)) tap = true;
            else if (e.type == EventType.MouseDown && e.button == 0 && !PauseRect.Contains(e.mousePosition)) tap = true;
            if (tap && race.countdown <= 0f && race.racing)
            {
                race.Tap();
                tapFlash = 1f;
                Sfx.Play(Sfx.Id.Step, 0.5f, 0.9f + Random.value * 0.3f);
            }
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) state = S.Paused;
        }
        else if (state == S.Race)
        {
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                if (!spaceHeld)
                {
                    spaceHeld = true;
                    race.TryJump();
                }
            }
            else if (e.type == EventType.MouseDown && e.button == 0 && !PauseRect.Contains(e.mousePosition))
            {
                race.TryJump();
            }
            else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                state = S.Paused;
            }
        }
        else if (state == S.Paused && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            state = S.Race;
        }
    }

    void DrawMenu()
    {
        var d = Store.D;
        int lv = Store.Level(d.xp);
        Backdrop(0.3f);
        PlayerChip();
        float bob = Mathf.Sin(Time.unscaledTime * 2f) * 6f;
        T(new Rect(0f, 30f + bob, 1280f, 110f), "RUN TO FINISH", 96, Color.white);
        Round(new Rect(420f, 148f, 440f, 44f), Orange);
        T(new Rect(420f, 148f, 440f, 44f), "EPIC PROGRESSION EDITION", 22, Color.white, TextAnchor.MiddleCenter, false);

        PanelBox(new Rect(235f, 212f, 195f, 86f), Orange);
        Badge(248f, 228f, 52f, Orange, lv.ToString(), 26);
        T(new Rect(308f, 224f, 120f, 24f), "LEVEL", 15, Gray, TextAnchor.MiddleLeft, false);
        T(new Rect(308f, 248f, 120f, 36f), Store.Title(lv).ToUpper(), 20, Color.white, TextAnchor.MiddleLeft, false);

        PanelBox(new Rect(442f, 212f, 195f, 86f), Gold);
        Coin(456f, 232f, 46f);
        T(new Rect(510f, 224f, 120f, 24f), "COINS", 15, Gray, TextAnchor.MiddleLeft, false);
        T(new Rect(510f, 248f, 120f, 36f), d.coins.ToString(), 26, Color.white, TextAnchor.MiddleLeft, false);

        PanelBox(new Rect(649f, 212f, 195f, 86f), Purple);
        Gem(661f, 230f, 50f);
        T(new Rect(716f, 224f, 120f, 24f), "KOIN UPGRADE", 13, Gray, TextAnchor.MiddleLeft, false);
        T(new Rect(716f, 248f, 120f, 36f), d.ucoins.ToString(), 26, Color.white, TextAnchor.MiddleLeft, false);

        PanelBox(new Rect(856f, 212f, 195f, 86f), Blue);
        T(new Rect(868f, 224f, 175f, 24f), "TOTAL JARAK", 15, Gray, TextAnchor.MiddleCenter, false);
        T(new Rect(868f, 248f, 175f, 36f), Fmt.Dist(d.totalMeters), 24, Color.white, TextAnchor.MiddleCenter, false);

        int lo = Store.XpFloor(lv);
        int hi = Store.XpFloor(lv + 1);
        Bar(new Rect(235f, 312f, 816f, 14f), (d.xp - lo) / (float)(hi - lo), new Color(0.4f, 0.85f, 1f));
        T(new Rect(235f, 326f, 816f, 22f), "XP " + d.xp + " / " + hi, 15, Gray, TextAnchor.MiddleCenter, false);

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 4f) * 0.02f;
        float w = 340f * pulse;
        if (Btn(new Rect(640f - w / 2f, 358f, w, 84f), "START", Orange, 40)) state = S.Select;
        int cl = Store.Claimable();
        if (Btn(new Rect(466f, 456f, 170f, 64f), "LATIHAN", Purple, 24)) state = S.Train;
        if (Btn(new Rect(644f, 456f, 170f, 64f), "SHOP", Slate, 24)) { shopTab = 0; state = S.Shop; }
        if (Btn(new Rect(466f, 532f, 170f, 64f), cl > 0 ? "MISI (" + cl + ")" : "MISI", cl > 0 ? Green : Slate, 24)) state = S.Missions;
        if (Btn(new Rect(644f, 532f, 170f, 64f), "PROFIL", Slate, 24)) state = S.Profile;
        if (Btn(new Rect(1060f, 22f, 200f, 52f), Sfx.Muted ? "SUARA: OFF" : "SUARA: ON", Sfx.Muted ? Slate : Green, 20)) Sfx.Toggle();
        T(new Rect(0f, 664f, 1280f, 30f), "Tap / Spasi untuk lompat  |  Tap lagi di udara untuk double jump", 18, new Color(1f, 1f, 1f, 0.85f), TextAnchor.MiddleCenter, true);
    }

    void DrawSelect()
    {
        PanelBox(new Rect(40f, 20f, 1200f, 680f), Orange);
        T(new Rect(0f, 28f, 1280f, 56f), "PILIH MODE, JARAK & KESULITAN", 38, Color.white);
        T(new Rect(70f, 88f, 400f, 28f), "MODE", 20, Orange, TextAnchor.MiddleLeft, false);
        for (int i = 0; i < 3; i++)
        {
            var r = new Rect(70f + i * 387f, 116f, 366f, 100f);
            if (i == selMode) Round(new Rect(r.x - 5f, r.y - 5f, r.width + 10f, r.height + 10f), new Color(1f, 1f, 1f, 0.95f));
            if (Btn(r, "", ModeColors[i], 20)) selMode = i;
            T(new Rect(r.x, r.y + 8f, r.width, 40f), ModeNames[i], 32, Color.white);
            T(new Rect(r.x + 8f, r.y + 48f, r.width - 16f, 26f), ModeInfo[i], 15, new Color(1f, 1f, 1f, 0.92f), TextAnchor.MiddleCenter, false);
            T(new Rect(r.x, r.y + 72f, r.width, 22f), "Hadiah x" + ModeMult[i].ToString("0.0"), 15, Gold, TextAnchor.MiddleCenter, false);
        }

        T(new Rect(70f, 232f, 400f, 28f), "JARAK", 20, Orange, TextAnchor.MiddleLeft, false);
        for (int i = 0; i < Targets.Length; i++)
        {
            int row = i / 4;
            int col = i % 4;
            var r = new Rect(70f + col * 290f, 262f + row * 74f, 270f, 62f);
            if (i == selTarget) Round(new Rect(r.x - 5f, r.y - 5f, r.width + 10f, r.height + 10f), new Color(1f, 1f, 1f, 0.95f));
            if (Btn(r, TargetNames[i], i == selTarget ? Orange : Slate, 30)) selTarget = i;
        }

        T(new Rect(70f, 410f, 400f, 28f), "KESULITAN", 20, Orange, TextAnchor.MiddleLeft, false);
        string[] info = selMode == 1 ? TapInfo : (selMode == 2 ? HurdleInfo : DiffInfo);
        for (int i = 0; i < 4; i++)
        {
            var dd = (Difficulty)i;
            bool ok = Store.Unlocked(dd);
            var r = new Rect(70f + i * 290f, 440f, 270f, 140f);
            if (dd == selDiff) Round(new Rect(r.x - 5f, r.y - 5f, r.width + 10f, r.height + 10f), new Color(1f, 1f, 1f, 0.95f));
            if (Btn(r, "", DiffColors[i], 20, ok)) selDiff = dd;
            T(new Rect(r.x, r.y + 8f, r.width, 40f), DiffNames[i], 32, Color.white);
            T(new Rect(r.x, r.y + 50f, r.width, 28f), "Coins/XP x" + Store.DiffMult(dd).ToString("0.00"), 20, Color.white);
            T(new Rect(r.x, r.y + 82f, r.width, 26f), info[i], 17, new Color(1f, 1f, 1f, 0.9f), TextAnchor.MiddleCenter, false);
            if (!ok) T(new Rect(r.x, r.y + 110f, r.width, 24f), "TERKUNCI: " + DiffLock[i], 14, Gold, TextAnchor.MiddleCenter, false);
        }

        if (Btn(new Rect(70f, 606f, 220f, 68f), "KEMBALI", Slate, 28)) state = S.Menu;
        if (Btn(new Rect(990f, 606f, 220f, 68f), "LANJUT", Orange, 28)) state = S.Prep;
    }

    void CycleItem(List<ShopItem> list, string cur, int dir, int kind)
    {
        var owned = new List<ShopItem>();
        foreach (var it in list) if (Store.D.owned.Contains(it.id)) owned.Add(it);
        if (owned.Count == 0) return;
        int idx = owned.FindIndex(x => x.id == cur);
        if (idx < 0) idx = 0;
        idx = (idx + dir + owned.Count) % owned.Count;
        string id = owned[idx].id;
        if (kind == 0) Store.D.shoe = id;
        else if (kind == 1) Store.D.runner = id;
        else
        {
            Store.D.track = id;
            race.SetTrack(owned[idx].theme);
        }
        if (kind != 2) race.ApplyLook(RunnerItem, Shoe, false);
        Store.Save();
    }

    void BuffRow(float y, string label, float value, Color c, float max = 0.6f)
    {
        T(new Rect(826f, y, 140f, 30f), label, 22, Color.white, TextAnchor.MiddleLeft, false);
        Bar(new Rect(960f, y + 7f, 170f, 16f), value / max, c);
        T(new Rect(1138f, y, 70f, 30f), "+" + Mathf.RoundToInt(value * 100f) + "%", 22, Color.white, TextAnchor.MiddleRight, false);
    }

    void DrawPrep()
    {
        T(new Rect(0f, 28f, 1280f, 60f), "PERSIAPAN BALAPAN", 42, Color.white);
        Round(new Rect(500f, 92f, 280f, 40f), DiffColors[(int)selDiff]);
        T(new Rect(500f, 92f, 280f, 40f), TargetNames[selTarget] + "  |  " + DiffNames[(int)selDiff], 22, Color.white, TextAnchor.MiddleCenter, false);

        string[] cats = { "SEPATU", "PELARI", "TRACK" };
        Color[] accents = { Orange, Blue, Green };
        ShopItem[] items = { Shoe, RunnerItem, Track };
        int[] tabOf = { 0, 1, 2 };
        for (int i = 0; i < 3; i++)
        {
            float y = 150f + i * 150f;
            Card(new Rect(60f, y, 720f, 136f), accents[i], true);
            ItemIcon(tabOf[i], items[i], 94f, y + 26f, 84f, accents[i]);
            T(new Rect(204f, y + 12f, 300f, 26f), cats[i], 18, accents[i], TextAnchor.MiddleLeft, false);
            T(new Rect(204f, y + 40f, 400f, 44f), items[i].name, 32, Color.white, TextAnchor.MiddleLeft, false);
            T(new Rect(204f, y + 88f, 400f, 28f), items[i].Desc(), 20, Gray, TextAnchor.MiddleLeft, false);
            var list = i == 0 ? Catalog.Shoes : i == 1 ? Catalog.Runners : Catalog.Tracks;
            string cur = i == 0 ? Store.D.shoe : i == 1 ? Store.D.runner : Store.D.track;
            if (Btn(new Rect(620f, y + 34f, 66f, 68f), "<", Slate, 34)) CycleItem(list, cur, -1, i);
            if (Btn(new Rect(696f, y + 34f, 66f, 68f), ">", Slate, 34)) CycleItem(list, cur, 1, i);
        }

        var sh = Shoe;
        var ru = RunnerItem;
        var tr = Track;
        PanelBox(new Rect(800f, 150f, 420f, 436f), Orange);
        T(new Rect(820f, 158f, 380f, 34f), "BUFF TOTAL", 24, Orange, TextAnchor.MiddleCenter, false);
        BuffRow(198f, "Speed", sh.speed + ru.speed + UpBonus(0), Orange, 0.6f);
        BuffRow(234f, "Jump", sh.jump + ru.jump + UpBonus(1), Blue, 0.5f);
        BuffRow(270f, "Stamina", sh.stamina + ru.stamina + UpBonus(2), Green, 0.8f);
        BuffRow(306f, "Pulih", UpBonus(3), Purple, 0.6f);
        BuffRow(342f, "Track", tr.reward, Gold, 1.0f);

        int coins, xp, uc;
        Reward(out coins, out xp, out uc);
        Round(new Rect(820f, 384f, 380f, 2f), new Color(1f, 1f, 1f, 0.2f));
        T(new Rect(820f, 390f, 380f, 30f), "ESTIMASI HADIAH  x" + (Store.DiffMult(selDiff) * (1f + tr.reward)).ToString("0.00"), 20, Orange, TextAnchor.MiddleCenter, false);
        Coin(850f, 430f, 40f);
        T(new Rect(906f, 426f, 280f, 48f), coins + " COINS", 30, Gold, TextAnchor.MiddleLeft, false);
        Badge(852f, 480f, 36f, Blue, "XP", 15);
        T(new Rect(906f, 474f, 280f, 48f), xp + " XP", 30, new Color(0.45f, 0.85f, 1f), TextAnchor.MiddleLeft, false);
        Gem(848f, 524f, 44f);
        T(new Rect(906f, 522f, 280f, 48f), uc + " KOIN UPGRADE", 26, new Color(0.8f, 0.7f, 1f), TextAnchor.MiddleLeft, false);

        if (Btn(new Rect(60f, 612f, 220f, 68f), "KEMBALI", Slate, 28)) state = S.Select;
        if (Btn(new Rect(900f, 604f, 320f, 80f), "MULAI LARI", Orange, 36)) BeginRace();
    }

    void Pill(float x, float w, string label, string value)
    {
        Round(new Rect(x, 10f, w, 58f), new Color(0.04f, 0.06f, 0.14f, 0.66f));
        T(new Rect(x, 12f, w, 20f), label, 14, Gray, TextAnchor.MiddleCenter, false);
        T(new Rect(x, 30f, w, 36f), value, 28, Color.white, TextAnchor.MiddleCenter, false);
    }

    void DrawHud()
    {
        float m = race.meters;
        Pill(16f, 220f, "WAKTU", Fmt.Time(race.virtTime));
        Pill(250f, 220f, "JARAK", Fmt.Dist(m));
        Pill(484f, 240f, "PACE", Fmt.Pace(race.virtTime, m));
        Pill(738f, 240f, "KECEPATAN", (race.speedMS * 3.6f).ToString("0.0") + " KM/H");
        int di = (int)race.diff;
        Round(new Rect(992f, 10f, 170f, 58f), DiffColors[di]);
        T(new Rect(992f, 10f, 170f, 58f), DiffNames[di], 26, Color.white, TextAnchor.MiddleCenter, false);
        if (Btn(PauseRect, "II", Slate, 30) && state == S.Race) state = S.Paused;

        var bar = new Rect(60f, 84f, 1160f, 16f);
        Bar(bar, race.Progress, Orange);
        float mx = bar.x + (bar.width - 28f) * race.Progress;
        foreach (var rv in race.rivals)
        {
            float rx = bar.x + (bar.width - 28f) * Mathf.Clamp01(rv.meters / Mathf.Max(1f, race.target));
            Disc(rx + 2f, 84f - 2f, 22f, Color.white);
            Disc(rx + 4f, 84f, 18f, rv.color);
        }
        Disc(mx - 3f, 84f - 7f, 34f, Color.white);
        Disc(mx, 84f - 4f, 28f, Orange);
        T(new Rect(60f, 104f, 100f, 20f), "START", 13, new Color(1f, 1f, 1f, 0.8f), TextAnchor.MiddleLeft, false);
        T(new Rect(1120f, 104f, 100f, 20f), "FINISH", 13, new Color(1f, 1f, 1f, 0.8f), TextAnchor.MiddleRight, false);
        T(new Rect(540f, 102f, 200f, 24f), Mathf.RoundToInt(race.Progress * 100f) + "%", 18, Color.white, TextAnchor.MiddleCenter, true);

        float st = race.stamina / 100f;
        Color sc = st > 0.5f ? Color.Lerp(Gold, Green, (st - 0.5f) * 2f) : Color.Lerp(DiffColors[2], Gold, st * 2f);
        Round(new Rect(24f, 636f, 340f, 62f), new Color(0.04f, 0.06f, 0.14f, 0.66f));
        T(new Rect(40f, 640f, 200f, 22f), "STAMINA", 15, Gray, TextAnchor.MiddleLeft, false);
        T(new Rect(240f, 640f, 110f, 22f), Mathf.RoundToInt(race.stamina) + "%", 15, Color.white, TextAnchor.MiddleRight, false);
        Bar(new Rect(40f, 666f, 308f, 20f), st, sc);

        if (race.mode == 2) T(new Rect(250f, 102f, 300f, 24f), "HURDLE LEWAT: " + race.hurdlesPassed, 18, Color.white, TextAnchor.MiddleLeft, true);
        else if (race.mode == 1) T(new Rect(250f, 102f, 300f, 24f), "LOMBA LARI", 18, Color.white, TextAnchor.MiddleLeft, true);
        if (race.mode > 0) DrawRanking();
        if (race.mode == 1) DrawTapHud();

        if (race.stumbleT > 0f)
        {
            float sh = Mathf.Sin(Time.unscaledTime * 50f) * 4f;
            float ra = Mathf.Clamp01(race.stumbleT / 0.6f);
            FadeDown(new Rect(0f, 0f, 1280f, 220f), new Color(1f, 0.1f, 0.1f, 0.45f * ra));
            FadeUp(new Rect(0f, 500f, 1280f, 220f), new Color(1f, 0.1f, 0.1f, 0.45f * ra));
            T(new Rect(sh, 140f, 1280f, 56f), "TERSANDUNG!", 40, new Color(1f, 0.45f, 0.4f));
        }

        if (race.countdown > 0f)
        {
            float f = Frac(race.countdown);
            if (f < 0.001f) f = 1f;
            int size = Mathf.RoundToInt(110f + 90f * f);
            T(new Rect(0f, 200f, 1280f, 260f), Mathf.CeilToInt(race.countdown).ToString(), size, new Color(1f, 1f, 1f, 0.35f + 0.65f * f));
        }
        else if (race.realTime < 0.8f && race.racing)
        {
            int size = Mathf.RoundToInt(150f + 60f * race.realTime);
            T(new Rect(0f, 200f, 1280f, 260f), "GO!", size, new Color(Orange.r, Orange.g, Orange.b, 1f - race.realTime / 0.8f));
        }
        else if (race.realTime < 4f && race.racing)
        {
            Round(new Rect(300f, 560f, 680f, 44f), new Color(0.04f, 0.06f, 0.14f, 0.6f));
            string hint = race.mode == 1 ? "TAP LAYAR / TEKAN TOMBOL SECEPAT MUNGKIN!" : "TAP / SPASI = LOMPAT   |   TAP LAGI DI UDARA = DOUBLE JUMP";
            T(new Rect(300f, 560f, 680f, 44f), hint, 18, Color.white, TextAnchor.MiddleCenter, false);
        }
    }

    void DrawRanking()
    {
        int n = race.rivals.Count + 1;
        var names = new string[n];
        var vals = new float[n];
        var cols = new Color[n];
        names[0] = "KAMU";
        vals[0] = race.meters;
        cols[0] = Orange;
        for (int i = 0; i < race.rivals.Count; i++)
        {
            names[i + 1] = race.rivals[i].name;
            vals[i + 1] = race.rivals[i].meters;
            cols[i + 1] = race.rivals[i].color;
        }
        var order = new int[n];
        for (int i = 0; i < n; i++) order[i] = i;
        for (int i = 1; i < n; i++)
        {
            int k = order[i];
            int j = i - 1;
            while (j >= 0 && vals[order[j]] < vals[k])
            {
                order[j + 1] = order[j];
                j--;
            }
            order[j + 1] = k;
        }
        float px = 1040f;
        float py = 136f;
        Round(new Rect(px, py, 224f, 34f + n * 36f), new Color(0.03f, 0.04f, 0.1f, 0.78f));
        T(new Rect(px + 12f, py + 2f, 200f, 30f), "POSISI", 16, Gray, TextAnchor.MiddleLeft, false);
        for (int i = 0; i < n; i++)
        {
            int id = order[i];
            float ry = py + 34f + i * 36f;
            if (id == 0) Round(new Rect(px + 6f, ry, 212f, 32f), new Color(1f, 0.55f, 0.08f, 0.35f));
            T(new Rect(px + 10f, ry, 24f, 32f), (i + 1).ToString(), 20, Color.white, TextAnchor.MiddleCenter, false);
            Disc(px + 38f, ry + 8f, 16f, cols[id]);
            T(new Rect(px + 60f, ry, 100f, 32f), names[id], 15, Color.white, TextAnchor.MiddleLeft, false);
            if (id != 0)
            {
                float dgap = vals[id] - race.meters;
                T(new Rect(px + 140f, ry, 72f, 32f), (dgap >= 0f ? "+" : "-") + Mathf.Abs(dgap).ToString("0") + "M", 15, dgap >= 0f ? new Color(1f, 0.55f, 0.55f) : new Color(0.55f, 1f, 0.65f), TextAnchor.MiddleRight, false);
            }
        }
    }

    void DrawTapHud()
    {
        float eff = Mathf.Clamp01(race.Effort);
        float pulse = 1f + Mathf.Max(0f, tapFlash) * 0.18f;
        float sz = 120f * pulse;
        float cx = 1130f;
        float cy = 570f;
        Glow(cx, cy, 120f * pulse, Alpha(Green, 0.25f + 0.4f * Mathf.Max(0f, tapFlash)));
        Disc(cx - sz / 2f - 4f, cy - sz / 2f - 4f, sz + 8f, new Color(1f, 1f, 1f, 0.9f));
        Disc(cx - sz / 2f, cy - sz / 2f, sz, Color.Lerp(Shade(Green, 0.7f), Green, Mathf.Max(0f, tapFlash)));
        T(new Rect(cx - 70f, cy - 30f, 140f, 60f), "TAP!", 40, Color.white);

        PanelBox(new Rect(400f, 616f, 480f, 82f), Green);
        T(new Rect(416f, 620f, 250f, 28f), "TAP / DETIK", 16, Gray, TextAnchor.MiddleLeft, false);
        T(new Rect(640f, 620f, 224f, 28f), race.tapRate.ToString("0") + " / " + race.TapRef.ToString("0"), 22, Color.white, TextAnchor.MiddleRight, false);
        Color ec = eff > 0.85f ? Green : (eff > 0.5f ? Gold : DiffColors[2]);
        Bar(new Rect(416f, 656f, 448f, 24f), eff, ec);
    }

    void DrawPause()
    {
        Flat(new Rect(0f, 0f, 1280f, 720f), new Color(0f, 0.02f, 0.08f, 0.65f));
        PanelBox(new Rect(440f, 120f, 400f, 470f), Orange);
        T(new Rect(440f, 140f, 400f, 80f), "PAUSE", 62, Color.white);
        if (Btn(new Rect(480f, 250f, 320f, 76f), "RESUME", Orange, 32)) state = S.Race;
        if (Btn(new Rect(480f, 346f, 320f, 70f), "RESTART", Slate, 30)) BeginRace();
        if (Btn(new Rect(480f, 436f, 320f, 70f), "MENU", Slate, 30)) ToMenu();
        if (Btn(new Rect(480f, 516f, 320f, 56f), Sfx.Muted ? "SUARA: OFF" : "SUARA: ON", Sfx.Muted ? Slate : Green, 24)) Sfx.Toggle();
    }

    void DrawResult()
    {
        PanelBox(new Rect(290f, 20f, 700f, 680f), Gold);
        float pop = Mathf.Clamp01(stateT / 0.4f);
        string title = rMode == 0 ? "FINISH!" : (rRank == 1 ? "JUARA 1!" : "POSISI " + rRank + " / " + rTotal);
        T(new Rect(290f, 30f, 700f, 80f), title, Mathf.RoundToInt(48f + 20f * pop), rRank == 1 || rMode == 0 ? Gold : Color.white);
        if (rPB)
        {
            Round(new Rect(420f, 108f, 440f, 40f), Gold);
            T(new Rect(420f, 108f, 440f, 40f), "NEW PERSONAL BEST!", 24, new Color(0.25f, 0.15f, 0f), TextAnchor.MiddleCenter, false);
        }

        int stars = rMode == 1 ? (rRatio >= 0.9f ? 3 : (rRatio >= 0.75f ? 2 : 1)) : (rHits == 0 ? 3 : (rHits <= 2 ? 2 : 1));
        for (int i = 0; i < 3; i++)
        {
            float t = Mathf.Clamp01((stateT - 0.3f - i * 0.25f) / 0.25f);
            float full = i == 1 ? 84f : 70f;
            if (i < stars && stateT > 0.45f + i * 0.25f && starPlayed <= i)
            {
                starPlayed = i + 1;
                Sfx.Play(Sfx.Id.Star, 0.8f, 1f + i * 0.2f);
            }
            Color c = i < stars ? Gold : new Color(0.3f, 0.34f, 0.44f);
            StarShape(550f + i * 90f, i == 1 ? 188f : 196f, i < stars ? full * (0.2f + 0.8f * t) : full, c);
        }

        string[] k = { "Target", "Jarak", "Waktu", "Pace", "Rata-rata", "Kesulitan" };
        string[] v =
        {
            TargetNames[selTarget] + "  " + ModeNames[rMode],
            Fmt.Dist(rMeters),
            Fmt.Time(rTime),
            Fmt.Pace(rTime, rMeters),
            (rMeters / Mathf.Max(0.1f, rTime) * 3.6f).ToString("0.0") + " KM/H",
            rMode == 1 ? rTapAvg.ToString("0.0") + " TAP/DETIK" : (rMode == 2 ? rPassed + " HURDLE / " + rHits + " TABRAK" : DiffNames[(int)selDiff])
        };
        k[5] = rMode == 1 ? "Rata-rata Tap" : (rMode == 2 ? "Hurdle" : "Kesulitan");
        for (int i = 0; i < k.Length; i++)
        {
            float y = 250f + i * 42f;
            if (i % 2 == 0) Round(new Rect(330f, y, 620f, 40f), new Color(1f, 1f, 1f, 0.07f));
            T(new Rect(346f, y, 260f, 40f), k[i], 24, Gray, TextAnchor.MiddleLeft, false);
            T(new Rect(600f, y, 336f, 40f), v[i], 26, i == 5 && rMode == 0 ? DiffColors[(int)selDiff] : Color.white, TextAnchor.MiddleRight, false);
        }

        float cu = Mathf.Clamp01((stateT - 0.9f) / 1.2f);
        Round(new Rect(330f, 508f, 620f, 1f), new Color(1f, 1f, 1f, 0.18f));
        Chip(330f, 520f, 196f, 52f, 0, "+" + Mathf.RoundToInt(rCoins * cu), Gold);
        Chip(542f, 520f, 196f, 52f, 1, "+" + Mathf.RoundToInt(rXp * cu), new Color(0.45f, 0.85f, 1f));
        Chip(754f, 520f, 196f, 52f, 2, "+" + Mathf.RoundToInt(rUc * cu), new Color(0.8f, 0.7f, 1f));
        if (rLvAfter > rLvBefore)
        {
            Round(new Rect(420f, 580f, 440f, 32f), Green);
            T(new Rect(420f, 580f, 440f, 32f), "LEVEL UP!  " + rLvBefore + "  >  " + rLvAfter, 20, Color.white, TextAnchor.MiddleCenter, false);
        }

        if (Btn(new Rect(330f, 622f, 300f, 66f), "PLAY AGAIN", Orange, 28)) BeginRace();
        if (Btn(new Rect(650f, 622f, 300f, 66f), "MENU", Slate, 28)) ToMenu();

        if (stars >= 2 || rPB) Confetti();
    }

    void Equip(ShopItem it)
    {
        if (shopTab == 0) Store.D.shoe = it.id;
        else if (shopTab == 1) Store.D.runner = it.id;
        else
        {
            Store.D.track = it.id;
            race.SetTrack(it.theme);
        }
        if (shopTab != 2) race.ApplyLook(RunnerItem, Shoe, false);
        Store.Save();
    }

    string MissionProgress(MissionData m)
    {
        if (m.type == "dist") return (m.progress / 1000f).ToString("0.0") + " / " + (m.target / 1000f).ToString("0.#") + " KM";
        return Mathf.RoundToInt(m.progress) + " / " + m.target;
    }
}

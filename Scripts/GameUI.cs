using System.Collections.Generic;
using UnityEngine;

public partial class Game
{
    int downArm = -1;
    float downArmT;
    Texture2D fxV;
    Texture2D fxH;
    Texture2D fxR;
    GUIStyle fieldStyle;
    string nameInput = "";
    bool editingName;
    bool focusField;

    static Color Bright(Color c)
    {
        return Color.Lerp(c, Color.white, 0.4f);
    }

    static Color Alpha(Color c, float a)
    {
        return new Color(c.r, c.g, c.b, a);
    }

    void EnsureFx()
    {
        if (fxV != null) return;
        fxV = new Texture2D(1, 64, TextureFormat.RGBA32, false);
        for (int y = 0; y < 64; y++) fxV.SetPixel(0, y, new Color(1f, 1f, 1f, y / 63f));
        fxV.wrapMode = TextureWrapMode.Clamp;
        fxV.filterMode = FilterMode.Bilinear;
        fxV.Apply();
        fxH = new Texture2D(64, 1, TextureFormat.RGBA32, false);
        for (int x = 0; x < 64; x++) fxH.SetPixel(x, 0, new Color(1f, 1f, 1f, 1f - x / 63f));
        fxH.wrapMode = TextureWrapMode.Clamp;
        fxH.filterMode = FilterMode.Bilinear;
        fxH.Apply();
        fxR = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(64f, 64f)) / 64f;
                float a = Mathf.Clamp01(1f - d);
                fxR.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
        }
        fxR.wrapMode = TextureWrapMode.Clamp;
        fxR.filterMode = FilterMode.Bilinear;
        fxR.Apply();
    }

    void FadeDown(Rect r, Color c)
    {
        EnsureFx();
        Tint(c);
        GUI.DrawTextureWithTexCoords(r, fxV, new Rect(0f, 0f, 1f, 1f));
        GUI.color = Color.white;
    }

    void FadeUp(Rect r, Color c)
    {
        EnsureFx();
        Tint(c);
        GUI.DrawTextureWithTexCoords(r, fxV, new Rect(0f, 1f, 1f, -1f));
        GUI.color = Color.white;
    }

    void FadeRight(Rect r, Color c)
    {
        EnsureFx();
        Tint(c);
        GUI.DrawTextureWithTexCoords(r, fxH, new Rect(0f, 0f, 1f, 1f));
        GUI.color = Color.white;
    }

    void Glow(float cx, float cy, float rad, Color c)
    {
        EnsureFx();
        Tint(c);
        GUI.DrawTexture(new Rect(cx - rad, cy - rad, rad * 2f, rad * 2f), fxR);
        GUI.color = Color.white;
    }

    void Stage(Color accent, float dim)
    {
        EnsureFx();
        Flat(new Rect(0f, 0f, 1280f, 720f), new Color(0.012f, 0.018f, 0.055f, dim * fade));
        Glow(1060f, 90f, 560f, Alpha(accent, 0.34f));
        Glow(120f, 660f, 480f, new Color(0.25f, 0.2f, 0.85f, 0.26f));
        GUI.BeginGroup(new Rect(0f, 0f, 1280f, 720f));
        for (int i = 0; i < 5; i++)
        {
            float x = Frac(Time.unscaledTime * 0.04f + i * 0.2f) * 1900f - 360f;
            var m = GUI.matrix;
            RotAt(x + 60f, 360f, 20f);
            Flat(new Rect(x, -300f, 110f + i * 18f, 1320f), new Color(1f, 1f, 1f, 0.022f));
            GUI.matrix = m;
        }
        GUI.EndGroup();
        Sparkles();
        FadeDown(new Rect(0f, 0f, 1280f, 150f), new Color(0f, 0f, 0f, 0.55f));
        FadeUp(new Rect(0f, 570f, 1280f, 150f), new Color(0f, 0f, 0f, 0.55f));
    }

    void PanelBox(Rect r, Color accent)
    {
        Round(new Rect(r.x + 2f, r.y + 8f, r.width, r.height), new Color(0f, 0f, 0f, 0.4f));
        Round(new Rect(r.x - 1.5f, r.y - 1.5f, r.width + 3f, r.height + 3f), Alpha(accent, 0.38f));
        Round(r, new Color(0.04f, 0.06f, 0.13f, 0.96f));
        FadeDown(new Rect(r.x + 8f, r.y + 4f, r.width - 16f, Mathf.Min(80f, r.height * 0.45f)), new Color(1f, 1f, 1f, 0.07f));
        Flat(new Rect(r.x + 24f, r.y, r.width - 48f, 3f), accent);
        float L = 16f;
        float t = 3f;
        Color c = Bright(accent);
        Flat(new Rect(r.x - 4f, r.y - 4f, L, t), c);
        Flat(new Rect(r.x - 4f, r.y - 4f, t, L), c);
        Flat(new Rect(r.xMax + 4f - L, r.y - 4f, L, t), c);
        Flat(new Rect(r.xMax + 4f - t, r.y - 4f, t, L), c);
        Flat(new Rect(r.x - 4f, r.yMax + 4f - t, L, t), c);
        Flat(new Rect(r.x - 4f, r.yMax + 4f - L, t, L), c);
        Flat(new Rect(r.xMax + 4f - L, r.yMax + 4f - t, L, t), c);
        Flat(new Rect(r.xMax + 4f - t, r.yMax + 4f - L, t, L), c);
    }

    bool Hit(Rect r, bool on, out bool hover, out bool down)
    {
        int id = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        hover = r.Contains(e.mousePosition);
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
        down = on && GUIUtility.hotControl == id && hover;
        return clicked;
    }

    bool Btn(Rect r, string s, Color c, int size, bool on = true)
    {
        bool hover;
        bool down;
        bool clicked = Hit(r, on, out hover, out down);
        if (Event.current.type != EventType.Repaint) return clicked;
        Color face = on ? c : new Color(0.24f, 0.26f, 0.33f);
        float dy = down ? 2f : 0f;
        var f = new Rect(r.x, r.y + dy, r.width, r.height);
        if (on && hover) Glow(f.center.x, f.center.y, Mathf.Max(f.width, f.height) * 0.75f, Alpha(c, 0.35f));
        Round(new Rect(f.x - 2f, f.y - 2f, f.width + 4f, f.height + 4f), on ? Alpha(Bright(c), 0.9f) : new Color(0.4f, 0.42f, 0.5f, 0.6f));
        Round(f, Shade(face, 0.55f));
        Round(new Rect(f.x, f.y, f.width, f.height - 4f), face);
        FadeDown(new Rect(f.x + 6f, f.y + 3f, f.width - 12f, f.height * 0.45f), new Color(1f, 1f, 1f, on ? (hover ? 0.38f : 0.26f) : 0.08f));
        if (!string.IsNullOrEmpty(s)) T(new Rect(f.x, f.y - 2f, f.width, f.height - 2f), s, size, on ? Color.white : new Color(0.62f, 0.65f, 0.72f));
        return clicked;
    }

    bool Header(string title, string sub, Color accent)
    {
        Flat(new Rect(0f, 0f, 1280f, 98f), new Color(0.008f, 0.012f, 0.04f, 0.94f));
        FadeRight(new Rect(0f, 0f, 860f, 98f), Alpha(accent, 0.3f));
        Flat(new Rect(0f, 96f, 1280f, 2f), accent);
        FadeDown(new Rect(0f, 98f, 1280f, 18f), Alpha(accent, 0.3f));
        bool back = Btn(new Rect(24f, 18f, 70f, 60f), "<", Slate, 38);
        T(new Rect(114f, 8f, 600f, 58f), title, 50, Color.white, TextAnchor.MiddleLeft);
        if (!string.IsNullOrEmpty(sub)) T(new Rect(116f, 64f, 600f, 26f), sub, 17, Bright(accent), TextAnchor.MiddleLeft, false);
        Pill(826f, 22f, 200f, 54f, 0, Store.D.coins.ToString(), Gold);
        Pill(1042f, 22f, 200f, 54f, 1, Store.D.ucoins.ToString(), new Color(0.8f, 0.7f, 1f));
        return back;
    }

    void Pill(float x, float y, float w, float h, int kind, string txt, Color c)
    {
        Round(new Rect(x - 1.5f, y - 1.5f, w + 3f, h + 3f), Alpha(c, 0.45f));
        Round(new Rect(x, y, w, h), new Color(0.03f, 0.04f, 0.1f, 0.96f));
        FadeRight(new Rect(x + 6f, y + 4f, w * 0.6f, h - 8f), Alpha(c, 0.18f));
        if (kind == 0) Coin(x + 8f, y + 6f, h - 12f);
        else Gem(x + 4f, y + 2f, h - 4f);
        T(new Rect(x + h, y, w - h - 14f, h), txt, 28, c, TextAnchor.MiddleRight, false);
    }

    Color Rarity(int price)
    {
        if (price <= 0)    return new Color(0.55f, 0.62f, 0.75f); // Common  – abu
        if (price < 1000)  return new Color(0.25f, 0.6f,  1f);    // Rare    – biru
        if (price < 2000)  return new Color(0.7f,  0.4f,  1f);    // Epic    – ungu
        if (price < 5000)  return new Color(1f,    0.72f, 0.12f); // Legend  – emas
        if (price < 8000)  return new Color(0.15f, 0.90f, 1f);    // Super   – cyan
        return new Color(1f, 0.25f, 0.60f);                        // Elite   – merah-pink
    }

    string RarityName(int price)
    {
        if (price <= 0)   return "COMMON";
        if (price < 1000) return "RARE";
        if (price < 2000) return "EPIC";
        if (price < 5000) return "LEGEND";
        if (price < 8000) return "SUPER";
        return "ELITE";
    }

    void StatRow(float x, float y, float w, string label, float v, float max, Color c)
    {
        T(new Rect(x, y - 4f, 46f, 22f), label, 14, Gray, TextAnchor.MiddleLeft, false);
        Bar(new Rect(x + 48f, y, w - 48f - 56f, 14f), v / max, c);
        T(new Rect(x + w - 54f, y - 4f, 54f, 22f), v > 0f ? "+" + Mathf.RoundToInt(v * 100f) + "%" : "-", 15, v > 0f ? Color.white : Gray, TextAnchor.MiddleRight, false);
    }

    void ShopCard(int tab, ShopItem it, float x, float y, float w, float h, string cur, Color tabColor)
    {
        bool owned = Store.D.owned.Contains(it.id);
        bool equipped = it.id == cur;
        Color rc = Rarity(it.price);
        PanelBox(new Rect(x, y, w, h), equipped ? Green : rc);
        FadeDown(new Rect(x + 6f, y + 6f, w - 12f, 170f), Alpha(rc, 0.28f));
        Glow(x + w / 2f, y + 100f, 120f, Alpha(rc, 0.5f));
        if (tab == 0) ShoeIcon(x + w / 2f - 82f, y + 52f, 164f, it);
        else if (tab == 1) Portrait(x + w / 2f - 62f, y + 38f, 124f, it, Shade(rc, 0.55f));
        else TrackSwatch(x + 18f, y + 48f, w - 36f, 112f, it.theme);
        Round(new Rect(x + 14f, y + 16f, 78f, 22f), Shade(rc, 0.8f));
        T(new Rect(x + 14f, y + 16f, 78f, 22f), RarityName(it.price), 13, Color.white, TextAnchor.MiddleCenter, false);
        if (equipped)
        {
            Round(new Rect(x + w - 78f, y + 16f, 64f, 22f), Green);
            T(new Rect(x + w - 78f, y + 16f, 64f, 22f), "AKTIF", 13, Color.white, TextAnchor.MiddleCenter, false);
        }
        T(new Rect(x + 8f, y + 176f, w - 16f, 32f), it.name.ToUpper(), 22, Color.white, TextAnchor.MiddleCenter, false);
        Flat(new Rect(x + 24f, y + 210f, w - 48f, 1f), new Color(1f, 1f, 1f, 0.15f));

        bool met = Store.MeetsReq(it);
        if (tab < 2)
        {
            if (!met && (it.req5 > 0f || it.req10 > 0f))
            {
                T(new Rect(x + 8f, y + 218f, w - 16f, 22f), it.Desc(), 13, Gray, TextAnchor.MiddleCenter, false);
                float b5 = Store.BestAny(5000);
                float b10 = Store.BestAny(10000);
                bool ok5 = b5 > 0f && b5 < it.req5;
                bool ok10 = b10 > 0f && b10 < it.req10;
                ReqLine(x + 16f, y + 244f, w - 32f, "5K", it.req5, b5, ok5);
                ReqLine(x + 16f, y + 274f, w - 32f, "10K", it.req10, b10, ok10);
            }
            else
            {
                StatRow(x + 16f, y + 226f, w - 32f, "SPD", it.speed, 0.22f, Orange);
                StatRow(x + 16f, y + 252f, w - 32f, "JMP", it.jump, 0.20f, Blue);
                StatRow(x + 16f, y + 278f, w - 32f, "STA", it.stamina, 0.40f, Green);
            }
        }
        else
        {
            StatRow(x + 16f, y + 222f, w - 32f, "HADIAH", it.reward, 0.80f, Gold);
            if (it.req5 > 0f || it.req10 > 0f)
            {
                float b5 = Store.BestAny(5000);
                float b10 = Store.BestAny(10000);
                bool ok5 = b5 > 0f && b5 < it.req5;
                bool ok10 = b10 > 0f && b10 < it.req10;
                ReqLine(x + 16f, y + 246f, w - 32f, "5K", it.req5, b5, ok5);
                ReqLine(x + 16f, y + 276f, w - 32f, "10K", it.req10, b10, ok10);
            }
            else T(new Rect(x + 8f, y + 252f, w - 16f, 26f), "Tanpa syarat", 15, Gray, TextAnchor.MiddleCenter, false);
        }

        var r = new Rect(x + 14f, y + h - 70f, w - 28f, 56f);
        if (owned)
        {
            if (equipped) Btn(r, "TERPASANG", Slate, 22, false);
            else if (Btn(r, "PASANG", Green, 26)) Equip(it);
        }
        else if (!met)
        {
            Btn(r, "TERKUNCI", Slate, 22, false);
        }
        else
        {
            bool can = Store.D.coins >= it.price;
            if (Btn(r, "BELI  " + it.price, can ? Orange : Slate, 24, can))
            {
                Store.D.coins -= it.price;
                Store.D.owned.Add(it.id);
                Equip(it);
                Toast("Berhasil dibeli: " + it.name);
            }
        }
    }

    void ReqLine(float x, float y, float w, string dist, float need, float cur, bool ok)
    {
        Disc(x, y + 2f, 18f, ok ? Green : new Color(0.85f, 0.25f, 0.25f));
        T(new Rect(x, y + 2f, 18f, 18f), ok ? "v" : "x", 12, Color.white, TextAnchor.MiddleCenter, false);
        T(new Rect(x + 24f, y - 2f, w - 24f, 22f), "PB " + dist + " < " + Fmt.Time(need), 16, Color.white, TextAnchor.MiddleLeft, false);
        T(new Rect(x + 24f, y + 15f, w - 24f, 16f), "kini " + (cur > 0f ? Fmt.Time(cur) : "--:--"), 13, ok ? new Color(0.5f, 0.95f, 0.6f) : new Color(1f, 0.6f, 0.6f), TextAnchor.MiddleLeft, false);
    }

    void DrawShop()
    {
        Stage(Orange, 0.95f);
        if (Header("TOKO", "SEPATU  |  PELARI  |  TRACK", Orange)) state = S.Menu;

        string[] tabs = { "SEPATU", "PELARI", "TRACK" };
        Color[] accents = { Orange, Blue, Green };
        for (int i = 0; i < 3; i++)
        {
            var r = new Rect(52f + i * 232f, 126f, 220f, 54f);
            bool sel = i == shopTab;
            bool hover;
            bool down;
            if (Hit(r, true, out hover, out down)) { shopTab = i; shopPage = 0; }
            if (Event.current.type == EventType.Repaint)
            {
                Round(r, sel ? Alpha(accents[i], 0.9f) : new Color(0.06f, 0.08f, 0.16f, hover ? 0.95f : 0.8f));
                if (sel)
                {
                    FadeDown(new Rect(r.x + 6f, r.y + 3f, r.width - 12f, 24f), new Color(1f, 1f, 1f, 0.28f));
                    Flat(new Rect(r.x + 20f, r.yMax - 4f, r.width - 40f, 4f), Color.white);
                }
                T(r, tabs[i], 26, sel ? Color.white : Gray, TextAnchor.MiddleCenter, false);
            }
        }

        List<ShopItem> list = shopTab == 0 ? Catalog.Shoes : shopTab == 1 ? Catalog.Runners : Catalog.Tracks;
        string cur = shopTab == 0 ? Store.D.shoe : shopTab == 1 ? Store.D.runner : Store.D.track;

        // Paginasi: 5 kartu per halaman
        const int PageSize = 5;
        int totalPages = Mathf.Max(1, (list.Count + PageSize - 1) / PageSize);
        shopPage = Mathf.Clamp(shopPage, 0, totalPages - 1);
        int startIdx = shopPage * PageSize;
        int endIdx   = Mathf.Min(startIdx + PageSize, list.Count);
        int n        = endIdx - startIdx;

        float cw   = 218f;
        float gap  = 14f;
        float total = n * cw + (n - 1) * gap;
        float sx   = (1280f - total) / 2f;
        for (int i = 0; i < n; i++)
            ShopCard(shopTab, list[startIdx + i], sx + i * (cw + gap), 200f, cw, 384f, cur, accents[shopTab]);

        // Tombol paginasi
        if (totalPages > 1)
        {
            float py = 596f;
            if (shopPage > 0 && Btn(new Rect(52f, py, 120f, 38f), "◀ PREV", Slate, 20))
                shopPage--;
            T(new Rect(400f, py, 480f, 38f), "Hal. " + (shopPage + 1) + " / " + totalPages, 20, Gray, TextAnchor.MiddleCenter, false);
            if (shopPage < totalPages - 1 && Btn(new Rect(1108f, py, 120f, 38f), "NEXT ▶", Slate, 20))
                shopPage++;
        }

        PanelBox(new Rect(52f, 638f, 1176f, 70f), Slate);
        T(new Rect(76f, 642f, 140f, 60f), "LOADOUT", 20, Gray, TextAnchor.MiddleLeft, false);
        ShoeIcon(204f, 652f, 64f, Shoe);
        T(new Rect(278f, 644f, 200f, 60f), Shoe.name, 18, Color.white, TextAnchor.MiddleLeft, false);
        Portrait(490f, 654f, 46f, RunnerItem, Blue);
        T(new Rect(548f, 644f, 200f, 60f), RunnerItem.name, 18, Color.white, TextAnchor.MiddleLeft, false);
        TrackSwatch(762f, 656f, 66f, 42f, Track.theme);
        T(new Rect(840f, 644f, 300f, 60f), Track.name, 18, Color.white, TextAnchor.MiddleLeft, false);
    }

    void DrawMissionColumn(float x, string title, Color accent, List<MissionData> list)
    {
        Round(new Rect(x, 128f, 560f, 38f), Alpha(accent, 0.25f));
        Flat(new Rect(x, 128f, 6f, 38f), accent);
        T(new Rect(x + 20f, 128f, 400f, 38f), title, 26, Bright(accent), TextAnchor.MiddleLeft, false);
        int dn = 0;
        foreach (var q in list) if (q.claimed) dn++;
        T(new Rect(x + 340f, 128f, 200f, 38f), dn + " / " + list.Count + " SELESAI", 17, Gray, TextAnchor.MiddleRight, false);
        for (int i = 0; i < list.Count; i++)
        {
            var m = list[i];
            float y = 178f + i * 144f;
            bool done = m.progress >= m.target;
            Color ac = m.claimed ? Slate : (done ? Green : accent);
            PanelBox(new Rect(x, y, 560f, 130f), ac);
            if (done && !m.claimed) Glow(x + 60f, y + 65f, 120f, Alpha(Green, 0.25f + 0.1f * Mathf.Sin(Time.unscaledTime * 5f)));
            Disc(x + 14f, y + 30f, 72f, Shade(ac, 0.6f));
            Disc(x + 18f, y + 34f, 64f, ac);
            if (m.type == "dist") StatIcon(0, x + 26f, y + 42f, 48f, Shade(ac, 0.5f));
            else
            {
                Tint(Color.white);
                GUI.DrawTexture(new Rect(x + 28f, y + 44f, 44f, 44f), SpriteFactory.Star.texture);
                GUI.color = Color.white;
            }
            T(new Rect(x + 104f, y + 12f, 300f, 32f), m.desc, 21, Color.white, TextAnchor.MiddleLeft, false);
            Bar(new Rect(x + 104f, y + 52f, 290f, 16f), m.target > 0 ? m.progress / m.target : 0f, ac);
            T(new Rect(x + 104f, y + 68f, 290f, 24f), MissionProgress(m), 17, Gray, TextAnchor.MiddleLeft, false);
            Chip(x + 104f, y + 94f, 96f, 28f, 0, m.coins.ToString(), Gold);
            Chip(x + 206f, y + 94f, 82f, 28f, 2, m.uc.ToString(), new Color(0.8f, 0.7f, 1f));
            Chip(x + 294f, y + 94f, 100f, 28f, 1, "+" + m.xp, new Color(0.45f, 0.85f, 1f));
            var r = new Rect(x + 416f, y + 34f, 128f, 62f);
            if (m.claimed) Btn(r, "SELESAI", Slate, 20, false);
            else if (Btn(r, "KLAIM", Green, 24, done))
            {
                if (Store.Claim(m)) Toast("+" + m.coins + " coins, +" + m.uc + " koin upgrade, +" + m.xp + " XP");
            }
        }
    }

    void DrawMissions()
    {
        Stage(Blue, 0.95f);
        if (Header("MISI", "Selesaikan misi untuk hadiah", Blue)) state = S.Menu;
        DrawMissionColumn(50f, "HARIAN", Orange, Store.D.daily);
        DrawMissionColumn(670f, "MINGGUAN", Blue, Store.D.weekly);
        T(new Rect(0f, 640f, 1280f, 30f), "Misi harian reset setiap hari, misi mingguan reset setiap pekan", 18, Gray, TextAnchor.MiddleCenter, false);
    }

    void DrawTrain()
    {
        Stage(Purple, 0.95f);
        if (Header("LATIHAN", "Tingkatkan kemampuan dengan Koin Upgrade", Purple)) state = S.Menu;

        var d = Store.D;
        int lv = Store.Level(d.xp);
        PanelBox(new Rect(40f, 130f, 340f, 470f), Purple);
        Glow(210f, 230f, 170f, Alpha(Purple, 0.5f));
        Portrait(140f, 152f, 140f, RunnerItem, Shade(Purple, 0.6f));
        T(new Rect(50f, 300f, 320f, 34f), RunnerItem.name.ToUpper(), 28, Color.white, TextAnchor.MiddleCenter, false);
        T(new Rect(50f, 332f, 320f, 24f), "LEVEL " + lv + "  " + Store.Title(lv).ToUpper(), 17, Bright(Purple), TextAnchor.MiddleCenter, false);
        Flat(new Rect(64f, 366f, 292f, 1f), new Color(1f, 1f, 1f, 0.15f));
        T(new Rect(64f, 372f, 292f, 24f), "TOTAL BONUS", 15, Gray, TextAnchor.MiddleLeft, false);
        for (int i = 0; i < 4; i++)
        {
            float y = 402f + i * 46f;
            Disc(64f, y + 2f, 30f, UpColors[i]);
            T(new Rect(102f, y, 150f, 34f), UpNames[i], 17, Color.white, TextAnchor.MiddleLeft, false);
            T(new Rect(240f, y, 116f, 34f), "+" + (UpShown(i, Store.UpLevel(i)) * 100f).ToString("0.#") + "%", 22, Bright(UpColors[i]), TextAnchor.MiddleRight, false);
        }

        for (int i = 0; i < 4; i++)
        {
            float x = 400f;
            float y = 130f + i * 124f;
            int ulv = Store.UpLevel(i);
            int cost = Store.UpCost(ulv);
            PanelBox(new Rect(x, y, 840f, 112f), UpColors[i]);
            Glow(x + 60f, y + 56f, 90f, Alpha(UpColors[i], 0.3f));
            StatIcon(i, x + 24f, y + 18f, 76f, UpColors[i]);
            T(new Rect(x + 116f, y + 8f, 260f, 34f), UpNames[i], 28, Color.white, TextAnchor.MiddleLeft, false);
            T(new Rect(x + 330f, y + 12f, 150f, 28f), "LV " + ulv, 22, ulv >= 10 ? Gold : Gray, TextAnchor.MiddleRight, false);
            int filled = ulv % 10;
            for (int p = 0; p < 10; p++)
            {
                var pr = new Rect(x + 116f + p * 36f, y + 50f, 30f, 14f);
                Round(pr, p < filled ? UpColors[i] : new Color(0f, 0f, 0f, 0.55f));
                if (p < filled) FadeDown(new Rect(pr.x + 4f, pr.y + 2f, pr.width - 8f, 6f), new Color(1f, 1f, 1f, 0.35f));
            }
            T(new Rect(x + 116f, y + 72f, 370f, 26f), UpDesc[i], 16, Gray, TextAnchor.MiddleLeft, false);
            T(new Rect(x + 500f, y + 12f, 170f, 28f), "Bonus +" + (UpShown(i, ulv) * 100f).ToString("0.#") + "%", 19, Color.white, TextAnchor.MiddleLeft, false);
            T(new Rect(x + 500f, y + 42f, 170f, 28f), (i == 3 && UpShown(i, ulv + 1) <= UpShown(i, ulv) ? "Efek maksimal" : "Naik +" + (UpShown(i, ulv + 1) * 100f).ToString("0.#") + "%"), 19, new Color(0.5f, 0.95f, 0.6f), TextAnchor.MiddleLeft, false);
            var br = new Rect(x + 676f, y + 12f, 144f, 54f);
            bool can = d.ucoins >= cost;
            Chip(x + 676f, y + 72f, 144f, 32f, 2, cost.ToString(), can ? new Color(0.8f, 0.7f, 1f) : new Color(1f, 0.55f, 0.55f));
            if (Btn(br, "LATIH", Green, 24, can))
            {
                if (Store.Upgrade(i))
                {
                    flashIdx = i;
                    flashT = 0.6f;
                    Toast(UpNames[i] + " naik ke LV " + Store.UpLevel(i));
                }
            }
            int dcost = Store.DownCost(ulv);
            bool canDown = ulv > 0 && d.ucoins >= dcost;
            bool armed = downArm == i && downArmT > 0f;
            string dl = ulv <= 0 ? "TURUN" : (armed ? "YAKIN? -" + dcost : "TURUN  -" + dcost);
            if (Btn(new Rect(x + 500f, y + 72f, 164f, 34f), dl, armed ? new Color(0.85f, 0.25f, 0.25f) : Slate, 17, canDown))
            {
                if (armed)
                {
                    if (Store.Downgrade(i))
                    {
                        Toast(UpNames[i] + " turun ke LV " + Store.UpLevel(i));
                        downArm = -1;
                    }
                }
                else
                {
                    downArm = i;
                    downArmT = 3f;
                }
            }
            if (flashIdx == i && flashT > 0f) Round(new Rect(x, y, 840f, 112f), new Color(1f, 1f, 1f, flashT * 0.5f));
        }

        PanelBox(new Rect(40f, 618f, 1200f, 80f), Purple);
        Gem(62f, 630f, 56f);
        T(new Rect(136f, 624f, 1060f, 40f), "KOIN UPGRADE", 28, new Color(0.8f, 0.7f, 1f), TextAnchor.MiddleLeft, false);
        T(new Rect(136f, 660f, 1060f, 28f), "Level tanpa batas. Didapat dari balapan, misi, dan rekor baru; tidak bisa ditukar Coins. Menurunkan level juga berbayar Koin Upgrade.", 17, Gray, TextAnchor.MiddleLeft, false);
    }

    void StatTile(float x, float y, float w, float h, string k, string v, Color c)
    {
        Round(new Rect(x, y, w, h), new Color(0.07f, 0.1f, 0.2f, 0.95f));
        Flat(new Rect(x, y + 10f, 4f, h - 20f), c);
        T(new Rect(x + 16f, y + 4f, w - 24f, 22f), k, 14, Gray, TextAnchor.MiddleLeft, false);
        T(new Rect(x + 16f, y + 24f, w - 24f, h - 28f), v, 21, Color.white, TextAnchor.MiddleLeft, false);
    }

    void DrawProfile()
    {
        Stage(Gold, 0.95f);
        var d = Store.D;
        int lv = Store.Level(d.xp);
        int lo = Store.XpFloor(lv);
        int hi = Store.XpFloor(lv + 1);
        string nm = string.IsNullOrEmpty(d.name) ? "PELARI" : d.name.ToUpper();
        if (Header("PROFIL", nm, Gold)) state = S.Menu;

        PanelBox(new Rect(40f, 130f, 470f, 210f), Gold);
        Glow(130f, 235f, 130f, Alpha(Gold, 0.4f));
        Portrait(60f, 160f, 150f, RunnerItem, Shade(Gold, 0.5f));
        T(new Rect(228f, 146f, 270f, 40f), nm, 28, Color.white, TextAnchor.MiddleLeft, false);
        Round(new Rect(228f, 190f, 150f, 28f), Orange);
        T(new Rect(228f, 190f, 150f, 28f), "LEVEL " + lv, 18, Color.white, TextAnchor.MiddleCenter, false);
        T(new Rect(388f, 190f, 112f, 28f), Store.Title(lv).ToUpper(), 16, Bright(Gold), TextAnchor.MiddleLeft, false);
        Bar(new Rect(228f, 232f, 262f, 18f), (d.xp - lo) / (float)(hi - lo), new Color(0.4f, 0.85f, 1f));
        T(new Rect(228f, 252f, 262f, 22f), "XP " + d.xp + " / " + hi, 15, Gray, TextAnchor.MiddleLeft, false);
        if (Btn(new Rect(228f, 284f, 262f, 42f), "GANTI AKUN", Slate, 20))
        {
            editingName = true;
            nameInput = d.name;
            focusField = true;
            state = S.Login;
        }

        int totalUp = 0;
        foreach (var u in d.ups) totalUp += u;
        string[] k = { "COINS", "KOIN UPGRADE", "TOTAL JARAK", "BALAPAN", "LATIHAN", "SEPATU", "PELARI", "TRACK" };
        string[] v = { d.coins.ToString(), d.ucoins.ToString(), Fmt.Dist(d.totalMeters), d.races.ToString(), totalUp.ToString(), Shoe.name, RunnerItem.name, Track.name };
        Color[] cc = { Gold, Purple, Blue, Green, Purple, Orange, Blue, Green };
        for (int i = 0; i < k.Length; i++) StatTile(40f + (i % 2) * 240f, 356f + (i / 2) * 82f, 230f, 72f, k[i], v[i], cc[i]);

        PanelBox(new Rect(540f, 130f, 700f, 560f), Gold);
        T(new Rect(570f, 140f, 400f, 44f), "PERSONAL BEST", 28, Gold, TextAnchor.MiddleLeft, false);
        T(new Rect(900f, 140f, 310f, 44f), "WAKTU  /  PACE", 15, Gray, TextAnchor.MiddleRight, false);
        for (int i = 0; i < Targets.Length; i++)
        {
            float b = Store.BestAny(Targets[i]);
            float y = 192f + i * 70f;
            bool has = b > 0f;
            Round(new Rect(566f, y, 648f, 62f), has ? Alpha(Gold, 0.12f) : new Color(1f, 1f, 1f, 0.04f));
            Flat(new Rect(566f, y + 8f, 5f, 46f), has ? Gold : Slate);
            T(new Rect(590f, y, 160f, 62f), TargetNames[i], 26, Color.white, TextAnchor.MiddleLeft, false);
            T(new Rect(740f, y, 460f, 62f), has ? Fmt.Time(b) + "    " + Fmt.Pace(b, Targets[i]) : "--:--", 24, has ? Gold : Gray, TextAnchor.MiddleRight, false);
        }
    }

    void PlayerChip()
    {
        var d = Store.D;
        int lv = Store.Level(d.xp);
        Round(new Rect(22f, 18f, 280f, 66f), new Color(0.03f, 0.04f, 0.1f, 0.88f));
        Flat(new Rect(22f, 28f, 4f, 46f), Orange);
        Portrait(38f, 26f, 50f, RunnerItem, Shade(Orange, 0.6f));
        T(new Rect(102f, 22f, 196f, 32f), string.IsNullOrEmpty(d.name) ? "PELARI" : d.name.ToUpper(), 21, Color.white, TextAnchor.MiddleLeft, false);
        T(new Rect(102f, 50f, 196f, 28f), "LEVEL " + lv + "  " + Store.Title(lv).ToUpper(), 15, Bright(Orange), TextAnchor.MiddleLeft, false);
    }

    void EnsureField()
    {
        if (fieldStyle != null) return;
        fieldStyle = new GUIStyle(GUI.skin.textField);
        fieldStyle.fontSize = 32;
        fieldStyle.fontStyle = FontStyle.Bold;
        fieldStyle.alignment = TextAnchor.MiddleLeft;
        fieldStyle.padding = new RectOffset(20, 20, 0, 0);
        fieldStyle.normal.background = box8.normal.background;
        fieldStyle.focused.background = box8.normal.background;
        fieldStyle.hover.background = box8.normal.background;
        fieldStyle.border = new RectOffset(10, 10, 10, 10);
        fieldStyle.normal.textColor = Color.white;
        fieldStyle.focused.textColor = Color.white;
        fieldStyle.hover.textColor = Color.white;
    }

    void EnterGame(string who)
    {
        Store.D.name = who;
        Store.Save();
        editingName = false;
        state = S.Menu;
        Toast("Selamat datang, " + who);
    }

    void DrawLogin()
    {
        EnsureStyles();
        EnsureField();
        Stage(Orange, 0.8f);
        var d = Store.D;
        bool returning = !string.IsNullOrEmpty(d.name) && !editingName;
        float bob = Mathf.Sin(Time.unscaledTime * 2f) * 6f;

        T(new Rect(60f, 40f + bob, 700f, 100f), "RUN TO FINISH", 84, Color.white, TextAnchor.MiddleLeft);
        Round(new Rect(66f, 140f, 400f, 38f), Orange);
        T(new Rect(66f, 140f, 400f, 38f), "EPIC PROGRESSION EDITION", 19, Color.white, TextAnchor.MiddleCenter, false);
        Glow(300f, 440f, 280f, Alpha(Orange, 0.45f));
        Portrait(150f, 290f, 300f, RunnerItem, new Color(0.35f, 0.18f, 0.05f));
        T(new Rect(60f, 612f, 480f, 40f), RunnerItem.name.ToUpper(), 30, Color.white, TextAnchor.MiddleCenter, false);

        var p = new Rect(680f, 150f, 540f, 430f);
        PanelBox(p, Orange);
        var e = Event.current;
        if (returning)
        {
            T(new Rect(p.x, p.y + 26f, p.width, 30f), "SELAMAT DATANG KEMBALI", 20, Bright(Orange), TextAnchor.MiddleCenter, false);
            T(new Rect(p.x, p.y + 62f, p.width, 70f), d.name.ToUpper(), 54, Color.white, TextAnchor.MiddleCenter, false);
            int lv = Store.Level(d.xp);
            Round(new Rect(p.x + 70f, p.y + 150f, p.width - 140f, 100f), new Color(1f, 1f, 1f, 0.06f));
            T(new Rect(p.x + 70f, p.y + 156f, 200f, 40f), "LEVEL " + lv, 28, Gold, TextAnchor.MiddleCenter, false);
            T(new Rect(p.x + 270f, p.y + 156f, 200f, 40f), Fmt.Dist(d.totalMeters), 28, Color.white, TextAnchor.MiddleCenter, false);
            T(new Rect(p.x + 70f, p.y + 196f, 200f, 36f), Store.Title(lv).ToUpper(), 17, Gray, TextAnchor.MiddleCenter, false);
            T(new Rect(p.x + 270f, p.y + 196f, 200f, 36f), "TOTAL JARAK", 17, Gray, TextAnchor.MiddleCenter, false);
            if (Btn(new Rect(p.x + 60f, p.y + 276f, p.width - 120f, 76f), "LANJUTKAN", Orange, 38)) EnterGame(d.name);
            if (Btn(new Rect(p.x + 150f, p.y + 364f, p.width - 300f, 46f), "GANTI AKUN", Slate, 20))
            {
                editingName = true;
                nameInput = "";
                focusField = true;
            }
        }
        else
        {
            T(new Rect(p.x, p.y + 24f, p.width, 50f), editingName && !string.IsNullOrEmpty(d.name) ? "GANTI AKUN" : "LOGIN", 42, Color.white, TextAnchor.MiddleCenter, false);
            T(new Rect(p.x, p.y + 76f, p.width, 28f), "Masukkan nama pelarimu", 19, Gray, TextAnchor.MiddleCenter, false);
            var fr = new Rect(p.x + 50f, p.y + 130f, p.width - 100f, 70f);
            bool submit = false;
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                submit = true;
                e.Use();
            }
            GUI.SetNextControlName("nameField");
            GUI.backgroundColor = new Color(0.1f, 0.14f, 0.3f, 1f);
            nameInput = GUI.TextField(fr, nameInput, 14, fieldStyle);
            GUI.backgroundColor = Color.white;
            if (focusField)
            {
                GUI.FocusControl("nameField");
                if (e.type == EventType.Repaint) focusField = false;
            }
            if (string.IsNullOrEmpty(nameInput) && GUI.GetNameOfFocusedControl() != "nameField")
                T(new Rect(fr.x + 20f, fr.y, fr.width - 40f, fr.height), "Nama pelari...", 28, new Color(1f, 1f, 1f, 0.35f), TextAnchor.MiddleLeft, false);
            Round(new Rect(fr.x, fr.yMax + 4f, fr.width, 3f), Orange);
            string clean = nameInput == null ? "" : nameInput.Trim();
            bool ok = clean.Length >= 2;
            T(new Rect(p.x, p.y + 214f, p.width, 24f), ok ? "Siap berlari, " + clean + "!" : "Minimal 2 karakter", 17, ok ? new Color(0.5f, 0.95f, 0.6f) : Gray, TextAnchor.MiddleCenter, false);
            if (Btn(new Rect(p.x + 60f, p.y + 256f, p.width - 120f, 76f), "MASUK", Orange, 38, ok) || (submit && ok)) EnterGame(clean);
            if (Btn(new Rect(p.x + 150f, p.y + 346f, p.width - 300f, 46f), "MAIN SEBAGAI TAMU", Slate, 18)) EnterGame(string.IsNullOrEmpty(d.name) ? "Pelari" : d.name);
        }
    }
}

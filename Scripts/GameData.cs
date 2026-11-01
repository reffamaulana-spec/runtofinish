using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public enum Difficulty { Easy = 0, Normal = 1, Hard = 2, Elite = 3 }

[Serializable]
public class ShopItem
{
    public string id;
    public string name;
    public int price;
    public float speed;
    public float jump;
    public float stamina;
    public float reward;
    public int theme;
    public string info;
    public float req5;
    public float req10;

    public string Desc()
    {
        var p = new List<string>();
        if (speed > 0f) p.Add("Speed +" + Mathf.RoundToInt(speed * 100f) + "%");
        if (jump > 0f) p.Add("Jump +" + Mathf.RoundToInt(jump * 100f) + "%");
        if (stamina > 0f) p.Add("Stamina +" + Mathf.RoundToInt(stamina * 100f) + "%");
        if (reward > 0f) p.Add("Hadiah +" + Mathf.RoundToInt(reward * 100f) + "%");
        return p.Count == 0 ? "Standar" : string.Join(", ", p.ToArray());
    }
}

public static class Catalog
{
    public static readonly List<ShopItem> Shoes = new List<ShopItem>
    {
        // Common
        new ShopItem { id = "shoe_basic",      name = "Basic Trainer",    price = 0,    info = "Sepatu latihan putih standar" },
        // Rare
        new ShopItem { id = "shoe_tempo",      name = "Tempo Racer",      price = 500,  speed = 0.05f, info = "Tipis dan ringan, merah ngebut" },
        new ShopItem { id = "shoe_air",        name = "Air Jump",         price = 800,  jump = 0.10f,  info = "Sol bantalan udara biru" },
        // Epic
        new ShopItem { id = "shoe_endure",     name = "Endurance Pro",    price = 1000, stamina = 0.25f, info = "High-top hijau tahan lelah" },
        new ShopItem { id = "shoe_blaze",      name = "Blaze Strike",     price = 1500, speed = 0.07f, jump = 0.05f, info = "Sepatu api merah-oranye dengan midsole reaktif" },
        // Legend
        new ShopItem { id = "shoe_carbon",     name = "Carbon Elite",     price = 2500, speed = 0.08f, jump = 0.06f, stamina = 0.15f, info = "Pelat karbon emas dengan paku" },
        new ShopItem { id = "shoe_phantom",    name = "Phantom Sprint",   price = 3500, speed = 0.12f, jump = 0.08f, stamina = 0.10f, reward = 0.05f, info = "Sepatu hitam-silver ultraringan, upper mesh transparan", req5 = 1500f, req10 = 3000f },
        // Super
        new ShopItem { id = "shoe_storm",      name = "Storm Runner",     price = 5500, speed = 0.14f, jump = 0.10f, stamina = 0.18f, reward = 0.08f, info = "Sepatu biru-petir dengan sol berteknologi nano-grip", req5 = 1100f, req10 = 2300f },
        new ShopItem { id = "shoe_titanium",   name = "Titanium Force",   price = 6500, speed = 0.13f, jump = 0.12f, stamina = 0.22f, reward = 0.10f, info = "Sepatu titanium abu-emas, sendi karbon ekstra keras", req5 = 1050f, req10 = 2100f },
        // Elite
        new ShopItem { id = "shoe_apex",       name = "Apex Legend X",    price = 9000, speed = 0.18f, jump = 0.15f, stamina = 0.30f, reward = 0.15f, info = "Sepatu merah tua-hitam kelas olimpiade, banned di dunia nyata", req5 = 900f,  req10 = 1800f },
        new ShopItem { id = "shoe_cosmic",     name = "Cosmic Blaster",   price = 12000, speed = 0.20f, jump = 0.18f, stamina = 0.35f, reward = 0.20f, info = "Sepatu ungu kosmik dengan sol anti-gravitasi virtual, edisi terbatas", req5 = 850f, req10 = 1700f },
    };

    public static readonly List<ShopItem> Runners = new List<ShopItem>
    {
        // Common
        new ShopItem { id = "runner_rio",      name = "Rio",               price = 0,     info = "Serba bisa, berbando merah" },
        // Rare
        new ShopItem { id = "runner_maya",     name = "Maya",              price = 700,   speed = 0.03f, stamina = 0.10f, info = "Lincah, berkuncir kuda" },
        // Rare
        new ShopItem { id = "runner_kenji",    name = "Kenji",             price = 1200,  speed = 0.02f, jump = 0.08f, info = "Jangkung, berkacamata" },
        // Epic
        new ShopItem { id = "runner_bima",     name = "Bima",              price = 1800,  stamina = 0.20f, info = "Kekar, berjanggut, tahan banting" },
        // Legend
        new ShopItem { id = "runner_ace",      name = "Ace",               price = 3000,  speed = 0.05f, jump = 0.05f, stamina = 0.15f, info = "Atlet elit berbadan kekar" },
        // Super — Jonathan Sawe: pelari Kenya fenomenal, stride panjang, stamina baja
        new ShopItem { id = "runner_sawe",     name = "Jonathan Sawe",     price = 5500,  speed = 0.12f, jump = 0.06f, stamina = 0.22f, reward = 0.08f,
                       req5 = 810f, req10 = 1650f, info = "Pelari Kenya bergaya stride panjang, juara dunia lintasan" },
        // Super — Jakob Ingebrigtsen: pelari Norwegia, tinggi atletis, spesialis 1500m-5000m
        new ShopItem { id = "runner_jakob",    name = "Jakob Ingebrigtsen",price = 7000,  speed = 0.14f, jump = 0.10f, stamina = 0.20f, reward = 0.12f,
                       req5 = 760f, req10 = 1560f, info = "Pelari Norwegia tinggi gagah, juara olimpiade 1500m & 5000m" },
        // Elite — Reffa Fabyan: pelari lokal Indonesia, level tertinggi, kecepatan & stamina sempurna
        new ShopItem { id = "runner_reffa",    name = "Reffa Fabyan",      price = 12000, speed = 0.20f, jump = 0.14f, stamina = 0.32f, reward = 0.20f,
                       req5 = 700f, req10 = 1440f, info = "Pelari legendaris Indonesia, sprint terakhir yang mematikan, tak tertandingi" },
    };

    public static readonly List<ShopItem> Tracks = new List<ShopItem>
    {
        // Common
        new ShopItem { id = "track_forest",   name = "Forest Track",     price = 0,     theme = 0, info = "Jalur tanah: pohon tumbang dan lumpur" },
        // Rare
        new ShopItem { id = "track_city",     name = "City Sprint",      price = 600,   theme = 1, reward = 0.10f, info = "Jalan raya: lubang dan cone roboh" },
        // Epic
        new ShopItem { id = "track_night",    name = "Night Stadium",    price = 1000,  theme = 2, reward = 0.20f, req5 = 1020f, req10 = 2160f, info = "Lintasan lari malam: hurdle" },
        new ShopItem { id = "track_olympic",  name = "Olympic Stadium",  price = 1500,  theme = 3, reward = 0.30f, req5 = 900f, req10 = 1800f, info = "Stadion olimpiade: hurdle dan foam roller" },
        // Legend — Track Gurun Sahara
        new ShopItem { id = "track_desert",   name = "Desert Inferno",   price = 3000,  theme = 4, reward = 0.40f, req5 = 840f, req10 = 1680f, info = "Gurun Sahara: pasir terbang, duri kaktus, matahari membakar" },
        // Super — Track Futuristik Kota Neon
        new ShopItem { id = "track_neon",     name = "Neon City Dash",   price = 5000,  theme = 5, reward = 0.55f, req5 = 780f, req10 = 1560f, info = "Kota futuristik neon: platform terapung dan laser-grid" },
        // Elite — Track Luar Angkasa
        new ShopItem { id = "track_cosmos",   name = "Cosmos Circuit",   price = 8000,  theme = 6, reward = 0.75f, req5 = 720f, req10 = 1440f, info = "Lintasan orbit luar angkasa: gravitasi rendah, asteroid melayang" },
    };

    public static ShopItem Find(List<ShopItem> list, string id)
    {
        foreach (var i in list) if (i.id == id) return i;
        return list[0];
    }
}

[Serializable]
public class BestEntry
{
    public int meters;
    public float time;
}

[Serializable]
public class MissionData
{
    public string type;
    public int target;
    public int coins;
    public int xp;
    public int uc;
    public float progress;
    public bool claimed;
    public string desc;
}

[Serializable]
public class SaveData
{
    public string name = "";
    public int coins;
    public int ucoins;
    public int xp;
    public float totalMeters;
    public int races;
    public bool done10k;
    public List<string> owned = new List<string> { "shoe_basic", "runner_rio", "track_forest" };
    public string shoe = "shoe_basic";
    public string runner = "runner_rio";
    public string track = "track_forest";
    public List<BestEntry> bests = new List<BestEntry>();
    public int dayId = -1;
    public int weekId = -1;
    public List<int> ups = new List<int> { 0, 0, 0, 0 };
    public List<MissionData> daily = new List<MissionData>();
    public List<MissionData> weekly = new List<MissionData>();
}

public static class Store
{
    const string Key = "RTF_SAVE_V1";
    static readonly float[] Mults = { 1f, 1.25f, 1.75f, 2.5f };
    public static SaveData D;

    public static void Load()
    {
        try
        {
            string s = PlayerPrefs.GetString(Key, "");
            D = string.IsNullOrEmpty(s) ? new SaveData() : JsonUtility.FromJson<SaveData>(s);
        }
        catch (Exception)
        {
            D = new SaveData();
        }
        if (D == null) D = new SaveData();
        if (D.owned == null) D.owned = new List<string> { "shoe_basic", "runner_rio", "track_forest" };
        if (D.bests == null) D.bests = new List<BestEntry>();
        if (D.daily == null) D.daily = new List<MissionData>();
        if (D.weekly == null) D.weekly = new List<MissionData>();
        if (D.ups == null) D.ups = new List<int>();
        while (D.ups.Count < 4) D.ups.Add(0);
        for (int i = 0; i < D.owned.Count; i++) if (D.owned[i] == "track_mountain") D.owned[i] = "track_olympic";
        if (D.track == "track_mountain") D.track = "track_olympic";
        EnsureMissions();
        Save();
    }

    public static void Save()
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(D));
        PlayerPrefs.Save();
    }

    public static float DiffMult(Difficulty d)
    {
        return Mults[(int)d];
    }

    public static int Level(int xp)
    {
        return 1 + (int)Mathf.Sqrt(xp / 100f);
    }

    public static int XpFloor(int level)
    {
        return (level - 1) * (level - 1) * 100;
    }

    public static string Title(int level)
    {
        if (level < 5)  return "Beginner";
        if (level < 10) return "Amateur";
        if (level < 20) return "Pro";
        if (level < 35) return "Elite";
        if (level < 55) return "Super";
        return "Legend";
    }

    public static bool Unlocked(Difficulty d)
    {
        if (d == Difficulty.Hard) return D.races >= 3;
        if (d == Difficulty.Elite) return Level(D.xp) >= 10 || D.done10k;
        return true;
    }

    public static bool MeetsReq(ShopItem it)
    {
        float b5 = BestAny(5000);
        float b10 = BestAny(10000);
        if (it.req5 > 0f && !(b5 > 0f && b5 < it.req5)) return false;
        if (it.req10 > 0f && !(b10 > 0f && b10 < it.req10)) return false;
        return true;
    }

    public static float GetBest(int meters, int mode = 0)
    {
        int key = meters + mode * 100000;
        foreach (var b in D.bests) if (b.meters == key) return b.time;
        return 0f;
    }

    public static float BestAny(int meters)
    {
        float best = 0f;
        for (int m = 0; m < 3; m++)
        {
            float t = GetBest(meters, m);
            if (t > 0f && (best <= 0f || t < best)) best = t;
        }
        return best;
    }

    public static void SetBest(int meters, float time, int mode = 0)
    {
        int key = meters + mode * 100000;
        foreach (var b in D.bests)
        {
            if (b.meters == key)
            {
                b.time = time;
                return;
            }
        }
        D.bests.Add(new BestEntry { meters = key, time = time });
    }

    static int DayId()
    {
        return (int)(DateTime.Now.Date - new DateTime(2024, 1, 1)).TotalDays;
    }

    static void EnsureMissions()
    {
        int day = DayId();
        int week = day / 7;
        if (D.dayId != day || D.daily.Count == 0)
        {
            D.daily = GenDaily(day);
            D.dayId = day;
        }
        if (D.weekId != week || D.weekly.Count == 0)
        {
            D.weekly = GenWeekly(week);
            D.weekId = week;
        }
    }

    static MissionData Make(string type, int target, int coins, int xp, int uc)
    {
        var m = new MissionData { type = type, target = target, coins = coins, xp = xp, uc = uc };
        switch (type)
        {
            case "dist":
                m.desc = "Lari sejauh " + (target / 1000f).ToString("0.#", CultureInfo.InvariantCulture) + " KM";
                break;
            case "races":
                m.desc = "Selesaikan " + target + " balapan";
                break;
            case "normal":
                m.desc = "Selesaikan " + target + " balapan Normal+";
                break;
            default:
                m.desc = "Pecahkan " + target + " Personal Best";
                break;
        }
        return m;
    }

    static List<MissionData> Pick(List<MissionData> pool, int n, System.Random r)
    {
        var res = new List<MissionData>();
        var used = new HashSet<string>();
        while (res.Count < n && pool.Count > 0)
        {
            int i = r.Next(pool.Count);
            var m = pool[i];
            pool.RemoveAt(i);
            if (used.Add(m.type)) res.Add(m);
        }
        return res;
    }

    static List<MissionData> GenDaily(int seed)
    {
        var r = new System.Random(seed);
        var pool = new List<MissionData>
        {
            Make("dist", 3000, 150, 60, 5),
            Make("dist", 5000, 250, 100, 8),
            Make("races", 2, 100, 40, 4),
            Make("races", 3, 150, 60, 5),
            Make("normal", 2, 200, 80, 6),
            Make("pb", 1, 200, 80, 6)
        };
        return Pick(pool, 3, r);
    }

    static List<MissionData> GenWeekly(int seed)
    {
        var r = new System.Random(seed + 7919);
        var pool = new List<MissionData>
        {
            Make("dist", 15000, 800, 300, 30),
            Make("dist", 25000, 1200, 500, 45),
            Make("races", 8, 600, 250, 25),
            Make("races", 12, 900, 350, 35),
            Make("normal", 5, 700, 300, 30),
            Make("pb", 3, 900, 400, 40)
        };
        return Pick(pool, 2, r);
    }

    public static void RegisterRace(float meters, Difficulty diff, bool pb)
    {
        var lists = new[] { D.daily, D.weekly };
        foreach (var list in lists)
        {
            foreach (var m in list)
            {
                if (m.claimed) continue;
                switch (m.type)
                {
                    case "dist":
                        m.progress += meters;
                        break;
                    case "races":
                        m.progress += 1f;
                        break;
                    case "normal":
                        if (diff >= Difficulty.Normal) m.progress += 1f;
                        break;
                    case "pb":
                        if (pb) m.progress += 1f;
                        break;
                }
                if (m.progress > m.target) m.progress = m.target;
            }
        }
    }

    public static bool Claim(MissionData m)
    {
        if (m.claimed || m.progress < m.target) return false;
        m.claimed = true;
        D.coins += m.coins;
        D.xp += m.xp;
        D.ucoins += m.uc;
        Save();
        return true;
    }


    public static int UpLevel(int kind)
    {
        return D.ups[kind];
    }

    public static int UpCost(int level)
    {
        return (int)Mathf.Min(1000000000f, Mathf.Round(15f * Mathf.Pow(1.45f, level)));
    }

    public static int DownCost(int level)
    {
        if (level <= 0) return 0;
        return Mathf.Max(5, Mathf.RoundToInt(UpCost(level - 1) * 0.5f));
    }

    public static bool Downgrade(int kind)
    {
        int lv = D.ups[kind];
        if (lv <= 0) return false;
        int cost = DownCost(lv);
        if (D.ucoins < cost) return false;
        D.ucoins -= cost;
        D.ups[kind] = lv - 1;
        Save();
        return true;
    }

    public static bool Upgrade(int kind)
    {
        int lv = D.ups[kind];
        int cost = UpCost(lv);
        if (D.ucoins < cost) return false;
        D.ucoins -= cost;
        D.ups[kind] = lv + 1;
        Save();
        return true;
    }

    public static int Claimable()
    {
        int n = 0;
        foreach (var m in D.daily) if (!m.claimed && m.progress >= m.target) n++;
        foreach (var m in D.weekly) if (!m.claimed && m.progress >= m.target) n++;
        return n;
    }
}

public static class Fmt
{
    public static string Time(float s)
    {
        if (s < 0f) s = 0f;
        int h = (int)(s / 3600f);
        int m = (int)((s % 3600f) / 60f);
        float sec = s % 60f;
        if (h > 0) return h + ":" + m.ToString("00") + ":" + ((int)sec).ToString("00");
        return m.ToString("00") + ":" + sec.ToString("00.0", CultureInfo.InvariantCulture);
    }

    public static string Pace(float sec, float meters)
    {
        if (meters < 5f || sec <= 0f) return "--:--";
        float spk = sec / (meters / 1000f);
        int m = (int)(spk / 60f);
        int s = (int)(spk % 60f);
        return m.ToString("00") + ":" + s.ToString("00") + "/KM";
    }

    public static string Dist(float m)
    {
        if (m >= 1000f) return (m / 1000f).ToString("0.00", CultureInfo.InvariantCulture) + " KM";
        return Mathf.RoundToInt(m) + " M";
    }
}

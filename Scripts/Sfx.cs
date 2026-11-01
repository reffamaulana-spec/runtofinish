using UnityEngine;

public static class Sfx
{
    public enum Id { Click, Jump, Jump2, Land, Hit, Step, Beep, Go, Reward, Finish, Star, Error }

    const int SR = 44100;
    static AudioClip[] clips;
    static AudioSource[] voices;
    static AudioSource music;
    static int next;
    static bool ready;
    static System.Random rng = new System.Random(11);
    public static bool Muted;

    static float[] Tone(float dur, float f0, float f1, float vol, int wave, float decay)
    {
        int n = (int)(dur * SR);
        var d = new float[n];
        float ph = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            ph += Mathf.Lerp(f0, f1, t) / SR;
            float p = ph - Mathf.Floor(ph);
            float s = wave == 0 ? Mathf.Sin(p * 6.2831853f) : (wave == 1 ? (p < 0.5f ? 0.6f : -0.6f) : (p * 2f - 1f) * 0.7f);
            d[i] = s * Mathf.Pow(1f - t, decay) * Mathf.Min(1f, i / 200f) * vol;
        }
        return d;
    }

    static float[] Noise(float dur, float vol, float decay, float smooth)
    {
        int n = (int)(dur * SR);
        var d = new float[n];
        float lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float w = (float)(rng.NextDouble() * 2.0 - 1.0);
            lp += (w - lp) * smooth;
            d[i] = lp * Mathf.Pow(1f - t, decay) * vol;
        }
        return d;
    }

    static float[] Mix(params float[][] parts)
    {
        int n = 0;
        foreach (var p in parts) if (p.Length > n) n = p.Length;
        var d = new float[n];
        foreach (var p in parts) for (int i = 0; i < p.Length; i++) d[i] += p[i];
        return d;
    }

    static float[] Seq(params float[][] parts)
    {
        int n = 0;
        foreach (var p in parts) n += p.Length;
        var d = new float[n];
        int o = 0;
        foreach (var p in parts)
        {
            for (int i = 0; i < p.Length; i++) d[o + i] = p[i];
            o += p.Length;
        }
        return d;
    }

    static void Place(float[] dst, float[] src, int offset)
    {
        for (int i = 0; i < src.Length && offset + i < dst.Length; i++) dst[offset + i] += src[i];
    }

    static AudioClip Make(string name, float[] d, bool loop = false)
    {
        for (int i = 0; i < d.Length; i++) d[i] = Mathf.Clamp(d[i], -1f, 1f);
        var c = AudioClip.Create(name, d.Length, 1, SR, false);
        c.SetData(d, 0);
        return c;
    }

    static float[] BuildMusic()
    {
        float step = 0.25f;
        int steps = 32;
        var d = new float[(int)(step * steps * SR)];
        float[][] chords =
        {
            new[] { 220f, 261.6f, 329.6f },
            new[] { 174.6f, 220f, 261.6f },
            new[] { 261.6f, 329.6f, 392f },
            new[] { 196f, 246.9f, 293.7f }
        };
        int[] pat = { 0, 1, 2, 1 };
        for (int s = 0; s < steps; s++)
        {
            var ch = chords[s / 8];
            int off = (int)(s * step * SR);
            Place(d, Tone(0.2f, ch[pat[s % 4]] * 2f, ch[pat[s % 4]] * 2f, 0.1f, 1, 1.6f), off);
            if (s % 2 == 0) Place(d, Tone(0.24f, ch[0] * 0.5f, ch[0] * 0.5f, 0.2f, 2, 1.2f), off);
            if (s % 2 == 1) Place(d, Noise(0.04f, 0.07f, 2f, 0.9f), off);
            if (s % 4 == 0) Place(d, Tone(0.12f, 110f, 45f, 0.3f, 0, 2f), off);
        }
        return d;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        ready = false;
        clips = null;
        voices = null;
        music = null;
        next = 0;
    }

    static void Init()
    {
        if (ready && music != null && voices != null && voices[0] != null) return;
        ready = true;
        Muted = PlayerPrefs.GetInt("RTF_MUTE", 0) == 1;
        var go = new GameObject("RunToFinishAudio");
        Object.DontDestroyOnLoad(go);
        if (Object.FindAnyObjectByType<AudioListener>() == null) go.AddComponent<AudioListener>();
        voices = new AudioSource[6];
        for (int i = 0; i < voices.Length; i++)
        {
            voices[i] = go.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }
        music = go.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.clip = Make("music", BuildMusic());

        clips = new AudioClip[12];
        clips[(int)Id.Click] = Make("click", Tone(0.05f, 1200f, 900f, 0.35f, 0, 2f));
        clips[(int)Id.Jump] = Make("jump", Tone(0.16f, 320f, 760f, 0.4f, 0, 1.5f));
        clips[(int)Id.Jump2] = Make("jump2", Tone(0.14f, 520f, 1100f, 0.35f, 0, 1.5f));
        clips[(int)Id.Land] = Make("land", Mix(Tone(0.12f, 140f, 60f, 0.6f, 0, 2f), Noise(0.08f, 0.25f, 2f, 0.3f)));
        clips[(int)Id.Hit] = Make("hit", Mix(Noise(0.35f, 0.7f, 1.6f, 0.25f), Tone(0.3f, 180f, 50f, 0.6f, 2, 1.4f)));
        clips[(int)Id.Step] = Make("step", Noise(0.05f, 0.35f, 2.5f, 0.18f));
        clips[(int)Id.Beep] = Make("beep", Tone(0.14f, 880f, 880f, 0.4f, 0, 1f));
        clips[(int)Id.Go] = Make("go", Tone(0.45f, 1320f, 1320f, 0.35f, 1, 1.2f));
        clips[(int)Id.Reward] = Make("reward", Seq(Tone(0.08f, 988f, 988f, 0.35f, 0, 1f), Tone(0.22f, 1319f, 1319f, 0.35f, 0, 1.5f)));
        clips[(int)Id.Finish] = Make("finish", Seq(Tone(0.12f, 523f, 523f, 0.35f, 1, 1f), Tone(0.12f, 659f, 659f, 0.35f, 1, 1f), Tone(0.12f, 784f, 784f, 0.35f, 1, 1f), Tone(0.55f, 1047f, 1047f, 0.35f, 1, 1.4f)));
        clips[(int)Id.Star] = Make("star", Mix(Tone(0.25f, 880f, 1760f, 0.3f, 0, 1.5f), Tone(0.25f, 1320f, 1320f, 0.15f, 0, 1.5f)));
        clips[(int)Id.Error] = Make("error", Tone(0.18f, 220f, 160f, 0.4f, 1, 1f));
    }

    public static void Play(Id id, float vol = 1f, float pitch = 1f)
    {
        Init();
        if (Muted) return;
        var v = voices[next];
        next = (next + 1) % voices.Length;
        v.pitch = pitch;
        v.PlayOneShot(clips[(int)id], vol);
    }

    public static void MusicVolume(float vol)
    {
        Init();
        music.volume = Muted ? 0f : vol;
        if (!music.isPlaying) music.Play();
    }

    public static void Toggle()
    {
        Init();
        Muted = !Muted;
        PlayerPrefs.SetInt("RTF_MUTE", Muted ? 1 : 0);
        PlayerPrefs.Save();
        if (!Muted) Play(Id.Click);
    }
}

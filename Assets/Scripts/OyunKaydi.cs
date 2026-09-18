using System.Collections.Generic;
using UnityEngine;

// Yarim kalan oyunun kaydi. JsonUtility ile yaziya cevrilip PlayerPrefs'e yaziliyor.
// Karo turleri enum yerine int olarak tutuluyor (KaroTuru sirasi degisirse eski kayit bozulur, dikkat).

[System.Serializable]
public class KaroKaydi
{
    public int x;
    public int y;
    public int tur;
    public float donus;
}

[System.Serializable]
public class GorevKaydi
{
    public int x;
    public int y;
    public int tur;
    public int hedef;
}

[System.Serializable]
public class OyunKaydi
{
    public int puan;
    public int konulanKaroSayisi;
    public int gunlukTarih; // 0 = normal oyun, degilse gunun kasabasi (ornek: 20260918)
    public List<KaroKaydi> karolar = new List<KaroKaydi>();
    public List<int> deste = new List<int>();
    public List<GorevKaydi> gorevler = new List<GorevKaydi>();
}

// Kayit okuma/yazma ve istatistikler (rekor, oynanan oyun, ses ayari)
public static class Kayit
{
    const string OyunAnahtari = "KayitliOyun";

    public static bool KayitVarMi()
    {
        return PlayerPrefs.HasKey(OyunAnahtari);
    }

    public static void OyunuKaydet(OyunKaydi kayit)
    {
        PlayerPrefs.SetString(OyunAnahtari, JsonUtility.ToJson(kayit));
        PlayerPrefs.Save();
    }

    // Kayit yoksa ya da bozuksa null doner
    public static OyunKaydi OyunuYukle()
    {
        if (!KayitVarMi()) return null;

        OyunKaydi kayit = null;
        try
        {
            kayit = JsonUtility.FromJson<OyunKaydi>(PlayerPrefs.GetString(OyunAnahtari));
        }
        catch (System.Exception hata)
        {
            Debug.LogWarning("Kayit okunamadi, yeni oyun baslatiliyor: " + hata.Message);
        }

        if (!GecerliMi(kayit))
        {
            Debug.LogWarning("Kayit bozuk, siliniyor.");
            KaydiSil();
            return null;
        }
        return kayit;
    }

    // Yuklenen kayit oyunu cokertmeyecek durumda mi
    static bool GecerliMi(OyunKaydi kayit)
    {
        if (kayit == null) return false;
        if (kayit.karolar == null || kayit.karolar.Count == 0) return false;
        if (kayit.deste == null || kayit.deste.Count == 0) return false; // biten oyun kaydedilmez
        if (kayit.gorevler == null) return false;

        Dictionary<Vector2Int, int> harita = new Dictionary<Vector2Int, int>();
        foreach (KaroKaydi karo in kayit.karolar)
        {
            if (!TurGecerliMi(karo.tur)) return false;

            // Ayni hucrede iki karo olamaz
            Vector2Int hucre = new Vector2Int(karo.x, karo.y);
            if (harita.ContainsKey(hucre)) return false;
            harita[hucre] = karo.tur;
        }
        foreach (int tur in kayit.deste)
        {
            if (!TurGecerliMi(tur)) return false;
        }
        foreach (GorevKaydi gorev in kayit.gorevler)
        {
            // Gorev, ayni turden bir karonun ustunde olmali
            Vector2Int hucre = new Vector2Int(gorev.x, gorev.y);
            if (!harita.ContainsKey(hucre) || harita[hucre] != gorev.tur) return false;
        }
        return true;
    }

    static bool TurGecerliMi(int tur)
    {
        return tur >= 0 && tur < KaroBilgi.TurSayisi;
    }

    public static void KaydiSil()
    {
        PlayerPrefs.DeleteKey(OyunAnahtari);
        PlayerPrefs.Save();
    }

    public static int Rekor()
    {
        return PlayerPrefs.GetInt("Rekor", 0);
    }

    public static int OynananOyun()
    {
        return PlayerPrefs.GetInt("OynananOyun", 0);
    }

    // Butun oyunlarin puanlari toplami; yeni karo turleri bununla acilir
    public static int ToplamPuan()
    {
        return PlayerPrefs.GetInt("ToplamPuan", 0);
    }

    // Bugunun tarihi sayi olarak (2026-09-18 -> 20260918). Gunun kasabasinin tohumu bu.
    public static int BugununTarihi()
    {
        System.DateTime bugun = System.DateTime.Now;
        return bugun.Year * 10000 + bugun.Month * 100 + bugun.Day;
    }

    public static int GunlukRekor(int tarih)
    {
        return PlayerPrefs.GetInt("GunlukRekor_" + tarih, 0);
    }

    // Oyun bitince cagrilir; yeni rekor kirildiysa true doner.
    // Normal oyun genel rekoru, gunun kasabasi o gunun rekorunu gunceller.
    public static bool OyunBitti(int puan, int gunlukTarih)
    {
        PlayerPrefs.SetInt("OynananOyun", OynananOyun() + 1);
        PlayerPrefs.SetInt("ToplamPuan", ToplamPuan() + puan);

        bool yeniRekor;
        if (gunlukTarih == 0)
        {
            yeniRekor = puan > Rekor();
            if (yeniRekor) PlayerPrefs.SetInt("Rekor", puan);
        }
        else
        {
            yeniRekor = puan > GunlukRekor(gunlukTarih);
            if (yeniRekor) PlayerPrefs.SetInt("GunlukRekor_" + gunlukTarih, puan);
        }

        KaydiSil(); // biten oyuna devam edilemez
        PlayerPrefs.Save();
        return yeniRekor;
    }

    public static bool SesAcikMi()
    {
        return PlayerPrefs.GetInt("SesAcik", 1) == 1;
    }

    public static void SesAyariniKaydet(bool acik)
    {
        if (acik)
        {
            PlayerPrefs.SetInt("SesAcik", 1);
        }
        else
        {
            PlayerPrefs.SetInt("SesAcik", 0);
        }
        PlayerPrefs.Save();
    }
}

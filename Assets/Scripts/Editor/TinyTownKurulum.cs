using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Sahneyi kuran ve oyun hesaplarini deneyen editor menusu.
// Menu: TinyTown > Sahneyi Kur / Kendini Test Et
public static class TinyTownKurulum
{
    static string SahneYolu = "Assets/Scenes/Oyun.unity";

    [MenuItem("TinyTown/Sahneyi Kur")]
    public static void SahneyiKur()
    {
        // Icinde sadece Game objesi olan bos sahne
        UnityEngine.SceneManagement.Scene sahne = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject oyun = new GameObject("Game");
        oyun.AddComponent<KasabaOyunu>();
        EditorSceneManager.SaveScene(sahne, SahneYolu);

        // Build listesinde tek sahne bu olsun
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(SahneYolu, true)
        };

        Debug.Log("TinyTown: sahne kuruldu -> " + SahneYolu);
    }

    [MenuItem("TinyTown/Kendini Test Et")]
    public static void SelfTest()
    {
        int hata = 0;

        // 1) Her hucrenin dunya konumu tekrar ayni hucreye donmeli
        for (int q = -6; q <= 6; q++)
        {
            for (int r = -6; r <= 6; r++)
            {
                Vector2Int hucre = new Vector2Int(q, r);
                Vector3 konum = HexGrid.DunyaKonumu(hucre);
                if (HexGrid.HucreBul(konum) != hucre)
                {
                    Debug.LogError("Hucre donusumu hatali: " + hucre);
                    hata++;
                }

                // Merkezden biraz kaymis nokta da ayni hucrede kalmali
                Vector3 kaymis = konum + new Vector3(0.5f, 0f, 0.3f) * Ayarlar.KaroBoyutu;
                if (HexGrid.HucreBul(kaymis) != hucre)
                {
                    Debug.LogError("Kaymis nokta yanlis hucreye dustu: " + hucre);
                    hata++;
                }
            }
        }

        // 2) Komsu hucrelerin merkezleri arasi mesafe hep kok(3) * boyut olmali
        for (int i = 0; i < 6; i++)
        {
            float d = Vector3.Distance(HexGrid.DunyaKonumu(Vector2Int.zero), HexGrid.DunyaKonumu(HexGrid.Komsu(Vector2Int.zero, i)));
            if (Mathf.Abs(d - Mathf.Sqrt(3f) * Ayarlar.KaroBoyutu) > 0.001f)
            {
                Debug.LogError("Komsu mesafesi hatali, yon " + i + ": " + d);
                hata++;
            }
        }

        // 3) Puanlama
        Dictionary<Vector2Int, KaroTuru> harita = new Dictionary<Vector2Int, KaroTuru>();
        harita[new Vector2Int(0, 0)] = KaroTuru.Ev;
        harita[new Vector2Int(1, 0)] = KaroTuru.Gol;
        harita[new Vector2Int(5, 5)] = KaroTuru.Ev; // uzakta, sayilmamali

        // (0,1) hucresi hem Ev'e hem Gol'e komsu: Tarla icin EvTarla + TarlaGol
        int beklenen = Ayarlar.EvTarla + Ayarlar.TarlaGol;
        int bulunan = Puanlama.YerlestirmePuani(harita, new Vector2Int(0, 1), KaroTuru.Tarla);
        if (bulunan != beklenen)
        {
            Debug.LogError("Tarla puani hatali. Beklenen " + beklenen + ", bulunan " + bulunan);
            hata++;
        }

        if (Puanlama.KomsuPuani(KaroTuru.Tarla, KaroTuru.Ev) != Puanlama.KomsuPuani(KaroTuru.Ev, KaroTuru.Tarla))
        {
            Debug.LogError("Komsu puani iki yonlu degil");
            hata++;
        }

        if (Puanlama.YerlestirmePuani(harita, new Vector2Int(0, 1), KaroTuru.Cimen) != 0)
        {
            Debug.LogError("Cimen puan vermemeli");
            hata++;
        }

        // 4) Yerlestirme kurali
        if (Puanlama.YerlestirilebilirMi(harita, new Vector2Int(0, 0))) { Debug.LogError("Dolu hucreye izin verildi"); hata++; }
        if (!Puanlama.YerlestirilebilirMi(harita, new Vector2Int(-1, 0))) { Debug.LogError("Komsu bos hucre reddedildi"); hata++; }
        if (Puanlama.YerlestirilebilirMi(harita, new Vector2Int(-3, 0))) { Debug.LogError("Kopuk hucreye izin verildi"); hata++; }

        if (HexGrid.Mesafe(Vector2Int.zero, new Vector2Int(2, -1)) != 2 || HexGrid.Mesafe(new Vector2Int(-1, 2), new Vector2Int(2, -1)) != 3)
        {
            Debug.LogError("Hex mesafe hesabi hatali");
            hata++;
        }

        // 5) Grup hesaplari
        Dictionary<Vector2Int, KaroTuru> grupHaritasi = new Dictionary<Vector2Int, KaroTuru>();
        grupHaritasi[new Vector2Int(0, 0)] = KaroTuru.Orman;
        grupHaritasi[new Vector2Int(1, 0)] = KaroTuru.Orman;
        grupHaritasi[new Vector2Int(2, 0)] = KaroTuru.Orman;
        grupHaritasi[new Vector2Int(0, 1)] = KaroTuru.Ev;    // farkli tur, gruba girmemeli
        grupHaritasi[new Vector2Int(4, 0)] = KaroTuru.Orman; // ayni tur ama kopuk

        int grupBoyu = GorevSistemi.GrupHucreleri(grupHaritasi, new Vector2Int(0, 0)).Count;
        if (grupBoyu != 3)
        {
            Debug.LogError("Grup boyu hatali. Beklenen 3, bulunan " + grupBoyu);
            hata++;
        }

        // Kopuk ormani baglayinca grup 5 olmali
        grupHaritasi[new Vector2Int(3, 0)] = KaroTuru.Orman;
        grupBoyu = GorevSistemi.GrupHucreleri(grupHaritasi, new Vector2Int(0, 0)).Count;
        if (grupBoyu != 5)
        {
            Debug.LogError("Birlesen grup boyu hatali. Beklenen 5, bulunan " + grupBoyu);
            hata++;
        }

        // Etrafi tamamen kapali tek karo buyuyemez
        Dictionary<Vector2Int, KaroTuru> kapali = new Dictionary<Vector2Int, KaroTuru>();
        kapali[Vector2Int.zero] = KaroTuru.Gol;
        for (int i = 0; i < 6; i++)
        {
            kapali[HexGrid.Komsu(Vector2Int.zero, i)] = KaroTuru.Cimen;
        }
        if (GorevSistemi.GrupBuyuyebilirMi(kapali, GorevSistemi.GrupHucreleri(kapali, Vector2Int.zero)))
        {
            Debug.LogError("Kapali grup buyuyebilir sanildi");
            hata++;
        }
        kapali.Remove(HexGrid.Komsu(Vector2Int.zero, 2));
        if (!GorevSistemi.GrupBuyuyebilirMi(kapali, GorevSistemi.GrupHucreleri(kapali, Vector2Int.zero)))
        {
            Debug.LogError("Acik grup buyuyemez sanildi");
            hata++;
        }

        // 5b) Acilan turlerin puanlari iki yonlu calismali
        if (Puanlama.KomsuPuani(KaroTuru.Tarla, KaroTuru.Degirmen) != Ayarlar.DegirmenTarla ||
            Puanlama.KomsuPuani(KaroTuru.Liman, KaroTuru.Gol) != Ayarlar.LimanGol ||
            Puanlama.KomsuPuani(KaroTuru.Ev, KaroTuru.Liman) != Ayarlar.LimanEv ||
            Puanlama.KomsuPuani(KaroTuru.Degirmen, KaroTuru.Liman) != 0)
        {
            Debug.LogError("Degirmen/Liman puanlari hatali");
            hata++;
        }

        // Her turun modeli Resources'ta olmali
        for (int i = 0; i < KaroBilgi.TurSayisi; i++)
        {
            if (Resources.Load<GameObject>("Modeller/" + KaroBilgi.ModelAdi((KaroTuru)i)) == null)
            {
                Debug.LogError("Model eksik: " + KaroBilgi.ModelAdi((KaroTuru)i));
                hata++;
            }
        }

        // 6) Kayit JSON'a yazilip geri okununca ayni kalmali
        OyunKaydi kayit = new OyunKaydi();
        kayit.puan = 42;
        kayit.konulanKaroSayisi = 7;
        kayit.gunlukTarih = 20260918;
        KaroKaydi karo = new KaroKaydi();
        karo.x = -2; karo.y = 3; karo.tur = (int)KaroTuru.Gol; karo.donus = 120f;
        kayit.karolar.Add(karo);
        kayit.deste.Add((int)KaroTuru.Ev);
        kayit.deste.Add((int)KaroTuru.Tarla);
        GorevKaydi gorevKaydi = new GorevKaydi();
        gorevKaydi.x = 1; gorevKaydi.y = -1; gorevKaydi.tur = (int)KaroTuru.Orman; gorevKaydi.hedef = 6;
        kayit.gorevler.Add(gorevKaydi);

        OyunKaydi okunan = JsonUtility.FromJson<OyunKaydi>(JsonUtility.ToJson(kayit));
        if (okunan.puan != 42 || okunan.konulanKaroSayisi != 7 || okunan.gunlukTarih != 20260918 ||
            okunan.karolar.Count != 1 || okunan.karolar[0].x != -2 || okunan.karolar[0].y != 3 ||
            okunan.karolar[0].tur != (int)KaroTuru.Gol || okunan.karolar[0].donus != 120f ||
            okunan.deste.Count != 2 || okunan.deste[1] != (int)KaroTuru.Tarla ||
            okunan.gorevler.Count != 1 || okunan.gorevler[0].hedef != 6)
        {
            Debug.LogError("Kayit JSON donusumu hatali: " + JsonUtility.ToJson(okunan));
            hata++;
        }

        if (hata == 0)
        {
            Debug.Log("TinyTown SelfTest: tum testler gecti.");
        }
        else
        {
            Debug.LogError("TinyTown SelfTest: " + hata + " hata.");
        }
    }
}

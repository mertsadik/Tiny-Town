using System.Collections.Generic;
using UnityEngine;

// Tek bir gorev: bu hucrenin bulundugu grup "hedef" buyukluge ulasmali
public class Gorev
{
    public Vector2Int hucre;
    public KaroTuru tur;
    public int hedef;
    public GorevIsareti isaret;
}

// Karolarin ustunde cikan "grubu buyut" gorevlerini yonetir.
// Gorev tamamlaninca desteye yeni karolar eklenir, boylece iyi oynayan daha uzun oynar.
public class GorevSistemi : MonoBehaviour
{
    Arayuz arayuz;
    Efektler efektler;
    Sesler sesler;
    List<Gorev> gorevler = new List<Gorev>();

    public void Kur(Arayuz arayuzScripti, Efektler efektlerScripti, Sesler seslerScripti)
    {
        arayuz = arayuzScripti;
        efektler = efektlerScripti;
        sesler = seslerScripti;
    }

    // Her karo konulduktan sonra cagrilir. Desteye eklenecek karo sayisini dondurur.
    public int KaroKonuldu(Dictionary<Vector2Int, KaroTuru> harita, Vector2Int hucre, KaroTuru tur)
    {
        int odul = GorevleriKontrolEt(harita);
        YeniGorevDene(harita, hucre, tur);
        return odul;
    }

    public void IsaretleriGizle()
    {
        foreach (Gorev gorev in gorevler)
        {
            gorev.isaret.gameObject.SetActive(false);
        }
    }

    int GorevleriKontrolEt(Dictionary<Vector2Int, KaroTuru> harita)
    {
        int toplamOdul = 0;

        // Listeden eleman silecegimiz icin sondan basa dogru geziyoruz
        for (int i = gorevler.Count - 1; i >= 0; i--)
        {
            Gorev gorev = gorevler[i];
            List<Vector2Int> grup = GrupHucreleri(harita, gorev.hucre);
            Vector3 yaziKonumu = HexGrid.DunyaKonumu(gorev.hucre) + Vector3.up * (Ayarlar.YaziYuksekligi + 0.4f);

            if (grup.Count >= gorev.hedef)
            {
                toplamOdul += Ayarlar.GorevKaroOdulu;
                arayuz.UcanYaziGoster(yaziKonumu, "+" + Ayarlar.GorevKaroOdulu + " KARO", new Color(1f, 0.85f, 0.3f));
                efektler.KonfetiPatlat(HexGrid.DunyaKonumu(gorev.hucre));
                sesler.GorevTamamSesi();
                GoreviSil(i);
            }
            else if (!GrupBuyuyebilirMi(harita, grup))
            {
                // Grubun etrafi tamamen doldu, gorev artik yapilamaz
                arayuz.UcanYaziGoster(yaziKonumu, "Görev kapandı", new Color(0.85f, 0.85f, 0.85f));
                sesler.GorevKapandiSesi();
                GoreviSil(i);
            }
            else
            {
                gorev.isaret.YaziGuncelle(grup.Count, gorev.hedef);
            }
        }

        return toplamOdul;
    }

    void YeniGorevDene(Dictionary<Vector2Int, KaroTuru> harita, Vector2Int hucre, KaroTuru tur)
    {
        if (tur == KaroTuru.Cimen) return;
        if (gorevler.Count >= Ayarlar.EnFazlaGorev) return;

        // Hic gorev yoksa kesin cikar, varsa sansa bagli
        if (gorevler.Count > 0 && Random.value > Ayarlar.GorevCikmaSansi) return;

        List<Vector2Int> grup = GrupHucreleri(harita, hucre);
        if (!GrupBuyuyebilirMi(harita, grup)) return;

        foreach (Gorev mevcut in gorevler)
        {
            // Ayni grupta zaten gorev varsa ikincisini acma
            if (grup.Contains(mevcut.hucre)) return;

            // Rozetler ekranda ust uste binmesin
            if (HexGrid.Mesafe(mevcut.hucre, hucre) < Ayarlar.GorevlerArasiMesafe) return;
        }

        int hedef = grup.Count + Random.Range(Ayarlar.GorevEnAzArtis, Ayarlar.GorevEnFazlaArtis + 1);
        GorevEkle(harita, hucre, tur, hedef);
    }

    // Yeni gorev acar. Kayittan yuklenirken de dogrudan bu cagrilir.
    public void GorevEkle(Dictionary<Vector2Int, KaroTuru> harita, Vector2Int hucre, KaroTuru tur, int hedef)
    {
        Gorev gorev = new Gorev();
        gorev.hucre = hucre;
        gorev.tur = tur;
        gorev.hedef = hedef;

        Vector3 isaretKonumu = HexGrid.DunyaKonumu(hucre) + Vector3.up * Ayarlar.YaziYuksekligi;
        gorev.isaret = arayuz.GorevIsaretiOlustur(isaretKonumu, tur);
        gorev.isaret.YaziGuncelle(GrupHucreleri(harita, hucre).Count, hedef);

        gorevler.Add(gorev);
    }

    // Kaydetmek icin aktif gorevlerin listesi
    public List<Gorev> AktifGorevler()
    {
        return gorevler;
    }

    void GoreviSil(int index)
    {
        Destroy(gorevler[index].isaret.gameObject);
        gorevler.RemoveAt(index);
    }

    // ---------------- GRUP HESAPLARI ----------------

    // Baslangic hucresiyle ayni turden, birbirine degen tum hucreleri bulur (genislik oncelikli arama)
    public static List<Vector2Int> GrupHucreleri(Dictionary<Vector2Int, KaroTuru> harita, Vector2Int baslangic)
    {
        List<Vector2Int> grup = new List<Vector2Int>();
        if (!harita.ContainsKey(baslangic)) return grup;

        KaroTuru tur = harita[baslangic];
        Queue<Vector2Int> siradakiler = new Queue<Vector2Int>();
        siradakiler.Enqueue(baslangic);
        grup.Add(baslangic);

        while (siradakiler.Count > 0)
        {
            Vector2Int simdiki = siradakiler.Dequeue();
            for (int i = 0; i < 6; i++)
            {
                Vector2Int komsu = HexGrid.Komsu(simdiki, i);
                if (harita.ContainsKey(komsu) && harita[komsu] == tur && !grup.Contains(komsu))
                {
                    grup.Add(komsu);
                    siradakiler.Enqueue(komsu);
                }
            }
        }

        return grup;
    }

    // Grubun yaninda en az bir bos hucre var mi
    public static bool GrupBuyuyebilirMi(Dictionary<Vector2Int, KaroTuru> harita, List<Vector2Int> grup)
    {
        foreach (Vector2Int hucre in grup)
        {
            for (int i = 0; i < 6; i++)
            {
                if (!harita.ContainsKey(HexGrid.Komsu(hucre, i))) return true;
            }
        }
        return false;
    }
}

using UnityEngine;

// Karonun yukaridan dusup yere oturmasi ve komsu puan verince zipla hareketi
public class KaroAnimasyonu : MonoBehaviour
{
    // Karo yere degince oyuna haber verilir (ses, toz, komsu ziplamasi icin)
    public KasabaOyunu oyun;
    public Vector2Int hucre;

    const int Bos = 0;
    const int Dusuyor = 1;
    const int Esniyor = 2;
    const int Zipliyor = 3;

    int durum = Bos;
    float gecenSure = 0f;
    Vector3 yerKonumu;
    Vector3 normalOlcek;

    public void Dus()
    {
        yerKonumu = transform.position;
        normalOlcek = transform.localScale;
        transform.position = yerKonumu + Vector3.up * Ayarlar.DusmeYuksekligi;
        durum = Dusuyor;
        gecenSure = 0f;
    }

    public void Zipla()
    {
        // Karo zaten hareket ediyorsa araya girme
        if (durum != Bos) return;

        yerKonumu = transform.position;
        normalOlcek = transform.localScale;
        durum = Zipliyor;
        gecenSure = 0f;
    }

    void Update()
    {
        if (durum == Bos) return;

        gecenSure += Time.deltaTime;

        if (durum == Dusuyor)
        {
            float t = Mathf.Clamp01(gecenSure / Ayarlar.DusmeSuresi);
            // t * t: yavas baslar, yere yaklastikca hizlanir
            float yukseklik = Mathf.Lerp(Ayarlar.DusmeYuksekligi, 0f, t * t);
            transform.position = yerKonumu + Vector3.up * yukseklik;

            if (t >= 1f)
            {
                durum = Esniyor;
                gecenSure = 0f;
                oyun.KaroYereDegdi(hucre);
            }
        }
        else if (durum == Esniyor)
        {
            // Yere carpinca bir an basilip genisler, sonra eski haline doner
            float t = Mathf.Clamp01(gecenSure / Ayarlar.EsnemeSuresi);
            float esneme = Mathf.Sin(t * Mathf.PI);
            transform.localScale = new Vector3(
                normalOlcek.x * (1f + 0.1f * esneme),
                normalOlcek.y * (1f - 0.2f * esneme),
                normalOlcek.z * (1f + 0.1f * esneme));

            if (t >= 1f)
            {
                transform.localScale = normalOlcek;
                durum = Bos;
            }
        }
        else if (durum == Zipliyor)
        {
            float t = Mathf.Clamp01(gecenSure / Ayarlar.ZiplamaSuresi);
            float yukseklik = Mathf.Sin(t * Mathf.PI) * Ayarlar.ZiplamaYuksekligi;
            transform.position = yerKonumu + Vector3.up * yukseklik;

            if (t >= 1f)
            {
                transform.position = yerKonumu;
                durum = Bos;
            }
        }
    }
}

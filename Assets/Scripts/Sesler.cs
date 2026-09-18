using UnityEngine;

// Oyundaki tum sesler. Ses dosyalari Resources/Sesler icinde.
public class Sesler : MonoBehaviour
{
    // Yerlestirme sesinin perdesi puana gore degistigi icin ayri bir kaynak kullaniyor
    AudioSource yerlestirmeKaynagi;
    AudioSource digerKaynak;

    AudioClip yerlestirSesi;
    AudioClip gorevSesi;
    AudioClip kapandiSesi;
    AudioClip oyunSonuSesi;

    public void Kur()
    {
        yerlestirmeKaynagi = gameObject.AddComponent<AudioSource>();
        yerlestirmeKaynagi.playOnAwake = false;

        digerKaynak = gameObject.AddComponent<AudioSource>();
        digerKaynak.playOnAwake = false;

        yerlestirSesi = Resources.Load<AudioClip>("Sesler/yerlestir");
        gorevSesi = Resources.Load<AudioClip>("Sesler/gorev");
        kapandiSesi = Resources.Load<AudioClip>("Sesler/kapandi");
        oyunSonuSesi = Resources.Load<AudioClip>("Sesler/oyunsonu");
    }

    // Cok puan getiren karo daha ince (yuksek) bir sesle oturur
    public void YerlestirmeSesi(int kazanilanPuan)
    {
        yerlestirmeKaynagi.pitch = 1f + Mathf.Min(kazanilanPuan, 6) * Ayarlar.PuanBasinaSesInceligi;
        yerlestirmeKaynagi.PlayOneShot(yerlestirSesi, Ayarlar.SesSeviyesi);
    }

    // Onizleme secilince kucuk, ince bir tik
    public void SecimSesi()
    {
        yerlestirmeKaynagi.pitch = 1.6f;
        yerlestirmeKaynagi.PlayOneShot(yerlestirSesi, Ayarlar.SesSeviyesi * 0.35f);
    }

    public void GorevTamamSesi()
    {
        digerKaynak.PlayOneShot(gorevSesi, Ayarlar.SesSeviyesi);
    }

    public void GorevKapandiSesi()
    {
        digerKaynak.PlayOneShot(kapandiSesi, Ayarlar.SesSeviyesi * 0.6f);
    }

    public void OyunSonuSesiCal()
    {
        digerKaynak.PlayOneShot(oyunSonuSesi, Ayarlar.SesSeviyesi);
    }
}

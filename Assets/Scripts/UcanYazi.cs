using UnityEngine;
using UnityEngine.UI;

// Karo konunca uzerinde beliren "+3" yazisi. Yukari kayar, soner ve kendini siler.
public class UcanYazi : MonoBehaviour
{
    public Vector3 dunyaKonumu;
    public Text yazi;

    public float omur = 1.1f;
    public float yukselmeHizi = 140f; // 1920 yuksekligindeki ekrana gore piksel / saniye

    float gecenSure = 0f;
    Color baslangicRengi;

    void Start()
    {
        baslangicRengi = yazi.color;
        KonumuGuncelle();
    }

    // Kamera Update icinde hareket ettigi icin konumu LateUpdate'te aliyoruz
    void LateUpdate()
    {
        gecenSure += Time.deltaTime;
        KonumuGuncelle();

        // Omrunun yarisindan sonra yavasca kaybolur
        float oran = gecenSure / omur;
        Color renk = baslangicRengi;
        if (oran > 0.5f)
        {
            renk.a = 1f - (oran - 0.5f) * 2f;
        }
        yazi.color = renk;

        if (gecenSure >= omur)
        {
            Destroy(gameObject);
        }
    }

    void KonumuGuncelle()
    {
        Camera kamera = Camera.main;
        if (kamera == null) return;

        Vector3 ekranKonumu = kamera.WorldToScreenPoint(dunyaKonumu);
        float olcek = Screen.height / 1920f;
        ekranKonumu.y += gecenSure * yukselmeHizi * olcek;
        ekranKonumu.z = 0f;
        transform.position = ekranKonumu;
    }
}

using UnityEngine;

// Oyunun denge ve gorunum ayarlari tek yerde.
// Sayilari buradan degistirip Play'de deneyebilirsin.
public static class Ayarlar
{
    // ---------------- DESTE ----------------
    // Oyun basinda destede her turden kac karo olacak
    public static int CimenSayisi = 6;
    public static int OrmanSayisi = 7;
    public static int EvSayisi = 6;
    public static int TarlaSayisi = 6;
    public static int GolSayisi = 5;
    public static int DegirmenSayisi = 3;   // sadece acildiktan sonra desteye girer
    public static int LimanSayisi = 3;      // sadece acildiktan sonra desteye girer

    // ---------------- ACILAN KAROLAR ----------------
    // Butun oyunlardaki puanlarin toplami bu sayiya ulasinca yeni tur acilir
    public static int DegirmenAcilmaPuani = 100;
    public static int LimanAcilmaPuani = 250;

    // Ekranda gosterilen siradaki karo sayisi (ilki yerlestirilecek olan)
    public static int OnizlemeSayisi = 3;

    // ---------------- PUANLAR ----------------
    // Yeni karo konunca her komsusu icin bu puanlar eklenir (iki yonlu gecerli)
    public static int EvTarla = 2;
    public static int EvGol = 1;
    public static int EvEv = 1;
    public static int OrmanOrman = 1;
    public static int GolGol = 1;
    public static int TarlaGol = 1;
    public static int DegirmenTarla = 2;
    public static int DegirmenEv = 1;
    public static int LimanGol = 2;
    public static int LimanEv = 1;

    // ---------------- GOREVLER ----------------
    public static int EnFazlaGorev = 3;          // ayni anda en fazla kac gorev olabilir
    public static float GorevCikmaSansi = 0.3f;  // zaten gorev varken yeni karoya gorev cikma ihtimali
    public static int GorevEnAzArtis = 2;        // grup en az bu kadar buyumeli
    public static int GorevEnFazlaArtis = 4;     // grup en fazla bu kadar buyumeli
    public static int GorevKaroOdulu = 5;        // tamamlaninca desteye eklenen karo
    public static int GorevlerArasiMesafe = 3;   // iki gorevli karo arasinda en az bu kadar hucre olmali

    // ---------------- ANIMASYON VE SES ----------------
    public static float DusmeYuksekligi = 1.6f;   // karo bu yukseklikten dusup yerine oturur
    public static float DusmeSuresi = 0.2f;
    public static float EsnemeSuresi = 0.18f;     // yere carpinca basilip genisleme suresi
    public static float ZiplamaYuksekligi = 0.25f; // puan veren komsularin ziplamasi
    public static float ZiplamaSuresi = 0.25f;
    public static float SesSeviyesi = 0.8f;
    public static float PuanBasinaSesInceligi = 0.06f; // her puan yerlestirme sesini biraz inceltir

    // ---------------- KARO GORUNUMU ----------------
    public static float KaroBoyutu = 1f;         // merkezden koseye mesafe
    public static float KaroBoslukOrani = 0.96f; // karolar arasinda ince bosluk birakir
    public static float YaziYuksekligi = 1.4f;   // "+3" yazisi ve gorev rozetinin karodan yuksekligi
    public static float KaroYuksekligi = 0.3f;
    public static float GolYuksekligi = 0.15f;
    public static float BosYuvaYuksekligi = 0.04f;

    // ---------------- KONTROLLER ----------------
    // true: ilk dokunus karoyu onizler, ayni yere ikinci dokunus koyar. false: tek dokunusta koyar
    public static bool IkiDokunuslaYerlestir = true;
    public static float HayaletYuksekligi = 0.35f;   // onizleme karosu yerden ne kadar yukarida suzulur
    public static float DokunmaEsigiOrani = 0.012f;  // ekran yuksekliginin bu orani kadar kayan parmak surukleme sayilir
    public static float EnYakinZoom = 0.35f;         // 1 = otomatik mesafe, kucuk sayi = daha yakin
    public static float EnUzakZoom = 1.6f;
    public static float FareZoomHizi = 0.001f;       // fare tekerleginin bir tiki genelde 120 birimdir

    // ---------------- KAMERA ----------------
    public static float KameraEgimi = 55f;       // derece, 90 tam tepeden bakar
    public static float KameraGorusAcisi = 40f;
    public static float KameraBoslukCarpani = 1.1f; // buyudukce kasaba ekranda kuculur
    public static float KameraTakipHizi = 3f;
    public static float OyunSonuDonusHizi = 12f; // derece / saniye (menude de ayni hizla doner)
    public static float MenuZoomu = 1.15f;       // menu acikken kamera bu kadar uzaklasir (kasaba bos alana zaten sigdiriliyor)

    // ---------------- RENKLER ----------------
    public static Color ArkaPlanRengi = new Color(0.96f, 0.92f, 0.82f);
    public static Color BosYuvaRengi = new Color(0.88f, 0.82f, 0.70f);
}

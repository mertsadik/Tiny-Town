using UnityEngine;

// Yeni tur eklerken HER ZAMAN sona ekle: kayit dosyasi turleri sayi olarak tutuyor,
// araya eklenirse eski kayitlardaki karolar baska ture donusur.
public enum KaroTuru
{
    Cimen,
    Orman,
    Ev,
    Tarla,
    Gol,
    Degirmen,   // toplam puanla acilir
    Liman       // toplam puanla acilir
}

// Karo turlerinin isim, model, renk ve acilma bilgileri
public static class KaroBilgi
{
    public const int TurSayisi = 7;

    // Her oyunda bastan acik olan turler (Cimen, Orman, Ev, Tarla, Gol)
    public const int TemelTurSayisi = 5;

    public static string Isim(KaroTuru tur)
    {
        if (tur == KaroTuru.Cimen) return "Çimen";
        if (tur == KaroTuru.Orman) return "Orman";
        if (tur == KaroTuru.Ev) return "Ev";
        if (tur == KaroTuru.Tarla) return "Tarla";
        if (tur == KaroTuru.Gol) return "Göl";
        if (tur == KaroTuru.Degirmen) return "Değirmen";
        return "Liman";
    }

    // Resources/Modeller icindeki Kenney Hexagon Kit modelinin adi
    public static string ModelAdi(KaroTuru tur)
    {
        if (tur == KaroTuru.Cimen) return "grass";
        if (tur == KaroTuru.Orman) return "grass-forest";
        if (tur == KaroTuru.Ev) return "building-village";
        if (tur == KaroTuru.Tarla) return "building-farm";
        if (tur == KaroTuru.Gol) return "water";
        if (tur == KaroTuru.Degirmen) return "building-mill";
        return "building-port";
    }

    // Model bulunamazsa kullanilan duz altigenin rengi
    public static Color ZeminRengi(KaroTuru tur)
    {
        if (tur == KaroTuru.Cimen) return new Color(0.33f, 0.80f, 0.64f);
        if (tur == KaroTuru.Orman) return new Color(0.20f, 0.60f, 0.48f);
        if (tur == KaroTuru.Ev) return new Color(0.47f, 0.52f, 0.90f);
        if (tur == KaroTuru.Tarla) return new Color(0.96f, 0.62f, 0.42f);
        if (tur == KaroTuru.Gol) return new Color(0.55f, 0.86f, 0.96f);
        if (tur == KaroTuru.Degirmen) return new Color(0.72f, 0.50f, 0.38f);
        return new Color(0.26f, 0.56f, 0.78f);
    }

    // Arayuzde karoyu temsil eden renk. Kenney modellerindeki baskin renge yakin secildi:
    // cimen turkuaz, orman koyu turkuaz, ev mavi cati, tarla turuncu, gol acik mavi,
    // degirmen kahverengi, liman koyu mavi.
    public static Color ArayuzRengi(KaroTuru tur)
    {
        return ZeminRengi(tur);
    }

    // Bu turun acilmasi icin gereken toplam puan (butun oyunlarin puanlari toplami)
    public static int AcilmaPuani(KaroTuru tur)
    {
        if (tur == KaroTuru.Degirmen) return Ayarlar.DegirmenAcilmaPuani;
        if (tur == KaroTuru.Liman) return Ayarlar.LimanAcilmaPuani;
        return 0;
    }

    public static bool AcikMi(KaroTuru tur)
    {
        return Kayit.ToplamPuan() >= AcilmaPuani(tur);
    }

    // Destede bu turden kac karo olur
    public static int DesteSayisi(KaroTuru tur)
    {
        if (tur == KaroTuru.Cimen) return Ayarlar.CimenSayisi;
        if (tur == KaroTuru.Orman) return Ayarlar.OrmanSayisi;
        if (tur == KaroTuru.Ev) return Ayarlar.EvSayisi;
        if (tur == KaroTuru.Tarla) return Ayarlar.TarlaSayisi;
        if (tur == KaroTuru.Gol) return Ayarlar.GolSayisi;
        if (tur == KaroTuru.Degirmen) return Ayarlar.DegirmenSayisi;
        return Ayarlar.LimanSayisi;
    }
}

using System.Collections.Generic;
using UnityEngine;

// Puan hesaplari
public static class Puanlama
{
    // Iki komsu karo yan yana gelince kac puan verir
    public static int KomsuPuani(KaroTuru a, KaroTuru b)
    {
        if (IkiliMi(a, b, KaroTuru.Ev, KaroTuru.Tarla)) return Ayarlar.EvTarla;
        if (IkiliMi(a, b, KaroTuru.Ev, KaroTuru.Gol)) return Ayarlar.EvGol;
        if (IkiliMi(a, b, KaroTuru.Ev, KaroTuru.Ev)) return Ayarlar.EvEv;
        if (IkiliMi(a, b, KaroTuru.Orman, KaroTuru.Orman)) return Ayarlar.OrmanOrman;
        if (IkiliMi(a, b, KaroTuru.Gol, KaroTuru.Gol)) return Ayarlar.GolGol;
        if (IkiliMi(a, b, KaroTuru.Tarla, KaroTuru.Gol)) return Ayarlar.TarlaGol;
        if (IkiliMi(a, b, KaroTuru.Degirmen, KaroTuru.Tarla)) return Ayarlar.DegirmenTarla;
        if (IkiliMi(a, b, KaroTuru.Degirmen, KaroTuru.Ev)) return Ayarlar.DegirmenEv;
        if (IkiliMi(a, b, KaroTuru.Liman, KaroTuru.Gol)) return Ayarlar.LimanGol;
        if (IkiliMi(a, b, KaroTuru.Liman, KaroTuru.Ev)) return Ayarlar.LimanEv;
        return 0;
    }

    // Sira fark etmeksizin (a, b) ikilisi (x, y) ikilisine esit mi
    static bool IkiliMi(KaroTuru a, KaroTuru b, KaroTuru x, KaroTuru y)
    {
        return (a == x && b == y) || (a == y && b == x);
    }

    // Bir karo bu hucreye konulursa kac puan kazanilir
    public static int YerlestirmePuani(Dictionary<Vector2Int, KaroTuru> harita, Vector2Int hucre, KaroTuru tur)
    {
        int toplam = 0;
        for (int i = 0; i < 6; i++)
        {
            Vector2Int komsu = HexGrid.Komsu(hucre, i);
            if (harita.ContainsKey(komsu))
            {
                toplam += KomsuPuani(tur, harita[komsu]);
            }
        }
        return toplam;
    }

    // Hucre bos mu ve en az bir dolu komsusu var mi
    public static bool YerlestirilebilirMi(Dictionary<Vector2Int, KaroTuru> harita, Vector2Int hucre)
    {
        if (harita.ContainsKey(hucre)) return false;

        for (int i = 0; i < 6; i++)
        {
            if (harita.ContainsKey(HexGrid.Komsu(hucre, i))) return true;
        }
        return false;
    }
}

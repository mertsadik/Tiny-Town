using UnityEngine;

// Altigen izgara hesaplari.
// Sivri tepeli (pointy-top) altigenler ve axial koordinat (q, r) kullaniliyor.
// Vector2Int icinde x = q, y = r olarak tutuluyor.
// Kaynak: https://www.redblobgames.com/grids/hexagons/
public static class HexGrid
{
    static float kok3 = Mathf.Sqrt(3f);

    // 6 komsunun koordinat farklari
    public static Vector2Int[] KomsuYonleri = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };

    public static Vector2Int Komsu(Vector2Int hucre, int yon)
    {
        return hucre + KomsuYonleri[yon];
    }

    // Iki hucre arasindaki adim sayisi (komsu = 1)
    public static int Mesafe(Vector2Int a, Vector2Int b)
    {
        int dq = a.x - b.x;
        int dr = a.y - b.y;
        return (Mathf.Abs(dq) + Mathf.Abs(dr) + Mathf.Abs(dq + dr)) / 2;
    }

    // Hucre koordinatini dunya konumuna cevirir (y = 0)
    public static Vector3 DunyaKonumu(Vector2Int hucre)
    {
        float boyut = Ayarlar.KaroBoyutu;
        float x = boyut * kok3 * (hucre.x + hucre.y / 2f);
        float z = boyut * 1.5f * hucre.y;
        return new Vector3(x, 0f, z);
    }

    // Dunya konumunun hangi hucreye dustugunu bulur
    public static Vector2Int HucreBul(Vector3 konum)
    {
        float boyut = Ayarlar.KaroBoyutu;
        float q = (kok3 / 3f * konum.x - 1f / 3f * konum.z) / boyut;
        float r = (2f / 3f * konum.z) / boyut;
        return Yuvarla(q, r);
    }

    // Ondalikli koordinati en yakin hucreye yuvarlar (cube koordinat yontemi)
    static Vector2Int Yuvarla(float q, float r)
    {
        float s = -q - r;

        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float qFark = Mathf.Abs(rq - q);
        float rFark = Mathf.Abs(rr - r);
        float sFark = Mathf.Abs(rs - s);

        // En cok yuvarlanan eksen digerlerinden yeniden hesaplanir
        if (qFark > rFark && qFark > sFark)
        {
            rq = -rr - rs;
        }
        else if (rFark > sFark)
        {
            rr = -rq - rs;
        }

        return new Vector2Int(rq, rr);
    }

    // Altigen prizma mesh'i uretir. Tabani y = 0'da, ustu y = yukseklik'te.
    // Her yuzeyin kendi kose noktalari var, boylece kenarlar keskin gorunur.
    public static Mesh AltigenMeshOlustur(float yaricap, float yukseklik)
    {
        Vector3[] koseler = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            float aci = (60f * i + 30f) * Mathf.Deg2Rad;
            koseler[i] = new Vector3(Mathf.Cos(aci) * yaricap, 0f, Mathf.Sin(aci) * yaricap);
        }

        // Ust yuz: 7 nokta, yan yuzler: 6 x 4 nokta
        Vector3[] noktalar = new Vector3[7 + 24];
        int[] ucgenler = new int[6 * 3 + 6 * 6];

        Vector3 yukari = new Vector3(0f, yukseklik, 0f);

        // Ust yuz
        noktalar[0] = yukari;
        for (int i = 0; i < 6; i++)
        {
            noktalar[1 + i] = koseler[i] + yukari;
        }
        int u = 0;
        for (int i = 0; i < 6; i++)
        {
            int sonraki = (i + 1) % 6;
            ucgenler[u++] = 0;
            ucgenler[u++] = 1 + sonraki;
            ucgenler[u++] = 1 + i;
        }

        // Yan yuzler
        for (int i = 0; i < 6; i++)
        {
            int sonraki = (i + 1) % 6;
            int n = 7 + i * 4;
            noktalar[n] = koseler[i];                   // alt i
            noktalar[n + 1] = koseler[i] + yukari;      // ust i
            noktalar[n + 2] = koseler[sonraki] + yukari; // ust sonraki
            noktalar[n + 3] = koseler[sonraki];         // alt sonraki

            ucgenler[u++] = n;
            ucgenler[u++] = n + 1;
            ucgenler[u++] = n + 2;

            ucgenler[u++] = n;
            ucgenler[u++] = n + 2;
            ucgenler[u++] = n + 3;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Altigen";
        mesh.vertices = noktalar;
        mesh.triangles = ucgenler;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}

using UnityEngine;

// Kart ekrana sigmiyorsa (kisa ekranli telefon, tablet, editorde yatay Game view)
// karti oranini bozmadan kucultur. Sigiyorsa dokunmaz.
public class EkranaSigdir : MonoBehaviour
{
    public float ustBosluk = 40f;  // kartin ustunde bos kalmasi gereken alan (ornek: menudeki baslik)
    public float altBosluk = 40f;
    public float yanBosluk = 20f;

    RectTransform kart;

    void Awake()
    {
        kart = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        RectTransform ebeveyn = transform.parent as RectTransform;
        if (ebeveyn == null) return;

        float kartYuksekligi = kart.rect.height;
        float kartGenisligi = kart.rect.width;
        if (kartYuksekligi <= 0f || kartGenisligi <= 0f) return;

        float yukseklikOrani = (ebeveyn.rect.height - ustBosluk - altBosluk) / kartYuksekligi;
        float genislikOrani = (ebeveyn.rect.width - 2f * yanBosluk) / kartGenisligi;
        float olcek = Mathf.Min(1f, yukseklikOrani, genislikOrani);

        // Cok kucuk ekranlarda bile okunamayacak kadar kuculmesin
        olcek = Mathf.Max(0.4f, olcek);
        kart.localScale = new Vector3(olcek, olcek, 1f);
    }
}

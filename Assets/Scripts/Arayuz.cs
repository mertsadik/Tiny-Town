using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Ekrandaki tum arayuz koddan kuruluyor: puan, siradaki karolar, kurallar ve oyun sonu paneli.
public class Arayuz : MonoBehaviour
{
    KasabaOyunu oyun;

    Font font;
    Sprite yuvarlakSprite;
    Transform canvas;
    Transform oyunEkrani;
    Transform gorevKoku;

    GameObject menuPaneli;
    Text menuAnaButonYazisi;
    GameObject yeniOyunButonu;
    Text menuBilgiYazisi;
    Text sesButonYazisi;
    RectTransform menuKarti;
    RectTransform menuKurallarButonu;
    RectTransform menuSesButonu;
    RectTransform menuGunlukButonu;
    Text menuIlerlemeYazisi;
    Text menuGunlukYazisi;
    Text modYazisi;
    Text oyunSonuAcilanYazisi;

    // Kameranin kasabayi yerlestirecegi bos alani bulmak icin
    RectTransform altPanel;
    RectTransform oyunSonuKarti;
    RectTransform menuAltBaslik;

    Text onizlemeYazisi;
    DunyaTakip onizlemeTakip;
    Text ipucuYazisi;

    Text puanYazisi;
    int sonPuan = 0;
    Text kalanYazisi;

    Image[] onizlemeKutulari;
    Text[] onizlemeYazilari;

    GameObject kurallarPaneli;
    GameObject onayPaneli;
    Text onayYazisi;
    GameObject oyunSonuPaneli;
    Text oyunSonuPuanYazisi;
    Text oyunSonuRekorYazisi;

    Color yaziRengi = new Color(0.20f, 0.25f, 0.30f);
    Color kartRengi = new Color(1f, 1f, 1f, 0.92f);
    Color butonRengi = new Color(0.95f, 0.60f, 0.25f);
    Color ikinciButonRengi = new Color(0.45f, 0.58f, 0.68f);

    public void Kur(KasabaOyunu oyunScripti)
    {
        oyun = oyunScripti;
        font = Resources.Load<Font>("Fonts/Poppins-Bold");
        // Beyaz yuvarlak koseli panel (Inspector'da 9 dilimli sprite olarak ayarli)
        yuvarlakSprite = Resources.Load<Sprite>("UI/panel");

        CanvasKur();

        // Oyun sirasinda gorunen her sey bu objenin altinda; menu acilinca hepsi birden gizlenir
        oyunEkrani = TamEkranObjesiOlustur("OyunEkrani", canvas).transform;
        oyunEkrani.gameObject.AddComponent<GuvenliAlan>();

        // Rozetler en altta cizilsin, paneller onlarin ustunde kalsin
        gorevKoku = new GameObject("Gorevler", typeof(RectTransform)).transform;
        gorevKoku.SetParent(oyunEkrani, false);

        OnizlemeYazisiKur();

        UstKismiKur();
        AltPaneliKur();
        MenuKur();
        KurallarPaneliKur();
        OnayPaneliKur();
        OyunSonuPaneliKur();
    }

    // ---------------- KURULUM ----------------

    void CanvasKur()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas c = canvasObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler olcekleyici = canvasObj.AddComponent<CanvasScaler>();
        olcekleyici.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        olcekleyici.referenceResolution = new Vector2(1080, 1920);
        olcekleyici.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvas = canvasObj.transform;

        // Butonlarin calismasi icin EventSystem (yeni Input System modulu ile)
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }
    }

    // Hayalet karonun ustunde duran "+3" yazisi. Tek tane var, gerektikce gosterilip gizleniyor.
    void OnizlemeYazisiKur()
    {
        onizlemeYazisi = YaziOlustur(oyunEkrani, "", 70, Color.white);
        onizlemeYazisi.rectTransform.pivot = new Vector2(0.5f, 0f);
        onizlemeYazisi.rectTransform.sizeDelta = new Vector2(500, 150);
        onizlemeYazisi.alignment = TextAnchor.LowerCenter;

        Outline kenar = onizlemeYazisi.gameObject.AddComponent<Outline>();
        kenar.effectColor = new Color(0.15f, 0.2f, 0.25f, 0.8f);
        kenar.effectDistance = new Vector2(3, -3);

        onizlemeTakip = onizlemeYazisi.gameObject.AddComponent<DunyaTakip>();
        onizlemeYazisi.gameObject.SetActive(false);

        // Ilk karolarda alt panelin hemen ustunde duran ipucu
        ipucuYazisi = YaziOlustur(oyunEkrani, "Koymak için aynı yere tekrar dokun", 40, Color.white);
        Yerlestir(ipucuYazisi.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 460), new Vector2(1000, 70));
        Outline ipucuKenar = ipucuYazisi.gameObject.AddComponent<Outline>();
        ipucuKenar.effectColor = new Color(0.15f, 0.2f, 0.25f, 0.8f);
        ipucuKenar.effectDistance = new Vector2(3, -3);
        ipucuYazisi.gameObject.SetActive(false);
    }

    void UstKismiKur()
    {
        Text baslik = YaziOlustur(oyunEkrani, "PUAN", 40, yaziRengi);
        Yerlestir(baslik.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(400, 60));

        puanYazisi = YaziOlustur(oyunEkrani, "0", 110, yaziRengi);
        Yerlestir(puanYazisi.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -115), new Vector2(600, 140));

        modYazisi = YaziOlustur(oyunEkrani, "", 34, new Color(0.30f, 0.62f, 0.50f));
        Yerlestir(modYazisi.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -255), new Vector2(700, 50));

        Button yardimButonu = ButonOlustur(oyunEkrani, "?", 70, new Vector2(1f, 1f), new Vector2(-50, -50), new Vector2(120, 120));
        yardimButonu.onClick.AddListener(KurallariAc);

        Button menuButonu = ButonOlustur(oyunEkrani, "MENÜ", 40, new Vector2(0f, 1f), new Vector2(50, -50), new Vector2(200, 120));
        menuButonu.onClick.AddListener(oyun.MenuyeDon);
    }

    void AltPaneliKur()
    {
        Image panel = ResimOlustur(oyunEkrani, kartRengi);
        altPanel = panel.rectTransform;
        RectTransform rt = panel.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 30);
        rt.sizeDelta = new Vector2(-60, 400);

        Text baslik = YaziOlustur(panel.transform, "SIRADAKİ", 38, yaziRengi);
        baslik.alignment = TextAnchor.UpperLeft;
        Yerlestir(baslik.rectTransform, new Vector2(0f, 1f), new Vector2(40, -25), new Vector2(400, 60));

        kalanYazisi = YaziOlustur(panel.transform, "", 38, yaziRengi);
        kalanYazisi.alignment = TextAnchor.UpperRight;
        Yerlestir(kalanYazisi.rectTransform, new Vector2(1f, 1f), new Vector2(-40, -25), new Vector2(400, 60));

        onizlemeKutulari = new Image[Ayarlar.OnizlemeSayisi];
        onizlemeYazilari = new Text[Ayarlar.OnizlemeSayisi];

        for (int i = 0; i < Ayarlar.OnizlemeSayisi; i++)
        {
            // Ilk kutu (simdi konulacak karo) buyuk, digerleri kucuk ve yan yana
            float boyut = 140f;
            float x = -30f + (i - 1) * 190f;
            float y = -40f;
            int yaziBoyutu = 30;
            if (i == 0)
            {
                boyut = 210f;
                x = -250f;
                y = -5f;
                yaziBoyutu = 40;
            }

            Image kutu = ResimOlustur(panel.transform, Color.white);
            Yerlestir(kutu.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(boyut, boyut));
            onizlemeKutulari[i] = kutu;

            Text isim = YaziOlustur(panel.transform, "", yaziBoyutu, yaziRengi);
            Yerlestir(isim.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, y - boyut / 2f - 30f), new Vector2(260, 60));
            onizlemeYazilari[i] = isim;
        }
    }

    void KurallarPaneliKur()
    {
        kurallarPaneli = TamEkranKaplamaOlustur();

        Image kart = ResimOlustur(kurallarPaneli.transform, Color.white);
        Yerlestir(kart.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920, 1700));
        kart.gameObject.AddComponent<EkranaSigdir>(); // tablette ya da kisa ekranda TAMAM disarida kalmasin

        Text baslik = YaziOlustur(kart.transform, "NASIL OYNANIR?", 56, yaziRengi);
        Yerlestir(baslik.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(800, 90));

        string metin =
            "Açık gri bir yuvaya dokun, karo orada önizlenir. Koymak için aynı yere tekrar dokun.\n" +
            "Parmağınla kaydır, iki parmakla yakınlaştır.\n\n" +
            "Karo, yanındaki komşulara göre puan kazanır:\n\n" +
            "Ev + Tarla   +" + Ayarlar.EvTarla + "\n" +
            "Ev + Göl   +" + Ayarlar.EvGol + "\n" +
            "Ev + Ev   +" + Ayarlar.EvEv + "\n" +
            "Orman + Orman   +" + Ayarlar.OrmanOrman + "\n" +
            "Göl + Göl   +" + Ayarlar.GolGol + "\n" +
            "Tarla + Göl   +" + Ayarlar.TarlaGol + "\n" +
            AcilanTurKurallari() + "\n" +
            "Bazı karoların üstünde görev çıkar (ör. 3/6). O türden bitişik grubu hedefe kadar büyütürsen destene +" +
            Ayarlar.GorevKaroOdulu + " karo eklenir.\n\n" +
            "Karolar bitince kasaban tamamlanır.";

        Text kurallar = YaziOlustur(kart.transform, metin, 33, yaziRengi);
        kurallar.alignment = TextAnchor.UpperCenter;
        Yerlestir(kurallar.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -160), new Vector2(800, 1000));

        Button tamam = ButonOlustur(kart.transform, "TAMAM", 48, new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(400, 130));
        tamam.onClick.AddListener(KurallariKapat);

        kurallarPaneli.SetActive(false);
    }

    // "Emin misin?" penceresi. EVET'e basilinca oyun.OnayVerildi cagrilir; neyin onaylandigini oyun biliyor.
    void OnayPaneliKur()
    {
        onayPaneli = TamEkranKaplamaOlustur();

        Image kart = ResimOlustur(onayPaneli.transform, Color.white);
        Yerlestir(kart.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880, 520));
        kart.gameObject.AddComponent<EkranaSigdir>();

        Text baslik = YaziOlustur(kart.transform, "EMİN MİSİN?", 56, yaziRengi);
        Yerlestir(baslik.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(800, 80));

        onayYazisi = YaziOlustur(kart.transform, "", 38, yaziRengi);
        Yerlestir(onayYazisi.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(780, 160));

        Button vazgec = ButonOlustur(kart.transform, "VAZGEÇ", 46, new Vector2(0.5f, 0f), new Vector2(-190, 50), new Vector2(340, 130));
        vazgec.GetComponent<Image>().color = ikinciButonRengi;
        vazgec.onClick.AddListener(OnayiKapat);

        Button evet = ButonOlustur(kart.transform, "EVET", 46, new Vector2(0.5f, 0f), new Vector2(190, 50), new Vector2(340, 130));
        evet.onClick.AddListener(oyun.OnayVerildi);

        onayPaneli.SetActive(false);
    }

    public void OnayGoster(string mesaj)
    {
        onayYazisi.text = mesaj;
        onayPaneli.SetActive(true);
    }

    public void OnayiKapat()
    {
        onayPaneli.SetActive(false);
    }

    // Acilmis yeni turlerin puan satirlari (kilitliyse hic gorunmez, surpriz kalsin)
    string AcilanTurKurallari()
    {
        string metin = "";
        if (KaroBilgi.AcikMi(KaroTuru.Degirmen))
        {
            metin += "Değirmen + Tarla   +" + Ayarlar.DegirmenTarla + "\n";
            metin += "Değirmen + Ev   +" + Ayarlar.DegirmenEv + "\n";
        }
        if (KaroBilgi.AcikMi(KaroTuru.Liman))
        {
            metin += "Liman + Göl   +" + Ayarlar.LimanGol + "\n";
            metin += "Liman + Ev   +" + Ayarlar.LimanEv + "\n";
        }
        return metin;
    }

    void OyunSonuPaneliKur()
    {
        oyunSonuPaneli = new GameObject("OyunSonu", typeof(RectTransform));
        oyunSonuPaneli.transform.SetParent(canvas, false);
        RectTransform panelRt = oyunSonuPaneli.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;
        oyunSonuPaneli.AddComponent<GuvenliAlan>();

        // Kasaba gorunsun diye kart asagida duruyor, ekrani kaplamiyor
        Image kart = ResimOlustur(oyunSonuPaneli.transform, Color.white);
        oyunSonuKarti = kart.rectTransform;
        RectTransform kartRt = kart.rectTransform;
        kartRt.anchorMin = new Vector2(0f, 0f);
        kartRt.anchorMax = new Vector2(1f, 0f);
        kartRt.pivot = new Vector2(0.5f, 0f);
        kartRt.anchoredPosition = new Vector2(0, 30);
        kartRt.sizeDelta = new Vector2(-60, 600);

        Text baslik = YaziOlustur(kart.transform, "KASABAN TAMAMLANDI!", 54, yaziRengi);
        Yerlestir(baslik.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(950, 80));

        oyunSonuPuanYazisi = YaziOlustur(kart.transform, "", 90, yaziRengi);
        Yerlestir(oyunSonuPuanYazisi.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -125), new Vector2(900, 120));

        oyunSonuRekorYazisi = YaziOlustur(kart.transform, "", 40, yaziRengi);
        Yerlestir(oyunSonuRekorYazisi.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -250), new Vector2(900, 60));

        // Bu oyunla yeni karo acildiysa burada yazar
        oyunSonuAcilanYazisi = YaziOlustur(kart.transform, "", 40, butonRengi);
        Yerlestir(oyunSonuAcilanYazisi.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -310), new Vector2(900, 60));

        Button tekrar = ButonOlustur(kart.transform, "TEKRAR OYNA", 50, new Vector2(0.5f, 0f), new Vector2(-150, 50), new Vector2(460, 140));
        tekrar.onClick.AddListener(oyun.TekrarOyna);

        Button menu = ButonOlustur(kart.transform, "MENÜ", 50, new Vector2(0.5f, 0f), new Vector2(250, 50), new Vector2(300, 140));
        menu.GetComponent<Image>().color = ikinciButonRengi;
        menu.onClick.AddListener(oyun.MenuyeDonOyunSonu);

        oyunSonuPaneli.SetActive(false);
    }

    // Ana menu: ustte oyunun adi, altta butonlar. Arkada kasaba yavasca doner.
    void MenuKur()
    {
        menuPaneli = TamEkranObjesiOlustur("Menu", canvas);
        menuPaneli.AddComponent<GuvenliAlan>();

        Text baslik = YaziOlustur(menuPaneli.transform, "Mini Kasaba", 130, yaziRengi);
        Yerlestir(baslik.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -170), new Vector2(1000, 170));

        Text altBaslik = YaziOlustur(menuPaneli.transform, "Karo koy, kasabanı büyüt", 44, yaziRengi);
        menuAltBaslik = altBaslik.rectTransform;
        Yerlestir(altBaslik.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -330), new Vector2(1000, 70));

        Image kart = ResimOlustur(menuPaneli.transform, kartRengi);
        RectTransform kartRt = kart.rectTransform;
        kartRt.anchorMin = new Vector2(0f, 0f);
        kartRt.anchorMax = new Vector2(1f, 0f);
        kartRt.pivot = new Vector2(0.5f, 0f);
        kartRt.anchoredPosition = new Vector2(0, 30);
        kartRt.sizeDelta = new Vector2(-60, 620);
        menuKarti = kartRt;

        // Kisa ekranda kart basligin ustune binmesin
        EkranaSigdir menuSigdir = kart.gameObject.AddComponent<EkranaSigdir>();
        menuSigdir.ustBosluk = 430f;
        menuSigdir.altBosluk = 30f;

        // Butonlarin dikey konumlari MenuGoster'da ayarlaniyor (kayit olup olmamasina gore degisir)
        menuBilgiYazisi = YaziOlustur(kart.transform, "", 38, yaziRengi);
        Yerlestir(menuBilgiYazisi.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(900, 60));

        menuIlerlemeYazisi = YaziOlustur(kart.transform, "", 32, new Color(0.45f, 0.5f, 0.55f));
        Yerlestir(menuIlerlemeYazisi.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(900, 50));

        Button anaButon = ButonOlustur(kart.transform, "OYNA", 60, new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(640, 160));
        anaButon.onClick.AddListener(oyun.MenudenBasla);
        menuAnaButonYazisi = anaButon.GetComponentInChildren<Text>();

        Button yeniOyun = ButonOlustur(kart.transform, "YENİ OYUN", 44, new Vector2(0.5f, 1f), new Vector2(0, -335), new Vector2(640, 115));
        yeniOyun.GetComponent<Image>().color = ikinciButonRengi;
        yeniOyun.onClick.AddListener(oyun.YeniOyunButonunaBasildi);
        yeniOyunButonu = yeniOyun.gameObject;

        Button gunluk = ButonOlustur(kart.transform, "GÜNÜN KASABASI", 44, new Vector2(0.5f, 1f), new Vector2(0, -470), new Vector2(640, 115));
        gunluk.GetComponent<Image>().color = new Color(0.30f, 0.68f, 0.55f);
        gunluk.onClick.AddListener(oyun.GunlukButonunaBasildi);
        menuGunlukButonu = gunluk.GetComponent<RectTransform>();

        menuGunlukYazisi = YaziOlustur(kart.transform, "", 30, new Color(0.45f, 0.5f, 0.55f));
        Yerlestir(menuGunlukYazisi.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -590), new Vector2(900, 45));

        Button kurallar = ButonOlustur(kart.transform, "NASIL OYNANIR", 34, new Vector2(0.5f, 1f), new Vector2(-165, -450), new Vector2(310, 115));
        kurallar.GetComponent<Image>().color = ikinciButonRengi;
        kurallar.onClick.AddListener(KurallariAc);
        menuKurallarButonu = kurallar.GetComponent<RectTransform>();

        Button ses = ButonOlustur(kart.transform, "", 34, new Vector2(0.5f, 1f), new Vector2(165, -450), new Vector2(310, 115));
        ses.GetComponent<Image>().color = ikinciButonRengi;
        ses.onClick.AddListener(oyun.SesiAcKapat);
        sesButonYazisi = ses.GetComponentInChildren<Text>();
        menuSesButonu = ses.GetComponent<RectTransform>();

        menuPaneli.SetActive(false);
    }

    // ---------------- OYUNUN CAGIRDIGI FONKSIYONLAR ----------------

    // Puan artinca puan yazisi bir an buyuyup eski boyutuna doner
    void Update()
    {
        if (puanYazisi == null) return;
        puanYazisi.transform.localScale = Vector3.Lerp(puanYazisi.transform.localScale, Vector3.one, 10f * Time.deltaTime);
    }

    public void Guncelle(int puan, List<KaroTuru> deste)
    {
        if (puan > sonPuan)
        {
            puanYazisi.transform.localScale = Vector3.one * 1.35f;
        }
        sonPuan = puan;
        puanYazisi.text = puan.ToString();
        kalanYazisi.text = "Kalan: " + deste.Count;

        for (int i = 0; i < onizlemeKutulari.Length; i++)
        {
            bool varMi = i < deste.Count;
            onizlemeKutulari[i].gameObject.SetActive(varMi);
            onizlemeYazilari[i].gameObject.SetActive(varMi);

            if (varMi)
            {
                onizlemeKutulari[i].color = KaroBilgi.ArayuzRengi(deste[i]);
                onizlemeYazilari[i].text = KaroBilgi.Isim(deste[i]);
            }
        }
    }

    public void MenuGoster(bool kayitVar, string bilgi, string ilerleme, string gunlukBilgi)
    {
        if (kayitVar)
        {
            menuAnaButonYazisi.text = "DEVAM ET";
        }
        else
        {
            menuAnaButonYazisi.text = "OYNA";
        }
        yeniOyunButonu.SetActive(kayitVar);

        // Butonlari yukaridan asagi sirayla diz; "Yeni Oyun" yoksa digerleri yukari kayar
        float y = -335f; // ana butonun hemen alti
        if (kayitVar)
        {
            y -= 135f;
        }
        menuGunlukButonu.anchoredPosition = new Vector2(0, y);
        y -= 120f;
        menuGunlukYazisi.rectTransform.anchoredPosition = new Vector2(0, y);
        y -= 60f;
        menuKurallarButonu.anchoredPosition = new Vector2(-165, y);
        menuSesButonu.anchoredPosition = new Vector2(165, y);
        y -= 145f;
        menuKarti.sizeDelta = new Vector2(-60, -y);

        menuBilgiYazisi.text = bilgi;
        menuIlerlemeYazisi.text = ilerleme;
        menuGunlukYazisi.text = gunlukBilgi;

        menuPaneli.SetActive(true);
        oyunEkrani.gameObject.SetActive(false);
        oyunSonuPaneli.SetActive(false);
    }

    public void MenuGizle()
    {
        menuPaneli.SetActive(false);
        oyunEkrani.gameObject.SetActive(true);
    }

    public void SesYazisiGuncelle(bool sesAcik)
    {
        if (sesAcik)
        {
            sesButonYazisi.text = "SES: AÇIK";
        }
        else
        {
            sesButonYazisi.text = "SES: KAPALI";
        }
    }

    public void UcanYaziGoster(Vector3 dunyaKonumu, string metin)
    {
        UcanYaziGoster(dunyaKonumu, metin, Color.white);
    }

    public void UcanYaziGoster(Vector3 dunyaKonumu, string metin, Color renk)
    {
        // Oyun ekraninin altinda: menu acilinca ucan yazilar da gizlenir
        Text yazi = YaziOlustur(oyunEkrani, metin, 64, renk);
        yazi.rectTransform.sizeDelta = new Vector2(600, 90);

        // Acik renk zeminlerde okunsun diye koyu kenar
        Outline kenar = yazi.gameObject.AddComponent<Outline>();
        kenar.effectColor = new Color(0.15f, 0.2f, 0.25f, 0.8f);
        kenar.effectDistance = new Vector2(3, -3);

        UcanYazi ucan = yazi.gameObject.AddComponent<UcanYazi>();
        ucan.dunyaKonumu = dunyaKonumu;
        ucan.yazi = yazi;
    }

    // Gorevli karonun ustundeki rozet: tur renginde nokta + "3/6" yazisi
    public GorevIsareti GorevIsaretiOlustur(Vector3 dunyaKonumu, KaroTuru tur)
    {
        Image rozet = ResimOlustur(gorevKoku, Color.white);
        rozet.raycastTarget = false; // alttaki karoya dokunmayi engellemesin
        RectTransform rt = rozet.rectTransform;
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(136, 60);

        Image nokta = ResimOlustur(rozet.transform, KaroBilgi.ArayuzRengi(tur));
        nokta.raycastTarget = false;
        Yerlestir(nokta.rectTransform, new Vector2(0f, 0.5f), new Vector2(12, 0), new Vector2(34, 34));

        Text yazi = YaziOlustur(rozet.transform, "", 34, yaziRengi);
        Yerlestir(yazi.rectTransform, new Vector2(1f, 0.5f), new Vector2(-8, 0), new Vector2(86, 56));

        GorevIsareti isaret = rozet.gameObject.AddComponent<GorevIsareti>();
        isaret.dunyaKonumu = dunyaKonumu;
        isaret.yazi = yazi;
        return isaret;
    }

    // Kasabanin gorunecegi bos alanin alt siniri (0 = ekranin alti, 1 = ustu).
    // Alttaki panel ya da kartin ust kenari.
    public float BosAlanAlti()
    {
        RectTransform altSinir = altPanel;
        if (menuPaneli.activeSelf)
        {
            altSinir = menuKarti;
        }
        if (oyunSonuPaneli.activeSelf)
        {
            altSinir = oyunSonuKarti;
        }

        // GetWorldCorners: 0 sol alt, 1 sol ust kose. Overlay canvas'ta bu degerler ekran pikseli.
        Vector3[] koseler = new Vector3[4];
        altSinir.GetWorldCorners(koseler);
        return koseler[1].y / Screen.height;
    }

    // Kasabanin gorunecegi bos alanin ust siniri: basligin ya da puan yazisinin alt kenari
    public float BosAlanUstu()
    {
        RectTransform ustSinir = puanYazisi.rectTransform;
        if (modYazisi.text != "")
        {
            ustSinir = modYazisi.rectTransform; // gunun kasabasi yazisi puanin altinda
        }
        if (menuPaneli.activeSelf)
        {
            ustSinir = menuAltBaslik;
        }
        if (oyunSonuPaneli.activeSelf)
        {
            ustSinir = puanYazisi.rectTransform;
        }

        Vector3[] koseler = new Vector3[4];
        ustSinir.GetWorldCorners(koseler);
        return koseler[0].y / Screen.height;
    }

    // Puanin altinda oyun modunu yazar (ornek: "GUNUN KASABASI"); bos metin verilirse gorunmez
    public void ModYazisiAyarla(string metin)
    {
        modYazisi.text = metin;
    }

    public void OnizlemeYazisiGoster(Vector3 dunyaKonumu, string metin, bool ipucuGoster)
    {
        onizlemeYazisi.text = metin;
        onizlemeTakip.dunyaKonumu = dunyaKonumu;
        onizlemeYazisi.gameObject.SetActive(true);
        ipucuYazisi.gameObject.SetActive(ipucuGoster);
    }

    public void OnizlemeYazisiGizle()
    {
        onizlemeYazisi.gameObject.SetActive(false);
        ipucuYazisi.gameObject.SetActive(false);
    }

    public void OyunSonuGoster(int puan, string rekorYazisi, bool yeniRekor, string acilanYazisi)
    {
        oyunSonuPuanYazisi.text = puan + " puan";
        oyunSonuAcilanYazisi.text = acilanYazisi;
        if (yeniRekor)
        {
            oyunSonuRekorYazisi.text = "YENİ REKOR!";
        }
        else
        {
            oyunSonuRekorYazisi.text = rekorYazisi;
        }
        oyunSonuPaneli.SetActive(true);
    }

    public void KurallariAc()
    {
        kurallarPaneli.SetActive(true);
    }

    public void KurallariKapat()
    {
        kurallarPaneli.SetActive(false);
    }

    // Ekrandaki bu nokta bir arayuz elemaninin uzerinde mi
    public bool UIUzerindeMi(Vector2 ekranKonumu)
    {
        if (EventSystem.current == null) return false;

        PointerEventData veri = new PointerEventData(EventSystem.current);
        veri.position = ekranKonumu;
        List<RaycastResult> sonuclar = new List<RaycastResult>();
        EventSystem.current.RaycastAll(veri, sonuclar);
        return sonuclar.Count > 0;
    }

    // ---------------- YARDIMCI FONKSIYONLAR ----------------


    Text YaziOlustur(Transform ebeveyn, string metin, int boyut, Color renk)
    {
        GameObject obj = new GameObject("Yazi", typeof(RectTransform));
        obj.transform.SetParent(ebeveyn, false);
        Text yazi = obj.AddComponent<Text>();
        yazi.font = font;
        yazi.text = metin;
        yazi.fontSize = boyut;
        yazi.color = renk;
        yazi.alignment = TextAnchor.MiddleCenter;
        yazi.horizontalOverflow = HorizontalWrapMode.Wrap;
        yazi.verticalOverflow = VerticalWrapMode.Overflow;
        yazi.raycastTarget = false; // yazilar dokunmayi engellemesin
        return yazi;
    }

    Image ResimOlustur(Transform ebeveyn, Color renk)
    {
        GameObject obj = new GameObject("Resim", typeof(RectTransform));
        obj.transform.SetParent(ebeveyn, false);
        Image resim = obj.AddComponent<Image>();
        resim.sprite = yuvarlakSprite;
        resim.type = Image.Type.Sliced;
        resim.color = renk;
        return resim;
    }

    Button ButonOlustur(Transform ebeveyn, string metin, int yaziBoyutu, Vector2 capa, Vector2 konum, Vector2 boyut)
    {
        Image arkaPlan = ResimOlustur(ebeveyn, butonRengi);
        Yerlestir(arkaPlan.rectTransform, capa, konum, boyut);
        Button buton = arkaPlan.gameObject.AddComponent<Button>();

        Text yazi = YaziOlustur(arkaPlan.transform, metin, yaziBoyutu, Color.white);
        yazi.rectTransform.anchorMin = Vector2.zero;
        yazi.rectTransform.anchorMax = Vector2.one;
        yazi.rectTransform.sizeDelta = Vector2.zero;
        return buton;
    }

    // Resmi olmayan, tum ekrani kaplayan bos obje; baska arayuz elemanlarini gruplamak icin
    GameObject TamEkranObjesiOlustur(string isim, Transform ebeveyn)
    {
        GameObject obj = new GameObject(isim, typeof(RectTransform));
        obj.transform.SetParent(ebeveyn, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        return obj;
    }

    // Arkasi karartilmis, tum ekrani kaplayan panel (arkaya dokunmayi da engeller)
    GameObject TamEkranKaplamaOlustur()
    {
        Image karartma = ResimOlustur(canvas, new Color(0f, 0f, 0f, 0.55f));
        karartma.sprite = null;
        RectTransform rt = karartma.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        return karartma.gameObject;
    }

    // Capa ve pivot ayni noktada olacak sekilde yerlestirir
    void Yerlestir(RectTransform rt, Vector2 capa, Vector2 konum, Vector2 boyut)
    {
        rt.anchorMin = capa;
        rt.anchorMax = capa;
        rt.pivot = capa;
        rt.anchoredPosition = konum;
        rt.sizeDelta = boyut;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Oyunun ana scripti. Sahnedeki tek "Game" objesinde durur.
// Kamera, isik ve arayuz Awake icinde koddan kurulur, bu yuzden Play'e basmadan sahne bos gorunur.
public class KasabaOyunu : MonoBehaviour
{
    Camera kamera;
    Arayuz arayuz;
    GorevSistemi gorevSistemi;
    Efektler efektler;
    Sesler sesler;
    Transform karolarKoku;

    // Hangi hucrede hangi karo var
    Dictionary<Vector2Int, KaroTuru> harita = new Dictionary<Vector2Int, KaroTuru>();
    // Konulmus karolarin sahnedeki objeleri (ziplatmak icin)
    Dictionary<Vector2Int, GameObject> karoObjeleri = new Dictionary<Vector2Int, GameObject>();
    // Karo konulabilecek bos yuvalarin gorselleri
    Dictionary<Vector2Int, GameObject> bosYuvalar = new Dictionary<Vector2Int, GameObject>();
    // Siradaki karolar, ilk eleman simdi konulacak olan
    List<KaroTuru> deste = new List<KaroTuru>();

    // Sahne yeniden yuklenirken menuyu atlayip dogrudan oyuna gecmek icin (static oldugu icin sahne degisse de kalir)
    public static bool dogrudanBasla = false;
    // Sahne yeniden yuklenince gunun kasabasi baslasin
    public static bool gunlukBasla = false;

    int gunlukTarih = 0; // 0 = normal oyun
    // Bu oyunda desteye girebilen karo turleri (odul karolari da bunlardan secilir)
    List<KaroTuru> oyunTurleri = new List<KaroTuru>();

    int puan = 0;
    int konulanKaroSayisi = 0;
    bool oyunBitti = false;
    bool menuAcik = false;

    Mesh karoMesh;
    Mesh golMesh;
    Mesh yuvaMesh;

    Material[] zeminMalzemeleri;
    Material yuvaMalzemesi;

    // Kenney Hexagon Kit modelleri (enum sirasina gore)
    GameObject[] modeller;
    // Kenney modellerinde merkezden koseye mesafe 0.577; bizim karo boyutuna buyutmek icin kullanilir
    const float ModelKoseMesafesi = 0.57735f;

    // Siradaki karo bu aciyla donmus konur; her karo biraz farkli gorunsun diye
    float siradakiDonus = 0f;

    Vector3 kameraMerkezi;
    float kameraMesafesi;
    float kameraYonu = 0f;
    float kasabaYaricapi = 1f;

    // Oyuncunun kaydirma ve zoom ayari (otomatik kamera konumunun ustune eklenir)
    Vector3 kaydirma = Vector3.zero;
    float zoom = 1f;
    // Ekranda arayuzun kapatmadigi alanin alt ve ust siniri (0-1). Kasaba bu alana ortalanir.
    float bosAlanAlt = 0.25f;
    float bosAlanUst = 0.85f;

    // Dokunma / surukleme durumu
    Vector2 basmaKonumu;
    Vector2 oncekiKonum;
    bool arayuzeBasildi = false;
    bool basmaGoruldu = false;   // parmagin basildigi ani bu script gordu mu (menu butonundan kalan dokunmalari ayirmak icin)
    bool surukleniyor = false;
    bool dokunmaIptal = false;   // iki parmak degdiyse parmak kalkinca karo konmasin
    float oncekiParmakMesafesi = 0f;

    // Hayalet onizleme: ilk dokunusta secilen hucre, ikinci dokunusta karo konur
    bool secimVar = false;
    Vector2Int secilenHucre;
    GameObject hayalet;

    void Awake()
    {
        Application.targetFrameRate = 60;

        SahneyiKur();
        GorselleriHazirla();

        arayuz = gameObject.AddComponent<Arayuz>();
        arayuz.Kur(this);

        efektler = gameObject.AddComponent<Efektler>();
        efektler.Kur();

        sesler = gameObject.AddComponent<Sesler>();
        sesler.Kur();

        gorevSistemi = gameObject.AddComponent<GorevSistemi>();
        gorevSistemi.Kur(arayuz, efektler, sesler);
    }

    void Start()
    {
        OyunKaydi kayit = Kayit.OyunuYukle();
        if (gunlukBasla)
        {
            // Menuden "Gunun Kasabasi" secildi
            gunlukBasla = false;
            YeniOyun(Kayit.BugununTarihi());
        }
        else if (kayit != null && kayit.karolar.Count > 0)
        {
            // Yarim kalan oyun varsa onu yukle (menunun arkasinda gorunur)
            KaydiYukle(kayit);
        }
        else
        {
            YeniOyun(0);
        }

        if (gunlukTarih != 0)
        {
            arayuz.ModYazisiAyarla("GÜNÜN KASABASI");
        }
        else
        {
            arayuz.ModYazisiAyarla("");
        }

        SesiUygula(Kayit.SesAcikMi());

        if (dogrudanBasla)
        {
            // "Tekrar Oyna" ya da "Yeni Oyun" ile gelindiyse menu atlanir
            dogrudanBasla = false;
            OyunaGec();
        }
        else
        {
            MenuyuAc();
        }
    }

    void Update()
    {
        if (menuAcik)
        {
            // Menu acikken kamera biraz uzaklasip kasabanin etrafinda yavasca doner
            kameraYonu += Ayarlar.OyunSonuDonusHizi * Time.deltaTime;
            zoom = Mathf.Lerp(zoom, Ayarlar.MenuZoomu, 2f * Time.deltaTime);
        }
        else if (oyunBitti)
        {
            // Oyun bitince kamera kasabanin etrafinda yavasca doner
            kameraYonu += Ayarlar.OyunSonuDonusHizi * Time.deltaTime;

            // Oyuncunun kaydirma ve zoom ayari yavasca sifirlanir ki tum kasaba gorunsun
            // Oyuncunun kaydirma ve zoom ayari yavasca sifirlanir ki tum kasaba gorunsun
            kaydirma = Vector3.Lerp(kaydirma, Vector3.zero, 2f * Time.deltaTime);
            zoom = Mathf.Lerp(zoom, 1f, 2f * Time.deltaTime);
        }
        else
        {
            GirisiKontrolEt();
            HayaletiOynat();

            // Menuden donunce kamera yavasca eski yonune (0 derece) doner
            kameraYonu = Mathf.LerpAngle(kameraYonu, 0f, 3f * Time.deltaTime);
        }

        KamerayiGuncelle(false);
    }

    // ---------------- MENU ----------------

    void MenuyuAc()
    {
        menuAcik = true;
        basmaGoruldu = false;
        SecimiIptalEt();

        // Hic karo konmadiysa kaydetmeye gerek yok, yoksa menu bos oyun icin "Devam Et" gosterir
        if (!oyunBitti && konulanKaroSayisi > 0)
        {
            OyunuKaydet();
        }

        string bilgi = "Rekor: " + Kayit.Rekor() + "     Oynanan: " + Kayit.OynananOyun();

        int gunlukRekor = Kayit.GunlukRekor(Kayit.BugununTarihi());
        string gunlukBilgi;
        if (gunlukRekor > 0)
        {
            gunlukBilgi = "Bugünün en iyisi: " + gunlukRekor;
        }
        else
        {
            gunlukBilgi = "Herkes bugün aynı desteyle oynar";
        }

        arayuz.MenuGoster(Kayit.KayitVarMi(), bilgi, SonrakiKaroYazisi(), gunlukBilgi);
    }

    // Menude gosterilen "Sonraki karo: Degirmen (65/100 puan)" yazisi
    string SonrakiKaroYazisi()
    {
        for (int i = KaroBilgi.TemelTurSayisi; i < KaroBilgi.TurSayisi; i++)
        {
            KaroTuru tur = (KaroTuru)i;
            if (!KaroBilgi.AcikMi(tur))
            {
                return "Sonraki karo: " + KaroBilgi.Isim(tur) + " (" + Kayit.ToplamPuan() + "/" + KaroBilgi.AcilmaPuani(tur) + " puan)";
            }
        }
        return "Bütün karolar açık";
    }

    void OyunaGec()
    {
        menuAcik = false;
        basmaGoruldu = false;
        arayuz.MenuGizle();

        // Menudeki uzak kamera ziplamadan normale donsun: mesafe ayni kalir, sonra kendiliginden yaklasir
        kameraMesafesi *= zoom;
        zoom = 1f;

        // Kurallari ilk acilista bir kez goster
        if (PlayerPrefs.GetInt("KurallarGosterildi", 0) == 0)
        {
            arayuz.KurallariAc();
            PlayerPrefs.SetInt("KurallarGosterildi", 1);
            PlayerPrefs.Save();
        }
    }

    // Menu butonlarinin cagirdigi fonksiyonlar
    public void MenudenBasla()
    {
        OyunaGec();
    }

    public void MenuyeDon()
    {
        // Oyun bittiyse ayni kasabaya donulemez; sahne yeniden yuklenip temiz menu acilir
        if (oyunBitti)
        {
            MenuyeDonOyunSonu();
            return;
        }
        MenuyuAc();
    }

    public void YeniOyunBaslat()
    {
        Kayit.KaydiSil();
        dogrudanBasla = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Yarim kalan oyunu silecek butonlar once onay ister. Hangisinin beklendigi burada tutulur.
    string onayBekleyenIslem = "";

    public void YeniOyunButonunaBasildi()
    {
        // Bu buton sadece kayit varken gorunur, yani her zaman onay sorulur
        onayBekleyenIslem = "yeniOyun";
        arayuz.OnayGoster("Yarım kalan oyunun silinecek.\nYeni oyun başlasın mı?");
    }

    public void GunlukButonunaBasildi()
    {
        if (Kayit.KayitVarMi())
        {
            onayBekleyenIslem = "gunluk";
            arayuz.OnayGoster("Yarım kalan oyunun silinecek.\nGünün kasabası başlasın mı?");
        }
        else
        {
            GunlukOyunBaslat();
        }
    }

    // Onay penceresinde EVET'e basilinca
    public void OnayVerildi()
    {
        arayuz.OnayiKapat();

        if (onayBekleyenIslem == "yeniOyun")
        {
            YeniOyunBaslat();
        }
        else if (onayBekleyenIslem == "gunluk")
        {
            GunlukOyunBaslat();
        }
        onayBekleyenIslem = "";
    }

    public void GunlukOyunBaslat()
    {
        // Ayni anda tek oyun kaydi tutuluyor; gunun kasabasi yarim kalan oyunun yerine gecer
        Kayit.KaydiSil();
        gunlukBasla = true;
        dogrudanBasla = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MenuyeDonOyunSonu()
    {
        // Sahne yeniden yuklenince menu acilir, arkada yeni bir kasaba kurulur
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SesiAcKapat()
    {
        bool sesAcik = !Kayit.SesAcikMi();
        Kayit.SesAyariniKaydet(sesAcik);
        SesiUygula(sesAcik);
    }

    // Ses kapaliysa butun sesler susturulur
    void SesiUygula(bool sesAcik)
    {
        if (sesAcik)
        {
            AudioListener.volume = 1f;
        }
        else
        {
            AudioListener.volume = 0f;
        }
        arayuz.SesYazisiGuncelle(sesAcik);
    }

    // ---------------- KAYIT ----------------

    void OyunuKaydet()
    {
        OyunKaydi kayit = new OyunKaydi();
        kayit.puan = puan;
        kayit.konulanKaroSayisi = konulanKaroSayisi;
        kayit.gunlukTarih = gunlukTarih;

        foreach (Vector2Int hucre in harita.Keys)
        {
            KaroKaydi karo = new KaroKaydi();
            karo.x = hucre.x;
            karo.y = hucre.y;
            karo.tur = (int)harita[hucre];
            karo.donus = karoObjeleri[hucre].transform.eulerAngles.y;
            kayit.karolar.Add(karo);
        }

        foreach (KaroTuru tur in deste)
        {
            kayit.deste.Add((int)tur);
        }

        foreach (Gorev gorev in gorevSistemi.AktifGorevler())
        {
            GorevKaydi g = new GorevKaydi();
            g.x = gorev.hucre.x;
            g.y = gorev.hucre.y;
            g.tur = (int)gorev.tur;
            g.hedef = gorev.hedef;
            kayit.gorevler.Add(g);
        }

        Kayit.OyunuKaydet(kayit);
    }

    void KaydiYukle(OyunKaydi kayit)
    {
        puan = kayit.puan;
        konulanKaroSayisi = kayit.konulanKaroSayisi;
        oyunBitti = false;
        gunlukTarih = kayit.gunlukTarih;
        OyunTurleriniBelirle();

        // Karolar animasyonsuz, kaydedildigi aciyla yerine konur
        foreach (KaroKaydi karo in kayit.karolar)
        {
            siradakiDonus = karo.donus;
            KaroyuHaritayaEkle(new Vector2Int(karo.x, karo.y), (KaroTuru)karo.tur);
        }
        siradakiDonus = RastgeleDonus();

        deste.Clear();
        foreach (int tur in kayit.deste)
        {
            deste.Add((KaroTuru)tur);
        }

        foreach (GorevKaydi g in kayit.gorevler)
        {
            gorevSistemi.GorevEkle(harita, new Vector2Int(g.x, g.y), (KaroTuru)g.tur, g.hedef);
        }

        arayuz.Guncelle(puan, deste);
        KamerayiGuncelle(true);
    }

    // Telefonda uygulama arka plana atilinca (ya da kapatilinca) oyun kaybolmasin
    void OnApplicationPause(bool durdu)
    {
        if (durdu && !oyunBitti && konulanKaroSayisi > 0)
        {
            OyunuKaydet();
        }
    }

    void OnApplicationQuit()
    {
        if (!oyunBitti && konulanKaroSayisi > 0)
        {
            OyunuKaydet();
        }
    }

    // ---------------- KURULUM ----------------

    void SahneyiKur()
    {
        GameObject kameraObj = new GameObject("Kamera");
        kameraObj.tag = "MainCamera";
        kamera = kameraObj.AddComponent<Camera>();
        kamera.clearFlags = CameraClearFlags.SolidColor;
        kamera.backgroundColor = Ayarlar.ArkaPlanRengi;
        kamera.fieldOfView = Ayarlar.KameraGorusAcisi;
        kamera.nearClipPlane = 0.3f;
        kamera.farClipPlane = 200f;
        kameraObj.AddComponent<AudioListener>(); // seslerin duyulmasi icin

        GameObject isikObj = new GameObject("Gunes");
        Light isik = isikObj.AddComponent<Light>();
        isik.type = LightType.Directional;
        isik.intensity = 1.3f;
        isik.color = new Color(1f, 0.96f, 0.88f);
        isik.shadows = LightShadows.Soft;
        isik.shadowStrength = 0.6f;
        isikObj.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

        // Golgeler cok karanlik olmasin
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);

        karolarKoku = new GameObject("Karolar").transform;
    }

    void GorselleriHazirla()
    {
        float yaricap = Ayarlar.KaroBoyutu * Ayarlar.KaroBoslukOrani;
        karoMesh = HexGrid.AltigenMeshOlustur(yaricap, Ayarlar.KaroYuksekligi);
        golMesh = HexGrid.AltigenMeshOlustur(yaricap, Ayarlar.GolYuksekligi);
        yuvaMesh = HexGrid.AltigenMeshOlustur(yaricap * 0.9f, Ayarlar.BosYuvaYuksekligi);

        // Her karo turu icin bir zemin malzemesi (enum sirasina gore)
        zeminMalzemeleri = new Material[KaroBilgi.TurSayisi];
        for (int i = 0; i < KaroBilgi.TurSayisi; i++)
        {
            zeminMalzemeleri[i] = MalzemeOlustur(KaroBilgi.ZeminRengi((KaroTuru)i));
        }

        yuvaMalzemesi = MalzemeOlustur(Ayarlar.BosYuvaRengi);

        modeller = new GameObject[KaroBilgi.TurSayisi];
        for (int i = 0; i < KaroBilgi.TurSayisi; i++)
        {
            string modelAdi = KaroBilgi.ModelAdi((KaroTuru)i);
            modeller[i] = Resources.Load<GameObject>("Modeller/" + modelAdi);
            if (modeller[i] == null)
            {
                Debug.LogError("Model bulunamadi: Resources/Modeller/" + modelAdi + " (duz altigen kullanilacak)");
            }
        }

        siradakiDonus = RastgeleDonus();
    }

    // Altigen 60 derecede bir kendisiyle cakisir, bu yuzden 60'in katlari
    float RastgeleDonus()
    {
        return Random.Range(0, 6) * 60f;
    }

    Material MalzemeOlustur(Color renk)
    {
        // Resources icindeki URP malzemesi kopyalanir; build'de shader'in dahil edilmesini garantiler
        Material temel = Resources.Load<Material>("KaroMalzeme");
        Material malzeme;
        if (temel != null)
        {
            malzeme = new Material(temel);
        }
        else
        {
            malzeme = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
        malzeme.color = renk;
        return malzeme;
    }

    // ---------------- OYUN AKISI ----------------

    // tarih 0 ise normal oyun, degilse o gunun kasabasi
    void YeniOyun(int tarih)
    {
        puan = 0;
        oyunBitti = false;
        gunlukTarih = tarih;

        // Gunun kasabasinda rastgelelik tarihten baslar: o gun herkes ayni desteyle oynar
        if (gunlukTarih != 0)
        {
            Random.InitState(gunlukTarih);
        }

        OyunTurleriniBelirle();
        DesteyiHazirla();

        // Ortaya bedava bir cimen karo konur, oyun onun etrafinda buyur
        KaroyuHaritayaEkle(Vector2Int.zero, KaroTuru.Cimen);

        arayuz.Guncelle(puan, deste);
        KamerayiGuncelle(true);
    }

    // Bu oyunda hangi karo turleri cikacak.
    // Gunun kasabasinda herkes esit olsun diye sadece temel turler var; normal oyunda acilanlar da eklenir.
    void OyunTurleriniBelirle()
    {
        oyunTurleri.Clear();
        for (int i = 0; i < KaroBilgi.TurSayisi; i++)
        {
            KaroTuru tur = (KaroTuru)i;
            bool temelTur = i < KaroBilgi.TemelTurSayisi;
            if (temelTur || (gunlukTarih == 0 && KaroBilgi.AcikMi(tur)))
            {
                oyunTurleri.Add(tur);
            }
        }
    }

    void DesteyiHazirla()
    {
        deste.Clear();
        foreach (KaroTuru tur in oyunTurleri)
        {
            EkleBirden(tur, KaroBilgi.DesteSayisi(tur));
        }

        // Karistir (Fisher-Yates)
        for (int i = deste.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            KaroTuru gecici = deste[i];
            deste[i] = deste[j];
            deste[j] = gecici;
        }
    }

    void EkleBirden(KaroTuru tur, int adet)
    {
        for (int i = 0; i < adet; i++)
        {
            deste.Add(tur);
        }
    }

    void GirisiKontrolEt()
    {
        // Fare tekerlegi ile zoom (bilgisayarda denemek icin)
        if (Mouse.current != null)
        {
            float tekerlek = Mouse.current.scroll.ReadValue().y;
            if (tekerlek != 0f)
            {
                ZoomYap(1f - tekerlek * Ayarlar.FareZoomHizi);
            }
        }

        // Pointer hem fareyi hem dokunmatik ekrani kapsar
        Pointer isaretci = Pointer.current;
        if (isaretci == null) return;

        Vector2 konum = isaretci.position.ReadValue();

        if (isaretci.press.wasPressedThisFrame)
        {
            basmaKonumu = konum;
            oncekiKonum = konum;
            arayuzeBasildi = arayuz.UIUzerindeMi(konum);
            surukleniyor = false;
            dokunmaIptal = false;
            oncekiParmakMesafesi = 0f;
            basmaGoruldu = true;
        }

        // Basilma ani gorulmediyse (ornegin parmak menudeki OYNA butonuna basmisti) bu dokunmayi yok say
        if (!basmaGoruldu) return;
        if (arayuzeBasildi) return;

        // Iki parmak ekrandaysa kaydirma yerine zoom yapilir
        if (IkiParmaklaZoom())
        {
            dokunmaIptal = true;
            oncekiKonum = konum; // parmak kalkinca kamera ziplamasin
            return;
        }

        if (isaretci.press.isPressed)
        {
            // Parmak yeterince kayinca dokunma degil surukleme sayilir
            float esik = Screen.height * Ayarlar.DokunmaEsigiOrani;
            if (!surukleniyor && Vector2.Distance(konum, basmaKonumu) > esik)
            {
                surukleniyor = true;
            }

            if (surukleniyor)
            {
                Kaydir(oncekiKonum, konum);
            }
            oncekiKonum = konum;
        }

        if (isaretci.press.wasReleasedThisFrame)
        {
            basmaGoruldu = false;
            if (!surukleniyor && !dokunmaIptal)
            {
                EkranaDokunuldu(konum);
            }
        }
    }

    // Iki parmak ekrandaysa aralarindaki mesafenin degisimine gore zoom yapar
    bool IkiParmaklaZoom()
    {
        Touchscreen ekran = Touchscreen.current;
        if (ekran == null) return false;

        int parmakSayisi = 0;
        Vector2 birinci = Vector2.zero;
        Vector2 ikinci = Vector2.zero;

        for (int i = 0; i < ekran.touches.Count; i++)
        {
            if (!ekran.touches[i].press.isPressed) continue;

            if (parmakSayisi == 0) birinci = ekran.touches[i].position.ReadValue();
            else if (parmakSayisi == 1) ikinci = ekran.touches[i].position.ReadValue();
            parmakSayisi++;
        }

        if (parmakSayisi < 2)
        {
            oncekiParmakMesafesi = 0f;
            return false;
        }

        float mesafe = Vector2.Distance(birinci, ikinci);
        if (oncekiParmakMesafesi > 0f && mesafe > 0f)
        {
            // Parmaklar acilinca yaklas, kapaninca uzaklas
            ZoomYap(oncekiParmakMesafesi / mesafe);
        }
        oncekiParmakMesafesi = mesafe;
        return true;
    }

    void ZoomYap(float carpan)
    {
        zoom = Mathf.Clamp(zoom * carpan, Ayarlar.EnYakinZoom, Ayarlar.EnUzakZoom);
    }

    // Parmagin altindaki zemin noktasi parmakla birlikte hareket edecek sekilde kamerayi kaydirir
    void Kaydir(Vector2 eskiEkranKonumu, Vector2 yeniEkranKonumu)
    {
        Vector3 eskiNokta;
        Vector3 yeniNokta;
        if (!ZemindekiNokta(eskiEkranKonumu, 0f, out eskiNokta)) return;
        if (!ZemindekiNokta(yeniEkranKonumu, 0f, out yeniNokta)) return;

        kaydirma += eskiNokta - yeniNokta;

        // Kasaba ekrandan tamamen kacmasin
        kaydirma = Vector3.ClampMagnitude(kaydirma, kasabaYaricapi);

        // Kamerayi hemen tasi, yoksa parmak ile zemin arasinda kayma hissedilir
        KamerayiYerlestir();
    }

    // Ekrandaki noktanin, verilen yukseklikteki yatay zeminde nereye denk geldigini bulur
    bool ZemindekiNokta(Vector2 ekranKonumu, float yukseklik, out Vector3 nokta)
    {
        Ray isin = kamera.ScreenPointToRay(ekranKonumu);
        Plane zemin = new Plane(Vector3.up, new Vector3(0f, yukseklik, 0f));

        float mesafe;
        if (zemin.Raycast(isin, out mesafe))
        {
            nokta = isin.GetPoint(mesafe);
            return true;
        }
        nokta = Vector3.zero;
        return false;
    }

    void EkranaDokunuldu(Vector2 ekranKonumu)
    {
        Vector3 nokta;
        if (!ZemindekiNokta(ekranKonumu, Ayarlar.KaroYuksekligi * 0.5f, out nokta)) return;

        Vector2Int hucre = HexGrid.HucreBul(nokta);

        // Bos yuva disinda bir yere dokunulursa secim iptal olur
        if (!Puanlama.YerlestirilebilirMi(harita, hucre))
        {
            SecimiIptalEt();
            return;
        }

        if (!Ayarlar.IkiDokunuslaYerlestir)
        {
            KaroYerlestir(hucre);
            return;
        }

        if (secimVar && secilenHucre == hucre)
        {
            // Ayni yuvaya ikinci dokunus: karoyu koy
            SecimiIptalEt();
            KaroYerlestir(hucre);
        }
        else
        {
            HucreyiSec(hucre);
        }
    }

    // ---------------- HAYALET ONIZLEME ----------------

    void HucreyiSec(Vector2Int hucre)
    {
        SecimiIptalEt();

        secimVar = true;
        secilenHucre = hucre;

        KaroTuru tur = deste[0];
        hayalet = KaroGorseliOlustur(hucre, tur);
        hayalet.name = "Hayalet";
        bosYuvalar[hucre].SetActive(false);
        HayaletiOynat();
        sesler.SecimSesi();

        int kazanilacak = Puanlama.YerlestirmePuani(harita, hucre, tur);
        Vector3 yaziKonumu = HexGrid.DunyaKonumu(hucre) + Vector3.up * (Ayarlar.YaziYuksekligi + Ayarlar.HayaletYuksekligi);

        // "Tekrar dokun" ipucu sadece ilk birkac karoda gosterilir, sonra kalabalik yapmasin
        bool ipucuGoster = konulanKaroSayisi < 3;
        arayuz.OnizlemeYazisiGoster(yaziKonumu, "+" + kazanilacak, ipucuGoster);
    }

    void SecimiIptalEt()
    {
        if (hayalet != null)
        {
            Destroy(hayalet);
            hayalet = null;
        }

        if (secimVar && bosYuvalar.ContainsKey(secilenHucre))
        {
            bosYuvalar[secilenHucre].SetActive(true);
        }

        secimVar = false;
        arayuz.OnizlemeYazisiGizle();
    }

    // Hayalet karo yerinin biraz ustunde yavasca inip kalkar
    void HayaletiOynat()
    {
        if (hayalet == null) return;

        Vector3 konum = HexGrid.DunyaKonumu(secilenHucre);
        konum.y = Ayarlar.HayaletYuksekligi + Mathf.Sin(Time.time * 4f) * 0.06f;
        hayalet.transform.position = konum;
    }

    void KaroYerlestir(Vector2Int hucre)
    {
        KaroTuru tur = deste[0];
        deste.RemoveAt(0);
        konulanKaroSayisi++;

        // Puan, karo haritaya eklenmeden once komsulara bakilarak hesaplanir
        int kazanilan = Puanlama.YerlestirmePuani(harita, hucre, tur);
        puan += kazanilan;

        KaroyuHaritayaEkle(hucre, tur);

        // Karo yukaridan duser; yere degince KaroYereDegdi cagrilir (ses, toz, komsu ziplamasi)
        karoObjeleri[hucre].GetComponent<KaroAnimasyonu>().Dus();
        siradakiDonus = RastgeleDonus();

        if (kazanilan > 0)
        {
            Vector3 yaziKonumu = HexGrid.DunyaKonumu(hucre) + Vector3.up * Ayarlar.YaziYuksekligi;
            arayuz.UcanYaziGoster(yaziKonumu, "+" + kazanilan);
        }

        // Tamamlanan gorevler desteye karo ekler (son karoda tamamlanirsa oyun devam eder)
        int odulKaro = gorevSistemi.KaroKonuldu(harita, hucre, tur);
        DesteyeKaroEkle(odulKaro);

        arayuz.Guncelle(puan, deste);

        if (deste.Count == 0)
        {
            OyunuBitir();
        }
        else
        {
            // Her karodan sonra kaydedilir, uygulama beklenmedik kapansa bile oyun kaybolmaz
            OyunuKaydet();
        }
    }

    // Dusen karo yere degdiginde KaroAnimasyonu tarafindan cagrilir
    public void KaroYereDegdi(Vector2Int hucre)
    {
        KaroTuru tur = harita[hucre];
        efektler.TozCikar(HexGrid.DunyaKonumu(hucre));

        // Puan veren komsular ziplar, boylece puanin nereden geldigi gorulur
        int toplam = 0;
        for (int i = 0; i < 6; i++)
        {
            Vector2Int komsu = HexGrid.Komsu(hucre, i);
            if (!harita.ContainsKey(komsu)) continue;

            int komsuPuani = Puanlama.KomsuPuani(tur, harita[komsu]);
            if (komsuPuani > 0)
            {
                toplam += komsuPuani;
                karoObjeleri[komsu].GetComponent<KaroAnimasyonu>().Zipla();
            }
        }

        sesler.YerlestirmeSesi(toplam);
    }

    // Odul karolari rastgele turlerden secilir ve destenin rastgele yerlerine karistirilir.
    // Ekranda gorunen siradaki karolar degismesin diye onizlemenin arkasina eklenir.
    void DesteyeKaroEkle(int adet)
    {
        for (int i = 0; i < adet; i++)
        {
            KaroTuru tur = oyunTurleri[Random.Range(0, oyunTurleri.Count)];
            int enKucukIndex = Mathf.Min(Ayarlar.OnizlemeSayisi, deste.Count);
            int index = Random.Range(enKucukIndex, deste.Count + 1);
            deste.Insert(index, tur);
        }
    }

    void KaroyuHaritayaEkle(Vector2Int hucre, KaroTuru tur)
    {
        harita[hucre] = tur;

        GameObject karo = KaroGorseliOlustur(hucre, tur);
        KaroAnimasyonu animasyon = karo.AddComponent<KaroAnimasyonu>();
        animasyon.oyun = this;
        animasyon.hucre = hucre;
        karoObjeleri[hucre] = karo;

        // Bu hucredeki bos yuva artik gereksiz
        if (bosYuvalar.ContainsKey(hucre))
        {
            Destroy(bosYuvalar[hucre]);
            bosYuvalar.Remove(hucre);
        }

        // Bos komsulara yeni yuva ekle
        for (int i = 0; i < 6; i++)
        {
            Vector2Int komsu = HexGrid.Komsu(hucre, i);
            if (!harita.ContainsKey(komsu) && !bosYuvalar.ContainsKey(komsu))
            {
                bosYuvalar[komsu] = BosYuvaOlustur(komsu);
            }
        }
    }

    void OyunuBitir()
    {
        oyunBitti = true;
        SecimiIptalEt();

        // Bos yuvalar kasabanin son halinde gorunmesin
        foreach (GameObject yuva in bosYuvalar.Values)
        {
            yuva.SetActive(false);
        }
        gorevSistemi.IsaretleriGizle();

        // Puan eklenmeden once hangi turlerin kilitli oldugunu not al
        bool[] onceAcikti = new bool[KaroBilgi.TurSayisi];
        for (int i = 0; i < KaroBilgi.TurSayisi; i++)
        {
            onceAcikti[i] = KaroBilgi.AcikMi((KaroTuru)i);
        }

        // Rekor, toplam puan ve oynanan oyun sayisi guncellenir, yarim oyun kaydi silinir
        bool yeniRekor = Kayit.OyunBitti(puan, gunlukTarih);

        // Bu oyunun puaniyla yeni acilan karo var mi
        string acilanYazisi = "";
        for (int i = 0; i < KaroBilgi.TurSayisi; i++)
        {
            if (!onceAcikti[i] && KaroBilgi.AcikMi((KaroTuru)i))
            {
                // Tek oyunda iki tur birden acilabilir: "Degirmen, Liman"
                if (acilanYazisi != "") acilanYazisi += ", ";
                acilanYazisi += KaroBilgi.Isim((KaroTuru)i);
            }
        }
        if (acilanYazisi != "")
        {
            acilanYazisi = "Yeni karo açıldı: " + acilanYazisi + "!";
        }

        string rekorYazisi;
        if (gunlukTarih == 0)
        {
            rekorYazisi = "Rekor: " + Kayit.Rekor();
        }
        else
        {
            rekorYazisi = "Bugünün en iyisi: " + Kayit.GunlukRekor(gunlukTarih);
        }

        arayuz.OyunSonuGoster(puan, rekorYazisi, yeniRekor, acilanYazisi);
        sesler.OyunSonuSesiCal();
    }

    public void TekrarOyna()
    {
        // Gunun kasabasi bittiyse tekrar oynayinca yine gunun kasabasi acilir
        if (gunlukTarih != 0)
        {
            gunlukBasla = true;
        }
        dogrudanBasla = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------------- GORSELLER ----------------

    GameObject KaroGorseliOlustur(Vector2Int hucre, KaroTuru tur)
    {
        GameObject model = modeller[(int)tur];
        GameObject karo;

        if (model != null)
        {
            karo = Instantiate(model, karolarKoku);
            karo.name = "Karo " + KaroBilgi.Isim(tur);

            // Kenney modelini bizim karo boyutuna buyut
            float olcek = Ayarlar.KaroBoyutu * Ayarlar.KaroBoslukOrani / ModelKoseMesafesi;
            karo.transform.localScale = Vector3.one * olcek;
            karo.transform.rotation = Quaternion.Euler(0f, siradakiDonus, 0f);
        }
        else
        {
            // Model yoksa oyun yine calissin diye duz renkli altigen
            Mesh mesh = karoMesh;
            if (tur == KaroTuru.Gol)
            {
                mesh = golMesh;
            }
            karo = MeshObjesiOlustur("Karo " + KaroBilgi.Isim(tur), mesh, zeminMalzemeleri[(int)tur]);
        }

        karo.transform.position = HexGrid.DunyaKonumu(hucre);
        return karo;
    }

    GameObject BosYuvaOlustur(Vector2Int hucre)
    {
        GameObject yuva = MeshObjesiOlustur("Bos Yuva", yuvaMesh, yuvaMalzemesi);
        yuva.transform.position = HexGrid.DunyaKonumu(hucre);
        return yuva;
    }

    GameObject MeshObjesiOlustur(string isim, Mesh mesh, Material malzeme)
    {
        GameObject obj = new GameObject(isim);
        obj.transform.SetParent(karolarKoku, false);
        obj.AddComponent<MeshFilter>().sharedMesh = mesh;
        obj.AddComponent<MeshRenderer>().sharedMaterial = malzeme;
        return obj;
    }

    // ---------------- KAMERA ----------------

    // Kamerayi tum karolari ve bos yuvalari ekrana sigdiracak sekilde ayarlar
    void KamerayiGuncelle(bool aninda)
    {
        Vector3 enKucuk = new Vector3(float.MaxValue, 0f, float.MaxValue);
        Vector3 enBuyuk = new Vector3(float.MinValue, 0f, float.MinValue);

        List<Vector2Int> hucreler = new List<Vector2Int>(harita.Keys);
        if (!oyunBitti)
        {
            hucreler.AddRange(bosYuvalar.Keys);
        }

        foreach (Vector2Int hucre in hucreler)
        {
            Vector3 p = HexGrid.DunyaKonumu(hucre);
            enKucuk = Vector3.Min(enKucuk, p);
            enBuyuk = Vector3.Max(enBuyuk, p);
        }

        Vector3 merkez = (enKucuk + enBuyuk) / 2f;
        float yaricap = 0f;
        foreach (Vector2Int hucre in hucreler)
        {
            float d = Vector3.Distance(HexGrid.DunyaKonumu(hucre), merkez);
            if (d > yaricap) yaricap = d;
        }
        yaricap += Ayarlar.KaroBoyutu;
        kasabaYaricapi = yaricap;

        // Arayuzun kapatmadigi bos alan (menu karti, alt panel, oyun sonu karti haric)
        float alt = arayuz.BosAlanAlti();
        float ust = arayuz.BosAlanUstu();
        if (ust - alt < 0.2f)
        {
            // Arayuz henuz yerlesmediyse (ilk kare) tahmini bir deger kullan
            alt = 0.25f;
            ust = 0.85f;
        }

        if (aninda)
        {
            bosAlanAlt = alt;
            bosAlanUst = ust;
        }
        else
        {
            bosAlanAlt = Mathf.Lerp(bosAlanAlt, alt, 5f * Time.deltaTime);
            bosAlanUst = Mathf.Lerp(bosAlanUst, ust, 5f * Time.deltaTime);
        }

        // Kasaba bos alana sigmali: dikey gorus acisi bos alanin yuksekligi kadar daralir.
        // Dikey ve yatay acidan dar olani sigdirmayi belirler.
        float dikeyYarim = kamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float bosDikeyYarim = Mathf.Atan(Mathf.Tan(dikeyYarim) * (bosAlanUst - bosAlanAlt));
        float yatayYarim = Mathf.Atan(Mathf.Tan(dikeyYarim) * kamera.aspect);
        float darAci = Mathf.Min(bosDikeyYarim, yatayYarim);
        float hedefMesafe = yaricap * Ayarlar.KameraBoslukCarpani / Mathf.Sin(darAci);

        if (aninda)
        {
            kameraMerkezi = merkez;
            kameraMesafesi = hedefMesafe;
        }
        else
        {
            float t = Ayarlar.KameraTakipHizi * Time.deltaTime;
            kameraMerkezi = Vector3.Lerp(kameraMerkezi, merkez, t);
            kameraMesafesi = Mathf.Lerp(kameraMesafesi, hedefMesafe, t);
        }

        KamerayiYerlestir();
    }

    // Otomatik konumun ustune oyuncunun kaydirma ve zoom ayarini ekleyip kamerayi yerine koyar
    void KamerayiYerlestir()
    {
        Quaternion donus = Quaternion.Euler(Ayarlar.KameraEgimi, kameraYonu, 0f);
        float mesafe = kameraMesafesi * zoom;

        // Kasabayi ekranin ortasina degil, bos alanin ortasina getir.
        // Bos alanin ortasi ekranin ortasindan ne kadar yukaridaysa, kamera o kadar asagiya bakar.
        float ortaFarki = (bosAlanAlt + bosAlanUst) * 0.5f - 0.5f;
        float ekranYarimYuksekligi = Mathf.Tan(kamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * mesafe;
        Vector3 bakisNoktasi = kameraMerkezi + kaydirma - (donus * Vector3.up) * (2f * ekranYarimYuksekligi * ortaFarki);

        kamera.transform.rotation = donus;
        kamera.transform.position = bakisNoktasi - (donus * Vector3.forward) * mesafe;
    }
}

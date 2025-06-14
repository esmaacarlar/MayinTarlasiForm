using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MayinTarlasiForm
{
    public partial class Form1 : Form
    {
        private Timer oyunSuresiTimer;
        private int gecenSure = 0;
        private Tahta tahta;
        
        private Button[,] butonlar;
        private const int hucreBoyutu = 40;

        public Form1()
        {
            InitializeComponent();
            
            button1.Click += BtnBaslat_Click;
            comboBox1.Items.AddRange(new string[] { "Kolay", "Orta", "Zor" });
        }

        private void BtnBaslat_Click(object sender, EventArgs e)
        {
            // Önceki zamanı ve label'ı sıfırla
            gecenSure = 0;
            label1.Text = "Süre: 0 sn";

            // Timer'ı başlat
            oyunSuresiTimer = new Timer();
            oyunSuresiTimer.Interval = 1000; // 1 saniye
            oyunSuresiTimer.Tick += OyunSuresiTimer_Tick;
            oyunSuresiTimer.Start();

            OyunZorlugu zorluk;

            string secilen = comboBox1.SelectedItem.ToString();

            if (secilen == "Kolay")
                zorluk = new Kolay();
            else if (secilen == "Orta")
                zorluk = new Orta();
            else if (secilen == "Zor")
                zorluk = new Zor();
            else
                zorluk = new Kolay();

            tahta = new Tahta(zorluk);
            this.ClientSize = new Size(tahta.SutunSayisi * hucreBoyutu + 40, tahta.SatirSayisi * hucreBoyutu + 70);
            tahta.MayinlariYerleştir();
            tahta.CevredekiMayinlariHesapla();

            TahtaUIOlustur();
        }

        private void OyunSuresiTimer_Tick(object sender, EventArgs e)
        {
            gecenSure++;
            label1.Text = $"Süre: {gecenSure} sn";
        }

        private void TahtaUIOlustur()  // Tahta arayüzünü oluşturur
        {
            panel1.Controls.Clear();

            int satirSayisi = tahta.SatirSayisi;
            int sutunSayisi = tahta.SutunSayisi;

            panel1.Size = new Size(hucreBoyutu * sutunSayisi, hucreBoyutu * satirSayisi);

            butonlar = new Button[satirSayisi, sutunSayisi];

            for (int s = 0; s < satirSayisi; s++)
            {
                for (int c = 0; c < sutunSayisi; c++)
                {
                    Button btn = new Button  
                    {
                        Size = new Size(hucreBoyutu, hucreBoyutu),
                        Location = new Point(c * hucreBoyutu, s * hucreBoyutu),
                        Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold),
                        BackColor = Color.LightGray,
                        Tag = tahta.Hucreler[s, c]
                    };
                    btn.MouseUp += HucreButon_MouseUp;
                    panel1.Controls.Add(btn);
                    butonlar[s, c] = btn;
                }
            }
        }

        private void HucreButon_MouseUp(object sender, MouseEventArgs e)  // Hücre butonuna tıklandığında çalışır
        {
            if (tahta == null || tahta.OyunBitti) return;

            Button btn = sender as Button;
            Hucre hucre = (Hucre)btn.Tag;

            if (e.Button == MouseButtons.Right)  // Sağ tık ile işaretleme
            {
                hucre.IsaretDegistir();
                btn.Text = hucre.IsaretlendiMi ? "🚩" : "";
                return;
            }
            else if (e.Button == MouseButtons.Left)  // Sol tık ile açma
            {
                if (hucre.IsaretlendiMi || hucre.AcildiMi) return;

                tahta.HucreAc(hucre.Satir, hucre.Sutun);

                TahtayiGuncelle();

                if (tahta.OyunBitti)
                {
                    TumMayinlariAc();
                    oyunSuresiTimer.Stop();
                    MessageBox.Show("Mayına bastınız! Oyun bitti. " + label1.Text, "Kaybettiniz");
                    ButonlariAc();
                }
                else if (tahta.KazandiMi())
                {
                    oyunSuresiTimer.Stop();
                    MessageBox.Show("Tebrikler! Oyunu kazandınız. " + label1.Text, "Kazandınız");
                    ButonlariAc();
                }
            }
        }

        private void TahtayiGuncelle()  
        {
            for (int s = 0; s < tahta.SatirSayisi; s++)
            {
                for (int c = 0; c < tahta.SutunSayisi; c++)
                {
                    var hucre = tahta.Hucreler[s, c];
                    var btn = butonlar[s, c];

                    if (hucre.AcildiMi)
                    {
                        btn.Enabled = false;
                        if (hucre.MayinVarMi)
                        {
                            btn.Text = "💣";
                            btn.BackColor = Color.Red;
                        }
                        else
                        {
                            btn.Text = hucre.CevredekiMayinSayisi > 0 ? hucre.CevredekiMayinSayisi.ToString() : "";
                            btn.BackColor = Color.White;
                        }
                    }
                    else
                    {
                        btn.Enabled = true;
                        btn.Text = hucre.IsaretlendiMi ? "🚩" : "";
                        btn.BackColor = Color.LightGray;
                    }
                }
            }
        }

        private void ButonlariAc()
        {
            for (int s = 0; s < tahta.SatirSayisi; s++)
            {
                for (int c = 0; c < tahta.SutunSayisi; c++)
                {
                    butonlar[s, c].Enabled = false;
                }
            }
        }

        private void TumMayinlariAc()
        {
            for (int i = 0; i < tahta.SatirSayisi; i++)
            {
                for (int j = 0; j < tahta.SutunSayisi; j++)
                {
                    var hucre = tahta.Hucreler[i, j];
                    var btn = butonlar[i, j];

                    if (!hucre.AcildiMi)
                    {
                        if (hucre.MayinVarMi)
                        {
                            btn.Text = "💣";
                            btn.BackColor = Color.Red;
                        }
                    }
                }
            }
        }
    }
    public abstract class OyunZorlugu
    {
        public abstract int SatirSayisi { get; }
        public abstract int SutunSayisi { get; }
        public abstract int MayinSayisi { get; }
    }

    public class Kolay : OyunZorlugu
    {
        public override int SatirSayisi
        {
            get { return 8; }
        }

        public override int SutunSayisi
        {
            get { return 8; }
        }

        public override int MayinSayisi
        {
            get { return 10; }
        }

    }

    public class Orta : OyunZorlugu
    {
        public override int SatirSayisi
        {
            get { return 10; }
        }

        public override int SutunSayisi
        {
            get { return 10; }
        }

        public override int MayinSayisi
        {
            get { return 20; }
        }
    }

    public class Zor : OyunZorlugu
    {
        public override int SatirSayisi
        {
            get { return 12; }
        }

        public override int SutunSayisi
        {
            get { return 12; }
        }

        public override int MayinSayisi
        {
            get { return 30; }
        }
    }

    // Hücre sınıfı
    public class Hucre
    {
        public bool MayinVarMi { get; private set; } = false;
        public bool AcildiMi { get; private set; } = false;
        public bool IsaretlendiMi { get; private set; } = false;
        public int CevredekiMayinSayisi { get; set; } = 0;
        public int Satir { get; }
        public int Sutun { get; }

        public Hucre(int satir, int sutun)
        {
            Satir = satir;
            Sutun = sutun;
        }

        public void MayinYerleştir()
        {
            MayinVarMi = true;
        }

        public void Ac()
        {
            AcildiMi = true;
        }

        public void IsaretDegistir()
        {
            if (!AcildiMi)
                IsaretlendiMi = !IsaretlendiMi;
        }
    }

    // Tahta sınıfı (Oyun alanı)
    public class Tahta
    {
        public int SatirSayisi { get; }
        public int SutunSayisi { get; }
        public int MayinSayisi { get; }
        public Hucre[,] Hucreler { get; }
        private Random rnd = new Random();

        public bool OyunBitti { get; private set; } = false;

        public Tahta(OyunZorlugu zorluk)
        {
            SatirSayisi = zorluk.SatirSayisi;
            SutunSayisi = zorluk.SutunSayisi;
            MayinSayisi = zorluk.MayinSayisi;

            Hucreler = new Hucre[SatirSayisi, SutunSayisi];
            for (int s = 0; s < SatirSayisi; s++)
                for (int c = 0; c < SutunSayisi; c++)
                    Hucreler[s, c] = new Hucre(s, c);
        }

        public void MayinlariYerleştir() // Mayınları rastgele yerleştirir
        {
            int yerlestirilen = 0;
            while (yerlestirilen < MayinSayisi)
            {
                int s = rnd.Next(SatirSayisi);
                int c = rnd.Next(SutunSayisi);
                if (!Hucreler[s, c].MayinVarMi)
                {
                    Hucreler[s, c].MayinYerleştir();
                    yerlestirilen++;
                }
            }
        }
        
        public void CevredekiMayinlariHesapla() // Her hücrenin çevresindeki mayın sayısını hesaplar
        {
            for (int s = 0; s < SatirSayisi; s++)
            {
                for (int c = 0; c < SutunSayisi; c++)
                {
                    if (Hucreler[s, c].MayinVarMi)
                    {
                        Hucreler[s, c].CevredekiMayinSayisi = -1;
                        continue;
                    }
                    int sayac = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue; // Kendisine bakmaz
                            int yeniSatir = s + i;
                            int yeniSutun = c + j;
                            if (yeniSatir >= 0 && yeniSatir < SatirSayisi && yeniSutun >= 0 && yeniSutun < SutunSayisi && Hucreler[yeniSatir, yeniSutun].MayinVarMi)
                                sayac++;
                        }
                    }
                    Hucreler[s, c].CevredekiMayinSayisi = sayac;
                }
            }
        }

        public void HucreAc(int s, int c) // Hücre açma işlemi
        {
            if (OyunBitti) return;
            if (s < 0 || s >= SatirSayisi || c < 0 || c >= SutunSayisi) return;

            Hucre hucre = Hucreler[s, c];
            if (hucre.AcildiMi || hucre.IsaretlendiMi) return;

            hucre.Ac();

            if (hucre.MayinVarMi)
            {
                OyunBitti = true;
                return;
            }

            if (hucre.CevredekiMayinSayisi == 0)
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        HucreAc(s + i, c + j);
                    }
                }
            }
        }
      
        public bool KazandiMi() // Kazanma kontrolü
        {
            if (OyunBitti) return false;

            int guvenliHucreSayisi = SatirSayisi * SutunSayisi - MayinSayisi;
            int acilanHucreSayisi = 0;

            foreach (var hucre in Hucreler)
                if (hucre.AcildiMi && !hucre.MayinVarMi)
                    acilanHucreSayisi++;

            return acilanHucreSayisi == guvenliHucreSayisi;
        }
    }
}
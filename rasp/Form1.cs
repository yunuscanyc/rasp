using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Net;
using System.IO;
using FluentScheduler;
namespace rasp
{
    
    public partial class Form1 : Form
    {
        MySqlConnection con;
        MySqlCommand cmd;
        MySqlDataReader dr;
        
        string txtsunucu, txtkullanici, txtparola, txtyol, oynayangorev, sabitdosya,id;
        List<gorev> oynayacak = new List<gorev>();
        string sabityer = "/home/pi/Desktop/sabit/";
        string sabitvideo = "/home/pi/Desktop/video/";
        //public string sabityer = @"c:\\video\";
        //public string sabitvideo = @"c:\\sabit\";
        IDictionary< int, string> zamanlar = new Dictionary<int,string>();
        //public bool guncel = false;
        bool guncel = true;


        public Form1()
        {
            InitializeComponent();
            con = new MySqlConnection("Server=213.238.178.192;Database=reklam;user=root;Pwd=root_password;SslMode=none");
            
        }
        private void vnckapat()
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xdotool",
                    Arguments = "search --name \"VNC\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            var pids = new List<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                string strCmdText;
                strCmdText = "windowunmap " + line;
                Process.Start("xdotool", strCmdText);

                                

            }
            proc.StandardOutput.Close();
            /*foreach (string el in pids)
            {
                string strCmdText;
                strCmdText = "windowname " + el;
                Process.Start("xdotool", strCmdText);
                
            }*/
            

            
            }

        private void label2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(oynayacak.Count.ToString());
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {


            


        }
        private void Download(string inecek, string nereden, string nereye)
        {
            string inputfilepath =  nereye  + inecek;

            string ftpfullpath = "ftp://213.238.178.192/files/yukle/" + nereden + inecek;
            try
            {

            
            using (WebClient request = new WebClient())
            {
                request.Credentials = new NetworkCredential("yunus", "Elifesma123");
                byte[] fileData = request.DownloadData(ftpfullpath);

                using (FileStream file = File.Create(inputfilepath))
                {
                    file.Write(fileData, 0, fileData.Length);
                    file.Close();
                }
                
            }
                label1.Text = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
            }
            catch
            {
                label1.Text = "Gücellenemedi";
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Cursor.Hide();
            id = "1";
            string j;
            for (int i = 0; i < 24; i++)
            {
                if (Convert.ToString(i).Length == 1)
                    j = "0" + Convert.ToString(i);
                else
                {
                    j = Convert.ToString(i);

                }
                zamanlar.Add(i,j + ":00-" + j + ":59");
            }
            cmd = new MySqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "select * from ayarlar";
            dr = cmd.ExecuteReader();


            while (dr.Read())
            {
                txtsunucu = dr.GetString("ftpsunucu");
                txtkullanici = dr.GetString("ftpuser");
                txtparola = dr.GetString("ftppass");
                txtyol = dr.GetString("ftpdizin");
                oynayangorev = dr.GetString("oynayan_gorev");
                sabitdosya = dr.GetString("sabitdosya");
            }
            dr.Close();
            
            System.IO.DirectoryInfo dil = new DirectoryInfo(sabityer);

            foreach (FileInfo file in dil.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in dil.GetDirectories())
            {
                dir.Delete(true);
            }

            Download(sabitdosya, "/sabit/", sabityer);
            con.Close();
            cmd = null;
            
        }
        public void guncelle()
        {
            oynayacak.Clear();
            cmd = new MySqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "select * from ayarlar";
            dr = cmd.ExecuteReader();


            while (dr.Read())
            {
                txtsunucu = dr.GetString("ftpsunucu");
                txtkullanici = dr.GetString("ftpuser");
                txtparola = dr.GetString("ftppass");
                txtyol = dr.GetString("ftpdizin");
                oynayangorev = dr.GetString("oynayan_gorev");
                sabitdosya = dr.GetString("sabitdosya");
            }
            dr.Close();
            
            cmd = new MySqlCommand();
            cmd.Connection = con;
            cmd.CommandText = "select * from reklamlar where gorev='" + oynayangorev + "'";
           
            dr = cmd.ExecuteReader();
            List<gorev> gorevler = new List<gorev>();

            

            while (dr.Read())
            {
                
                string gorev = dr.GetString("gorev");
                string dosya = dr.GetString("dosyaserver");
                string idler = dr.GetString("idler");
                string zamanlar = dr.GetString("zamanlar");
                
                gorevler.Add(new gorev()
                {
                    Gorev = gorev,
                    Dosya = dosya,
                    Idler = idler,
                    Zamanlar = zamanlar,

                });
            }
            dr.Close();

            con.Close();
            List<string> indirilecek = new List<string>();
            foreach (gorev dos in gorevler)
            {
                string[] ids = dos.Idler.Split(' ');
                if (!indirilecek.Contains(dos.Dosya) && ids.Contains(id))
                {
                    indirilecek.Add(dos.Dosya);
                }
                if (ids.Contains(id))
                {
                    oynayacak.Add(new gorev()
                    {
                        Gorev = dos.Gorev,
                        Dosya = dos.Dosya,
                        Idler = dos.Idler,
                        Zamanlar = dos.Zamanlar,

                    });
                }
            }
            System.IO.DirectoryInfo di = new DirectoryInfo(sabitvideo);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            foreach (string dos in indirilecek)
            {
                Download(dos, "/video/", sabitvideo);
            }
            
        }
        private void Form1_Deactivate(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //vnckapat();     
            var timer = new System.Threading.Timer(fe => guncelle(),null,TimeSpan.Zero, TimeSpan.FromSeconds(30));

            while (true)
            {
                bool oynadimi = false;
                

                
                foreach (gorev dos in oynayacak.ToList())
                {
                    Application.DoEvents();
                    try
                    {
                        DateTime bugun = DateTime.Now;
                    int simdi = bugun.Hour;
                    string[] zaman = dos.Zamanlar.Split(' ');                  
                    if (zaman.Contains(zamanlar[simdi]) && guncel && File.Exists(sabitvideo + dos.Dosya))
                    {
                        Process proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "omxplayer",
                                Arguments = sabitvideo + dos.Dosya,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                            }
                        };
                        proc.Start();
                        while (!proc.HasExited)
                        {
                            System.Threading.Thread.Sleep(500);
                            vnckapat();
                            }
                            oynadimi = true;
                        }
                    
                    
                       
                    
                }
                    catch
                    {
                        
                        break;

                    }
                }
               
                if (!oynadimi && guncel && File.Exists(sabityer + sabitdosya))
                {
                    Process proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "omxplayer",
                            Arguments = sabityer + sabitdosya,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    proc.Start();
                    while (!proc.HasExited)
                    {
                        System.Threading.Thread.Sleep(500);
                        vnckapat();
                    }
                }



            }
        }
    }
    class gorev
    {
        string gorevadi;
        string dosya;
        string idler;
        string zamanlar;

        public string Gorev
        {
            get
            {
                return gorevadi;
            }

            set
            {
                gorevadi = value;
            }
        }

        public string Dosya
        {
            get
            {
                return dosya;
            }

            set
            {
                dosya = value;
            }
        }
        public string Idler
        {
            get
            {
                return idler;
            }

            set
            {
                idler = value;
            }
        }
        public string Zamanlar
        {
            get
            {
                return zamanlar;
            }

            set
            {
                zamanlar = value;
            }
        }

    }

   
}

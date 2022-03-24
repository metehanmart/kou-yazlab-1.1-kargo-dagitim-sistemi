using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yazlab1._1
{
    public partial class Form3 : Form
    {
        public MySql.Data.MySqlClient.MySqlConnection conn;
        public List<String> adresler;
        public List<PointLatLng> points;
        public List<List<float>> distances;
        public IList<IList<int>> tumPermutasyonlar;
        private int indxPerm;
        public PointLatLng subeLok;
        public PointLatLng aracLok;
        public GMapOverlay yollar;
        private int aracIndx;
        private int aracharketIndx;
        private GMapOverlay markersArac;
        GMapOverlay markers;
        public Form3()
        {
            InitializeComponent();
            adresler = new List<string>();
            points = new List<PointLatLng>();
            tumPermutasyonlar = new List<IList<int>>();
            subeLok = new PointLatLng();
            aracLok = new PointLatLng();
            yolBulma();



        }
        static IList<IList<int>> Permute(int[] nums)
        {
            var list = new List<IList<int>>();
            return DoPermute(nums, 0, nums.Length - 1, list);
        }

        static IList<IList<int>> DoPermute(int[] nums, int start, int end, IList<IList<int>> list)
        {
            if (start == end)
            {

                list.Add(new List<int>(nums));
            }
            else
            {
                for (var i = start; i <= end; i++)
                {
                    Swap(ref nums[start], ref nums[i]);
                    DoPermute(nums, start + 1, end, list);
                    Swap(ref nums[start], ref nums[i]);
                }
            }

            return list;
        }

        static void Swap(ref int a, ref int b)
        {
            var temp = a;
            a = b;
            b = temp;
        }
        private void print()
        {
            foreach (var list in distances)
            {
                foreach (var element in list)
                {
                    Console.Write(element + "  ");
                }
                Console.WriteLine();
            }
        }

        private void yolBulma()
        {

            Connection();
            // tüm noktaları aldım pointste tutuyorum
            string sql = "select * from arac";//kargo subesiydi araca donustu!
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                double lat = double.Parse(rdr["Lat"].ToString());
                double lng = double.Parse(rdr["Lng"].ToString());
                points.Add(new PointLatLng(lat, lng));
            }
            conn.Close();
            Connection();
            sql = "select * from kargolar Where isTeslimEdildi = 0";
            cmd = new MySqlCommand(sql, conn);
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                double lat = double.Parse(rdr["Lat"].ToString());
                double lng = double.Parse(rdr["Lng"].ToString());
                points.Add(new PointLatLng(lat, lng));
            }
            conn.Close();
            /*
            for (int i = 0; i < points.Count(); i++)
            {
                Console.WriteLine("points   " + points[i].Lat.ToString() + " " + points[i].Lng.ToString());
            }
            */
            //kargo subesinden çıkacak kargo aracı o yüzden orası ilk node olacak 
            // tüm noktalar arasındaki mesafeler bulundu

            //Console.WriteLine("hadeeeee   " + points.Count());

            distances = new List<List<float>>();



            GMapProviders.GoogleMap.ApiKey = Apikey.Key;



            for (int i = 0; i < points.Count(); i++)
            {

                List<float> tmp = new List<float>();
                for (int j = 0; j < points.Count; j++)
                {
                    var route = GoogleMapProvider.Instance.GetRoute(points[i], points[j], false, false, 12);// sondaki parametre zoom degeri 
                    tmp.Add(float.Parse(route.Distance.ToString()));

                }
                distances.Add(new List<float>(tmp));
            }
            //print();

            //önce tüm permutasyonları bulmamımız lazım kaç tane node varsa orada distances listini kullanarak kısa yolu buluruz
            // tüm permutosyanlardan en kısa olanı bizim yolumuzdur
            int pointSayisi = points.Count();
            int[] arr = new int[pointSayisi];
            for (int i = 0; i < pointSayisi; i++)
            {
                arr[i] = i;
            }
            tumPermutasyonlar = Permute(arr);
            float enkisaYol = float.MaxValue;
            float tmpSum = 0;
            indxPerm = -1;// en kisa yolu tutan permin indexi

            for (int i = 0; i < tumPermutasyonlar.Count; i++)
            {
                tmpSum = 0;
                for (int j = 0; j < tumPermutasyonlar[i].Count - 1; j++)
                {
                    // kargo şubesinden başlamalı yolculuğa ordan başlamadıysa direkt eliyoruz
                    if (tumPermutasyonlar[i][0] != 0)
                    {
                        tmpSum = 0;
                        break;
                    }
                    else
                    {
                        tmpSum += distances[tumPermutasyonlar[i][j]][tumPermutasyonlar[i][j + 1]];// uzaklıkları topladım
                    }

                    if (j == tumPermutasyonlar[i].Count - 2)
                    {
                        if (tmpSum < enkisaYol)
                        {
                            enkisaYol = tmpSum;
                            indxPerm = i;
                        }
                    }

                }


            }
            /*
            Console.WriteLine("en kisa yol = "+enkisaYol);
            foreach (var i in tumPermutasyonlar[indxPerm])
            {
                Console.Write(i+" ");
            }
            
            Console.WriteLine("---------- ");
            */
            if (indxPerm != -1)
                ciz(tumPermutasyonlar[indxPerm].ToList());
            else
            {
                //MessageBox.Show("Dağıtılacak kargo kalmadı","Kargo Dağıtım Sistemi",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }


        }
        // yol bulma için
        private void polygonCiz()
        {

        }

        private void ciz(List<int> kisaYol)
        {
            GoogleMapProvider.Instance.ApiKey = Apikey.Key;
            yollar = new GMapOverlay("yollar");
            for (int i = 0; i < kisaYol.Count - 1; i++)
            {
                var route = GoogleMapProvider.Instance.GetRoute(points[kisaYol[i]], points[kisaYol[i + 1]], false, false, 12);
                var r = new GMapRoute(route.Points, "My route");
                if (r.Instructions.Count == 0)
                {
                    //Console.WriteLine("boş");
                }

                yollar.Routes.Add(r);
                gMapControl1.Overlays.Add(yollar);
            }


        }
        private void Connection()
        {
            string myConnectionString;
            myConnectionString = "connectionstring";
            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString);
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            adresler = new List<String>();

        }
        public void adresleriYukle()
        {
            Connection();
            string select = "select MusteriLokasyon from kargolar Where isTeslimEdildi = 0";
            MySqlCommand cmd = new MySqlCommand(select, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            GeoCoderStatusCode statusCode;

            while (rdr.Read())
            {
                adresler.Add(rdr["MusteriLokasyon"].ToString());
            }
            conn.Close();
            foreach (var i in adresler)
            {
                var pointLatLng = GoogleMapProvider.Instance.GetPoint(i.Trim(), out statusCode);
                if (statusCode == GeoCoderStatusCode.OK)
                {
                    double lat = (double)pointLatLng?.Lat;
                    double lng = (double)pointLatLng?.Lng;

                    PointLatLng point = new PointLatLng(lat, lng);
                    GMapMarker marker;

                    marker = new GMarkerGoogle(point, GMarkerGoogleType.red_pushpin);
                    //Overlay ekle
                    markers = new GMapOverlay("markers");
                    markers.IsVisibile = false;
                    //Markerlerı overlaya ekle
                    markers.Markers.Add(marker);
                    // haritayı doldur
                    gMapControl1.Overlays.Add(markers);
                    markers.IsVisibile = true;
                    //Console.WriteLine("foreacdeyim");
                }


            }

        }
        private void kargoDagitimMerekeziYukle()
        {
            Connection();
            string select = "select * from kargoSubesi";
            MySqlCommand cmd = new MySqlCommand(select, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            GeoCoderStatusCode statusCode;

            double lat = 0, lng = 0;
            while (rdr.Read())
            {
                adresler.Add(rdr["Lokasyon"].ToString());
                lat = double.Parse(rdr["Lat"].ToString());
                lng = double.Parse(rdr["Lng"].ToString());
            }
            conn.Close();
            foreach (var i in adresler)
            {

                var pointLatLng = GoogleMapProvider.Instance.GetPoint(i.Trim(), out statusCode);
                if (statusCode == GeoCoderStatusCode.OK)
                {

                    PointLatLng point = new PointLatLng(lat, lng);
                    subeLok = new PointLatLng(lat, lng);
                    GMapMarker marker;

                    marker = new GMarkerGoogle(point, GMarkerGoogleType.blue_dot);
                    //Overlay ekle
                    markers = new GMapOverlay("markers");
                    markers.IsVisibile = false;
                    //Markerlerı overlaya ekle
                    markers.Markers.Add(marker);
                    // haritayı doldur
                    gMapControl1.Overlays.Add(markers);
                    markers.IsVisibile = true;
                    //Console.WriteLine("foreacdeyim");
                }


            }
        }
        private void gMapControl1_Load(object sender, EventArgs e)
        {

            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            gMapControl1.Position = new GMap.NET.PointLatLng(40.7641692, 29.9414999);

            adresleriYukle();
            kargoDagitimMerekeziYukle();
            aracHareket();

        }

        // aracı subeye götüren method
        private void aracSubeye()
        {
            aracLok = subeLok;
            aracIndx = 0;
            aracharketIndx = 0;
            Connection();
            string sql = $"UPDATE arac SET hareketIndex='{aracharketIndx}' WHERE idarac=1;";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
            /*
            Console.WriteLine("lat = "+aracLok.Lat.ToString()+"lng = "+aracLok.Lng.ToString());
            Console.WriteLine("lat = " + subeLok.Lat.ToString() + "lng = " + subeLok.Lng.ToString());
            */

            Connection();

            //select arac sorgusu atılacak table olusturulacak
            sql = $"UPDATE arac SET Lat='{aracLok.Lat.ToString()}',Lng='{aracLok.Lng.ToString()}' WHERE idarac=1;";
            cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();


            // tabledan arac pozisyonu alıncak arac pozisyonu ilk başta ne olmalı kargo subesi olabilir

            GMapProviders.GoogleMap.ApiKey = Apikey.Key;
            GMapOverlay markersArac;
            GMapMarker markerArac;




            markerArac = new GMarkerGoogle(aracLok, GMarkerGoogleType.arrow);
            //Overlay ekle
            markersArac = new GMapOverlay("markersArac");
            markersArac.IsVisibile = true;
            //Markerlerı overlaya ekle
            markersArac.Markers.Add(markerArac);
            // haritayı doldur
            gMapControl1.Overlays.Add(markersArac);
            //Console.WriteLine("arac subedeyim");

        }
        // aracı hareket ettiren kod bunu timer tickte çağıracağız

        // aracın pozisyonunu değiştiren method
        private void aracIlerle()
        {

            // arachareketindxi clouddan alıyor
            Connection();
            string sql = "select * from arac";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                aracharketIndx = 0;
            }
            conn.Close();

            aracharketIndx++;



            //Console.WriteLine("arachareketIndx == "+aracharketIndx);
            //Console.WriteLine(tumPermutasyonlar[indxPerm].Count);
            //burada kargo bitince hata verdi dizin bir şeyler dışında faalan!!!
            //Console.WriteLine("indxperm =="+indxPerm);
            if (indxPerm != -1)
            {
                if (aracharketIndx < tumPermutasyonlar[indxPerm].Count)
                {
                    aracIndx = tumPermutasyonlar[indxPerm][aracharketIndx];
                }
                else
                {
                    aracharketIndx = 0;
                    aracIndx = tumPermutasyonlar[indxPerm][aracharketIndx];
                }
            }
            else
            {
                aracSubeye();
                return;

            }




            //clodu güncelliyor
            Connection();
            sql = $"UPDATE arac SET Lat='{points[aracIndx].Lat.ToString()}',Lng='{points[aracIndx].Lng.ToString()}',hareketIndex='{aracharketIndx}' WHERE idarac=1;";
            cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();



            Connection();
            sql = $"SELECT * from kargolar WHERE Lat='{points[aracIndx].Lat.ToString()}' And Lng='{points[aracIndx].Lng.ToString()}'";
            cmd = new MySqlCommand(sql, conn);
            rdr = cmd.ExecuteReader();
            int kargoID = 0;
            while (rdr.Read())
            {
                kargoID = int.Parse(rdr["KargoID"].ToString());
            }
            conn.Close();

            Connection();
            sql = $"UPDATE kargolar SET isTeslimEdildi=1 WHERE KargoID={kargoID};";
            cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();

            // görüntüyü güncelliyor
            markersArac.Clear();
            GMapMarker markerArac;
            PointLatLng point = new PointLatLng(points[aracIndx].Lat, points[aracIndx].Lng);

            markerArac = new GMarkerGoogle(point, GMarkerGoogleType.arrow);
            //Overlay ekle
            markersArac = new GMapOverlay("markers");
            markersArac.IsVisibile = false;
            //Markerlerı overlaya ekle
            markersArac.Markers.Add(markerArac);
            // haritayı doldur
            gMapControl1.Overlays.Add(markersArac);
            markersArac.IsVisibile = true;




        }
        private void aracHareket()//Markerın pozisyonunu okuyup haritada görsel günceller 
        {


            Connection();
            float lat = 0, lng = 0;
            //select arac sorgusu atılacak table olusturulacak
            // database yeni konum eklenecek
            //string update = "UPDATE arac ";
            string select = "select * from arac";
            MySqlCommand cmd = new MySqlCommand(select, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                lat = float.Parse(rdr["Lat"].ToString());
                lng = float.Parse(rdr["Lng"].ToString());
            }

            // tabledan arac pozisyonu alıncak arac pozisyonu ilk başta ne olmalı kargo subesi olabilir
            GMapProviders.GoogleMap.ApiKey = Apikey.Key;
            GMapMarker markerArac;


            PointLatLng point = new PointLatLng(lat, lng);

            markerArac = new GMarkerGoogle(point, GMarkerGoogleType.arrow);
            //Overlay ekle
            markersArac = new GMapOverlay("markers");
            markersArac.IsVisibile = false;
            //Markerlerı overlaya ekle
            markersArac.Markers.Add(markerArac);
            // haritayı doldur
            gMapControl1.Overlays.Add(markersArac);
            markersArac.IsVisibile = true;
            conn.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("tik tak");
            adresler.Clear();
            points.Clear();
            tumPermutasyonlar.Clear();
            for (int i = 0; i < markers.Markers.Count; i++)
                gMapControl1.Overlays.Clear();
            kargoDagitimMerekeziYukle();
            adresleriYukle();
            yolBulma();
            aracIlerle();
            aracHareket();

            gMapControl1.Zoom = -5;
            gMapControl1.Zoom = 13;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }
    }
}

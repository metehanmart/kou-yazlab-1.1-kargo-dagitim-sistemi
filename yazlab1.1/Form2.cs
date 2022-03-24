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
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Threading;

namespace yazlab1._1
{
    public partial class Form2 : Form
    {
        public MySql.Data.MySqlClient.MySqlConnection conn;
        public ThreadStart thread;
        public Thread t;
        public delegate void testDelegate();
        public List<String> adresler;
        public int silinecekID;
        public GMapOverlay markers = new GMapOverlay("markers");
        public Form2()
        {
            InitializeComponent();
            tabloyuAyarla();
            verileriGetir();
        }
        private void tabloyuAyarla()
        {
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.RowHeadersVisible = false;
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
            string select = "select MusteriLokasyon from kargolar WHERE isTeslimEdildi = 0";
            MySqlCommand cmd = new MySqlCommand(select, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            GeoCoderStatusCode statusCode;
            GMapOverlay markers;
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
            GMapOverlay markers;
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

        private void Form2_Load(object sender, EventArgs e)
        {
            label5.Visible = false;
            label5.ForeColor = Color.Red;

            GMapProviders.GoogleMap.ApiKey = Apikey.Key;
            label1.Text = Form1.girkullanici;// kullancı adını sag üste yazıyor
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;

            gMapControl1.Position = new GMap.NET.PointLatLng(40.7641692, 29.9414999);
            adresleriYukle();
            kargoDagitimMerekeziYukle();

        }


        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {



            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                richTextBox1.Clear();
                double lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
                double lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;

                textBox2.Text = lat.ToString();
                textBox3.Text = lng.ToString();

                PointLatLng point = new PointLatLng(lat, lng);

                Connection();
                GMapMarker marker;
                if (checkBox1.Checked == true)
                {
                    marker = new GMarkerGoogle(point, GMarkerGoogleType.yellow_dot);
                }
                else
                {
                    marker = new GMarkerGoogle(point, GMarkerGoogleType.red_pushpin);
                }

                //Overlay ekle
                GMapOverlay markers = new GMapOverlay("markers");
                markers.IsVisibile = false;
                //Markerlerı overlaya ekle
                markers.Markers.Add(marker);
                // haritayı doldur
                gMapControl1.Overlays.Add(markers);
                markers.IsVisibile = true;
                var adress = GetAdress(point);
                if (adress != null && checkBox1.Checked == false)
                {
                    richTextBox1.Text = adress[0];
                    //clouda adresleri yükleme

                    Connection();
                    string sql = $"INSERT INTO kargolar (MusteriLokasyon,Lat,Lng) values ('{adress[0]}','{lat}','{lng}');";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    cmd.ExecuteNonQuery();
                    conn.Close();

                }
                else if (adress != null && checkBox1.Checked == true)
                {
                    richTextBox1.Text = adress[0];
                    string sql = $"UPDATE kargoSubesi SET Lokasyon='{richTextBox1.Text.ToString()}',Lat='{lat}',Lng='{lng}' WHERE SubeID=1;";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    yenile();
                }
                else
                    richTextBox1.Text = "Adres bulunamadı";
            }
            verileriGetir();
        }

        private List<String> GetAdress(PointLatLng point)
        {
            List<Placemark> placemarks = null;
            var statusCode = GMapProviders.GoogleMap.GetPlacemarks(point, out placemarks);
            if (statusCode == GeoCoderStatusCode.OK && placemarks != null)
            {
                List<String> addresses = new List<String>();
                foreach (var i in placemarks)
                {
                    addresses.Add(i.Address);
                }
                return addresses;
            }
            return null;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            GeoCoderStatusCode statusCode;
            var pointLatLng = GoogleMapProvider.Instance.GetPoint(richTextBox1.Text.Trim(), out statusCode);
            if (statusCode == GeoCoderStatusCode.OK)
            {
                //clouda yükleme

                Connection();

                textBox2.Text = pointLatLng?.Lat.ToString();
                textBox3.Text = pointLatLng?.Lng.ToString();

                double lat = (double)pointLatLng?.Lat;
                double lng = (double)pointLatLng?.Lng;


                PointLatLng point = new PointLatLng(lat, lng);
                GMapMarker marker;
                if (checkBox1.Checked == true)
                {
                    //fixle
                    string sql = $"UPDATE kargoSubesi SET Lokasyon='{richTextBox1.Text}',Lat='{lat}',Lng='{lng}' WHERE SubeID=1;";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    marker = new GMarkerGoogle(point, GMarkerGoogleType.blue_dot);
                    yenile();


                }
                else
                {
                    string sql = $"INSERT INTO kargolar (MusteriLokasyon,Lat,Lng) values ('{richTextBox1.Text.ToString()}','{lat}','{lng}');";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    marker = new GMarkerGoogle(point, GMarkerGoogleType.red_pushpin);
                }

                //Overlay ekle

                markers.IsVisibile = false;
                //Markerlerı overlaya ekle
                markers.Markers.Add(marker);
                // haritayı doldur
                gMapControl1.Overlays.Add(markers);
                gMapControl1.Position = new GMap.NET.PointLatLng(lat, lng);
                markers.IsVisibile = true;
                yenile();
                verileriGetir();

            }
        }
        private void yenile()
        {
            gMapControl1.Overlays.Clear();
            adresler.Clear();
            for (int i = 0; i < markers.Markers.Count; i++)
                gMapControl1.Overlays.Clear();
            kargoDagitimMerekeziYukle();
            adresleriYukle();
            gMapControl1.Zoom = -5;
            gMapControl1.Zoom = 15;
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Connection();
            string sql = $"DELETE FROM kargolar WHERE KargoID={Convert.ToInt32(textBox1.Text)} ;";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
            yenile();
            verileriGetir();
        }

        private void verileriGetir()
        {
            Connection();
            string select = "select * from kargolar";
            MySqlCommand cmd = new MySqlCommand(select, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(rdr);
            dataGridView1.DataSource = dt;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            verileriGetir();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            yenile();
            verileriGetir();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Connection();
            double lat = -1, lng = -1;

            lat = double.Parse(textBox2.Text.ToString());
            lng = double.Parse(textBox3.Text.ToString());

            if (lat == -1 && lng == -1)
            {
                label5.Visible = true;
                return;
            }

            PointLatLng point = new PointLatLng(lat, lng);
            GMapMarker marker;
            if (checkBox1.Checked == true)
            {
                marker = new GMarkerGoogle(point, GMarkerGoogleType.yellow_dot);
            }
            else
            {
                marker = new GMarkerGoogle(point, GMarkerGoogleType.red_pushpin);
            }

            //Overlay ekle
            GMapOverlay markers = new GMapOverlay("markers");
            markers.IsVisibile = false;
            //Markerlerı overlaya ekle
            markers.Markers.Add(marker);
            // haritayı doldur
            gMapControl1.Overlays.Add(markers);
            markers.IsVisibile = true;
            var adress = GetAdress(point);
            if (adress != null && checkBox1.Checked == false)
            {
                richTextBox1.Text = adress[0];
                //clouda adresleri yükleme

                Connection();
                //string sql = $"INSERT INTO kargolar (MusteriLokasyon) values ('{adress[0]}');";
                string sql = $"INSERT INTO kargolar (MusteriLokasyon,Lat,Lng) values ('{adress[0]}','{lat}','{lng}');";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                yenile();
            }
            else if (adress != null && checkBox1.Checked == true)
            {
                richTextBox1.Text = adress[0];
                string sql = $"UPDATE kargoSubesi SET Lokasyon='{richTextBox1.Text.ToString()}',Lat='{lat}',Lng='{lng}' WHERE SubeID=1;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                yenile();
            }
            else
                richTextBox1.Text = "Adres bulunamadı";

        }
    }
}

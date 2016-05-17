using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Aero2TransferChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private HttpClient client;

        private void button1_Click(object sender, EventArgs e)
        {
            client = new HttpClient();
            login();
        }
      
        
        private async void login()
        {
            LoginData ld = new LoginData(); ld.login = textBox2.Text; ld.password = textBox3.Text;

            try
            {
                client.BaseAddress = new Uri("https://moje.aero2.pl/");
                var result = client.PostAsJsonAsync("ProstyPrepaid/selfcare/login", ld).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                if (result.StatusCode != System.Net.HttpStatusCode.OK) throw new Exception("not http 200");
                try
                {
                    var result2 = client.GetAsync("ProstyPrepaid/selfcare/getClientInfo").Result;
                    string resultContent2 = result2.Content.ReadAsStringAsync().Result;
                    Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(resultContent2);
                    DisplayData dd = parsePackageInfo(json);
                    textBox1.Text = Newtonsoft.Json.JsonConvert.SerializeObject(dd).ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("login failed!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("data parse failed!");
            }

        }

        private DisplayData parsePackageInfo(Newtonsoft.Json.Linq.JObject json)
        {
            DisplayData dd = new DisplayData();

            dd.remainingMB = Int32.Parse(json["data"]["currentPackage"]["totalLimitsRemaining"].ToString());
            dd.percentUsed =
                100*
                Int32.Parse(json["data"]["currentPackage"]["totalLimitsUsed"].ToString()) /
                Int32.Parse(json["data"]["currentPackage"]["totalLimit"].ToString());

            string dateOfEnd = json["data"]["currentPackage"]["expirationDate"].ToString(); // format: "04.06.2016 godzina 16:41"
            dd.dateOfEnd = new DateTime(
                Int32.Parse(dateOfEnd.Substring(6,4)),
                Int32.Parse(dateOfEnd.Substring(3,2)),
                Int32.Parse(dateOfEnd.Substring(0,2)),
                Int32.Parse(dateOfEnd.Substring(19,2)),
                Int32.Parse(dateOfEnd.Substring(22,2)),
                0
                );
            DateTime dateNow = DateTime.Now;
            TimeSpan daysLeftTS = dd.dateOfEnd - dateNow;
            dd.daysLeft = daysLeftTS.Days;

            dd.percentOfTime = 100 * dd.daysLeft / Int32.Parse(json["data"]["currentPackage"]["period"].ToString());

            dd.meanAllowedTransferPerDayMB = dd.remainingMB / dd.daysLeft;

            return dd;            
        }
    }
}

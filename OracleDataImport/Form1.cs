using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSU.PA.TJS;
using System.Threading.Tasks;
using System.Security;
using System.Diagnostics;
using System.Threading;
using NLog;
using DataImportLib.Models;
using DataImportLib;

namespace OracleDataImport
{
    public partial class Form1 : Form
    {
        String metaDataConnectionString;

        DateTime thisRun;

        private static Logger logger;

        public Form1()
        {
            logger = LogManager.GetLogger("OracleDataImport");
            //metaDataConnectionString = Properties.Settings.Default.MetaDataDBConnStr;

            InitializeComponent();


        }

        /*private void WriteToLog(string category, string message)
        {
            SqlConnection mdConn = new SqlConnection(metaDataConnectionString);

            mdConn.Open();
            SqlCommand cmd = new SqlCommand("INSERT INTO ImportMessages (ImportDate,TableName,Category,Message) VALUES (GETDATE(),@table_name,@category,@message)", mdConn);
            cmd.Parameters.AddWithValue("@table_name", "--System--");
            cmd.Parameters.AddWithValue("@category", category);
            cmd.Parameters.AddWithValue("@message", message);
            cmd.ExecuteNonQuery();

            mdConn.Close();
        }*/

        private void button1_Click(object sender, EventArgs e)
        {
            logger.Info("~~~~ Import Started ~~~~");

            DataImportExecutor di = new DataImportExecutor(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

            di.Run();

        }



        private void UpdateLogDisplay()
        {
            /*
            SqlConnection conn = new SqlConnection(metaDataConnectionString);
            SqlDataAdapter da = new SqlDataAdapter("select * from ImportMessages where ImportDate > @date  order by ImportDate", conn);
            da.SelectCommand.Parameters.AddWithValue("@date", thisRun);

            DataTable log = new DataTable();
            da.Fill(log);

            foreach (DataRow row in log.Rows)
            {
                DateTime t = System.Convert.ToDateTime(row[0]);
                if (t > thisRun) thisRun = t;
                ListViewItem lvi = new ListViewItem(t.ToString());
                lvi.SubItems.Add(row[1].ToString());
                lvi.SubItems.Add(row[2].ToString());
                lvi.SubItems.Add(row[3].ToString());
                lvLog.Items.Add(lvi);
            }
            */
        }

        private void btnCryptoForm_Click(object sender, EventArgs e)
        {
            GetCryptoPasswordFrm frm = new GetCryptoPasswordFrm(metaDataConnectionString);

            frm.ShowDialog();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            metaDataConnectionString = "Data Source=" + cbServer.Text + ";Initial Catalog=" + tbDataBase.Text + ";Integrated Security=SSPI;";
            DataSet ds = new DataSet();
            SqlConnection mdConn = new SqlConnection(metaDataConnectionString);
            //SqlDataAdapter da = new SqlDataAdapter("select * from ImportList",mdConn);
            //da.Fill(ds, "ImportList");

            dgvImportList.DataSource = ds.Tables["ImportList"];
            dgvImportList.ReadOnly = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //LandingRepository repo = new LandingRepository();
            //List<Landing_Table> import_tables = repo.GetLandingTables();

            //dgvImportList.DataSource = import_tables;
            //dgvImportList.ReadOnly = true;

            //cbServer.SelectedIndex = 0;

        }


    }
}

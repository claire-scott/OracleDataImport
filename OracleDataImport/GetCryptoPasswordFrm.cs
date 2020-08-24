using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Data.SqlClient;


namespace OracleDataImport
{
    public partial class GetCryptoPasswordFrm : Form
    {
        private String metaDataConnectionString;


        public GetCryptoPasswordFrm(string MetaDataConnectionString)
        {
            metaDataConnectionString = MetaDataConnectionString;

            InitializeComponent();
        }



        private void plainPass_TextChanged(object sender, EventArgs e)
        {
            cryptoPass.Text = RijndaelSimple.Encrypt(plainPass.Text, "passPhrase", "saltValue", "SHA1", 1, "initVector", 128);
        }

        private void btnSaveCypher_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(metaDataConnectionString);
            SqlCommand cmd = new SqlCommand("UPDATE ImportList SET SourcePass = @pass WHERE SourceUser = @user",conn);
            cmd.Parameters.AddWithValue("@pass", cryptoPass.Text);
            cmd.Parameters.AddWithValue("@user", tbUsername.Text);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}

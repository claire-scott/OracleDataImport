namespace OracleDataImport
{
    partial class GetCryptoPasswordFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.plainPass = new System.Windows.Forms.TextBox();
            this.cryptoPass = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.btnSaveCypher = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // plainPass
            // 
            this.plainPass.Location = new System.Drawing.Point(187, 31);
            this.plainPass.Name = "plainPass";
            this.plainPass.Size = new System.Drawing.Size(251, 20);
            this.plainPass.TabIndex = 1;
            this.plainPass.TextChanged += new System.EventHandler(this.plainPass_TextChanged);
            // 
            // cryptoPass
            // 
            this.cryptoPass.Location = new System.Drawing.Point(187, 57);
            this.cryptoPass.Name = "cryptoPass";
            this.cryptoPass.ReadOnly = true;
            this.cryptoPass.Size = new System.Drawing.Size(251, 20);
            this.cryptoPass.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(79, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Password to Secure";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(102, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Secure Version";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(126, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Username";
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(187, 5);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(251, 20);
            this.tbUsername.TabIndex = 0;
            // 
            // btnSaveCypher
            // 
            this.btnSaveCypher.Location = new System.Drawing.Point(267, 83);
            this.btnSaveCypher.Name = "btnSaveCypher";
            this.btnSaveCypher.Size = new System.Drawing.Size(171, 23);
            this.btnSaveCypher.TabIndex = 3;
            this.btnSaveCypher.Text = "Save Cyphered Password";
            this.btnSaveCypher.UseVisualStyleBackColor = true;
            this.btnSaveCypher.Click += new System.EventHandler(this.btnSaveCypher_Click);
            // 
            // GetCryptoPasswordFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 117);
            this.Controls.Add(this.btnSaveCypher);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbUsername);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cryptoPass);
            this.Controls.Add(this.plainPass);
            this.Name = "GetCryptoPasswordFrm";
            this.Text = "GetCryptoPasswordFrm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox plainPass;
        private System.Windows.Forms.TextBox cryptoPass;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.Button btnSaveCypher;
    }
}
namespace TSM_Select
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.OK = new System.Windows.Forms.Button();
            this.modINFO = new System.Windows.Forms.Label();
            this.groupBox_modINFO = new System.Windows.Forms.GroupBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.groupBox_modINFO.SuspendLayout();
            this.SuspendLayout();
            // 
            // OK
            // 
            this.OK.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.OK.Location = new System.Drawing.Point(1117, 822);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(114, 54);
            this.OK.TabIndex = 0;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // modINFO
            // 
            this.modINFO.AutoSize = true;
            this.modINFO.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.modINFO.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.modINFO.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.modINFO.Location = new System.Drawing.Point(21, 37);
            this.modINFO.Name = "modINFO";
            this.modINFO.Size = new System.Drawing.Size(80, 22);
            this.modINFO.TabIndex = 3;
            this.modINFO.Text = "modINFO";
            this.modINFO.UseMnemonic = false;
            this.modINFO.Click += new System.EventHandler(this.elementsCount_Click);
            // 
            // groupBox_modINFO
            // 
            this.groupBox_modINFO.Controls.Add(this.modINFO);
            this.groupBox_modINFO.Location = new System.Drawing.Point(747, 47);
            this.groupBox_modINFO.Name = "groupBox_modINFO";
            this.groupBox_modINFO.Size = new System.Drawing.Size(474, 137);
            this.groupBox_modINFO.TabIndex = 6;
            this.groupBox_modINFO.TabStop = false;
            this.groupBox_modINFO.Text = "groupBox_modINFO";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 20;
            this.listBox1.Location = new System.Drawing.Point(28, 44);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(626, 864);
            this.listBox1.TabIndex = 7;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // listBox2
            // 
            this.listBox2.FormattingEnabled = true;
            this.listBox2.ItemHeight = 20;
            this.listBox2.Location = new System.Drawing.Point(707, 244);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(524, 284);
            this.listBox2.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1276, 954);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.groupBox_modINFO);
            this.Controls.Add(this.OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "TSmatch Select   Выбор поставщиков";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox_modINFO.ResumeLayout(false);
            this.groupBox_modINFO.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Label modINFO;
        private System.Windows.Forms.GroupBox groupBox_modINFO;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox listBox2;
    }
}


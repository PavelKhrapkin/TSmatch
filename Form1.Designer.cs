namespace SELECT
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
            this.modINFOhdr = new System.Windows.Forms.Label();
            this.label_modINFO = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.OK = new System.Windows.Forms.Button();
            this.button_Read = new System.Windows.Forms.Button();
            this.label_supplier = new System.Windows.Forms.Label();
            this.label_totalPrice = new System.Windows.Forms.Label();
            this.label_supl = new System.Windows.Forms.Label();
            this.label_cs = new System.Windows.Forms.Label();
            this.label_suplPrice = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // modINFOhdr
            // 
            this.modINFOhdr.AutoSize = true;
            this.modINFOhdr.BackColor = System.Drawing.Color.Yellow;
            this.modINFOhdr.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.modINFOhdr.Location = new System.Drawing.Point(555, 39);
            this.modINFOhdr.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.modINFOhdr.Name = "modINFOhdr";
            this.modINFOhdr.Size = new System.Drawing.Size(125, 25);
            this.modINFOhdr.TabIndex = 0;
            this.modINFOhdr.Text = "modINFOhdr";
            this.modINFOhdr.Click += new System.EventHandler(this.modINFOhdr_Click);
            // 
            // label_modINFO
            // 
            this.label_modINFO.AutoSize = true;
            this.label_modINFO.BackColor = System.Drawing.Color.LightSkyBlue;
            this.label_modINFO.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label_modINFO.Location = new System.Drawing.Point(511, 64);
            this.label_modINFO.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_modINFO.Name = "label_modINFO";
            this.label_modINFO.Size = new System.Drawing.Size(134, 22);
            this.label_modINFO.TabIndex = 1;
            this.label_modINFO.Text = "label_modINFO";
            // 
            // listBox1
            // 
            this.listBox1.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 22;
            this.listBox1.Location = new System.Drawing.Point(16, 39);
            this.listBox1.Margin = new System.Windows.Forms.Padding(4);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(484, 466);
            this.listBox1.TabIndex = 2;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(515, 429);
            this.OK.Margin = new System.Windows.Forms.Padding(4);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(139, 62);
            this.OK.TabIndex = 3;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // button_Read
            // 
            this.button_Read.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.button_Read.Location = new System.Drawing.Point(507, 135);
            this.button_Read.Name = "button_Read";
            this.button_Read.Size = new System.Drawing.Size(120, 39);
            this.button_Read.TabIndex = 4;
            this.button_Read.Text = "Read Tekla";
            this.button_Read.UseVisualStyleBackColor = false;
            this.button_Read.Click += new System.EventHandler(this.button_Read_Click);
            // 
            // label_supplier
            // 
            this.label_supplier.AutoSize = true;
            this.label_supplier.Location = new System.Drawing.Point(517, 194);
            this.label_supplier.Name = "label_supplier";
            this.label_supplier.Size = new System.Drawing.Size(0, 25);
            this.label_supplier.TabIndex = 5;
            // 
            // label_totalPrice
            // 
            this.label_totalPrice.AutoSize = true;
            this.label_totalPrice.BackColor = System.Drawing.Color.Yellow;
            this.label_totalPrice.Location = new System.Drawing.Point(517, 400);
            this.label_totalPrice.Name = "label_totalPrice";
            this.label_totalPrice.Size = new System.Drawing.Size(64, 25);
            this.label_totalPrice.TabIndex = 6;
            this.label_totalPrice.Text = "label1";
            this.label_totalPrice.Click += new System.EventHandler(this.label_totalPrice_Click);
            // 
            // label_supl
            // 
            this.label_supl.AutoSize = true;
            this.label_supl.BackColor = System.Drawing.Color.White;
            this.label_supl.Location = new System.Drawing.Point(515, 212);
            this.label_supl.Name = "label_supl";
            this.label_supl.Size = new System.Drawing.Size(132, 25);
            this.label_supl.TabIndex = 7;
            this.label_supl.Text = "label_supplier";
            // 
            // label_cs
            // 
            this.label_cs.AutoSize = true;
            this.label_cs.Location = new System.Drawing.Point(517, 305);
            this.label_cs.Name = "label_cs";
            this.label_cs.Size = new System.Drawing.Size(93, 25);
            this.label_cs.TabIndex = 8;
            this.label_cs.Text = "label_CS";
            // 
            // label_suplPrice
            // 
            this.label_suplPrice.AutoSize = true;
            this.label_suplPrice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label_suplPrice.Location = new System.Drawing.Point(517, 344);
            this.label_suplPrice.Name = "label_suplPrice";
            this.label_suplPrice.Size = new System.Drawing.Size(184, 25);
            this.label_suplPrice.TabIndex = 9;
            this.label_suplPrice.Text = "label_totalSuplPrice";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 516);
            this.Controls.Add(this.label_suplPrice);
            this.Controls.Add(this.label_cs);
            this.Controls.Add(this.label_supl);
            this.Controls.Add(this.label_totalPrice);
            this.Controls.Add(this.label_supplier);
            this.Controls.Add(this.button_Read);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.label_modINFO);
            this.Controls.Add(this.modINFOhdr);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "TSmatch SELECT - поставщики проекта ";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label modINFOhdr;
        private System.Windows.Forms.Label label_modINFO;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button button_Read;
        private System.Windows.Forms.Label label_supplier;
        private System.Windows.Forms.Label label_totalPrice;
        private System.Windows.Forms.Label label_supl;
        private System.Windows.Forms.Label label_cs;
        private System.Windows.Forms.Label label_suplPrice;
    }
}


namespace TSmatch_SELECT
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
            this.button1 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label_modINFO = new System.Windows.Forms.Label();
            this.modINFOhdr = new System.Windows.Forms.Label();
            this.button_ReadCAD = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(865, 693);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 47);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OK_button_Click);
            // 
            // listBox1
            // 
            this.listBox1.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 22;
            this.listBox1.Location = new System.Drawing.Point(30, 46);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(713, 708);
            this.listBox1.TabIndex = 1;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // label_modINFO
            // 
            this.label_modINFO.AutoSize = true;
            this.label_modINFO.BackColor = System.Drawing.SystemColors.Highlight;
            this.label_modINFO.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_modINFO.Location = new System.Drawing.Point(776, 104);
            this.label_modINFO.Name = "label_modINFO";
            this.label_modINFO.Size = new System.Drawing.Size(122, 22);
            this.label_modINFO.TabIndex = 2;
            this.label_modINFO.Text = "label_modINFO";
            this.label_modINFO.Click += new System.EventHandler(this.label1_Click);
            // 
            // modINFOhdr
            // 
            this.modINFOhdr.AutoSize = true;
            this.modINFOhdr.BackColor = System.Drawing.Color.Yellow;
            this.modINFOhdr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.modINFOhdr.Location = new System.Drawing.Point(865, 58);
            this.modINFOhdr.Name = "modINFOhdr";
            this.modINFOhdr.Size = new System.Drawing.Size(103, 22);
            this.modINFOhdr.TabIndex = 3;
            this.modINFOhdr.Text = "modINFOhdr";
            // 
            // button_ReadCAD
            // 
            this.button_ReadCAD.Location = new System.Drawing.Point(865, 186);
            this.button_ReadCAD.Name = "button_ReadCAD";
            this.button_ReadCAD.Size = new System.Drawing.Size(116, 48);
            this.button_ReadCAD.TabIndex = 4;
            this.button_ReadCAD.Text = "ReadCAD";
            this.button_ReadCAD.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1382, 857);
            this.Controls.Add(this.button_ReadCAD);
            this.Controls.Add(this.modINFOhdr);
            this.Controls.Add(this.label_modINFO);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "TSmatch $SELECT";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label_modINFO;
        private System.Windows.Forms.Label modINFOhdr;
        private System.Windows.Forms.Button button_ReadCAD;
    }
}


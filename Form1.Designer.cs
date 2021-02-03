
namespace tx_zugferd {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.textControl1 = new TXTextControl.TextControl();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textControl1
			// 
			this.textControl1.Dock = System.Windows.Forms.DockStyle.Top;
			this.textControl1.Font = new System.Drawing.Font("Arial", 10F);
			this.textControl1.Location = new System.Drawing.Point(0, 0);
			this.textControl1.Name = "textControl1";
			this.textControl1.Size = new System.Drawing.Size(800, 363);
			this.textControl1.TabIndex = 0;
			this.textControl1.Text = "textControl1";
			this.textControl1.UserNames = null;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 381);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(164, 57);
			this.button1.TabIndex = 1;
			this.button1.Text = "Create Demo Invoice";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.textControl1);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "Creating ZUGFeRD Invoices";
			this.ResumeLayout(false);

		}

		#endregion

		private TXTextControl.TextControl textControl1;
		private System.Windows.Forms.Button button1;
	}
}


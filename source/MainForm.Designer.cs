namespace net.vieapps.MP3.HQLinks
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.label1 = new System.Windows.Forms.Label();
			this.SourceUrl = new System.Windows.Forms.TextBox();
			this.AddSource = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.SourceUrls = new System.Windows.Forms.ListBox();
			this.RemoveSource = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.DoProcess = new System.Windows.Forms.Button();
			this.AddLinksIntoIDM = new System.Windows.Forms.CheckBox();
			this.AutoDownloadFiles = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.Logs = new System.Windows.Forms.TextBox();
			this.ShowAbout = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(4, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(102, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Url của album nhạc:";
			// 
			// SourceUrl
			// 
			this.SourceUrl.Location = new System.Drawing.Point(121, 10);
			this.SourceUrl.Name = "SourceUrl";
			this.SourceUrl.Size = new System.Drawing.Size(794, 20);
			this.SourceUrl.TabIndex = 1;
			this.SourceUrl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SourceUrl_KeyPress);
			// 
			// AddSource
			// 
			this.AddSource.Location = new System.Drawing.Point(921, 8);
			this.AddSource.Name = "AddSource";
			this.AddSource.Size = new System.Drawing.Size(75, 23);
			this.AddSource.TabIndex = 2;
			this.AddSource.Text = "Thêm";
			this.AddSource.UseVisualStyleBackColor = true;
			this.AddSource.Click += new System.EventHandler(this.AddSource_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(120, 33);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(401, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Ví dụ: http://mp3.zing.vn/album/The-Best-Of-Mozart-Various-Artists/ZWZ9DUF8.html\r\n";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(4, 53);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(86, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Các url đã nhập:";
			// 
			// SourceUrls
			// 
			this.SourceUrls.FormattingEnabled = true;
			this.SourceUrls.Location = new System.Drawing.Point(121, 53);
			this.SourceUrls.Name = "SourceUrls";
			this.SourceUrls.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.SourceUrls.Size = new System.Drawing.Size(794, 95);
			this.SourceUrls.TabIndex = 4;
			// 
			// RemoveSource
			// 
			this.RemoveSource.Location = new System.Drawing.Point(921, 53);
			this.RemoveSource.Name = "RemoveSource";
			this.RemoveSource.Size = new System.Drawing.Size(75, 23);
			this.RemoveSource.TabIndex = 2;
			this.RemoveSource.Text = "Bớt";
			this.RemoveSource.UseVisualStyleBackColor = true;
			this.RemoveSource.Click += new System.EventHandler(this.RemoveSource_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(120, 151);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(662, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Mẹo: nhập sẵn url của album/bài hát cần download vào file \"MP3.HQLinks.txt\", mỗi " +
    "url trên một dọng, phần mềm sẽ tự động thêm vào đây";
			// 
			// DoProcess
			// 
			this.DoProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DoProcess.Location = new System.Drawing.Point(121, 172);
			this.DoProcess.Name = "DoProcess";
			this.DoProcess.Size = new System.Drawing.Size(200, 70);
			this.DoProcess.TabIndex = 2;
			this.DoProcess.Text = "Thực hiện";
			this.DoProcess.UseVisualStyleBackColor = true;
			this.DoProcess.Click += new System.EventHandler(this.DoProcess_Click);
			// 
			// AddLinksIntoIDM
			// 
			this.AddLinksIntoIDM.AutoSize = true;
			this.AddLinksIntoIDM.Location = new System.Drawing.Point(341, 215);
			this.AddLinksIntoIDM.Name = "AddLinksIntoIDM";
			this.AddLinksIntoIDM.Size = new System.Drawing.Size(426, 17);
			this.AddLinksIntoIDM.TabIndex = 6;
			this.AddLinksIntoIDM.Text = "Tự động đưa các liên kết tải về các file .MP3 vào IDM (Internet Download Manager)" +
    "";
			this.AddLinksIntoIDM.UseVisualStyleBackColor = true;
			// 
			// AutoDownloadFiles
			// 
			this.AutoDownloadFiles.AutoSize = true;
			this.AutoDownloadFiles.Checked = true;
			this.AutoDownloadFiles.CheckState = System.Windows.Forms.CheckState.Checked;
			this.AutoDownloadFiles.Location = new System.Drawing.Point(341, 186);
			this.AutoDownloadFiles.Name = "AutoDownloadFiles";
			this.AutoDownloadFiles.Size = new System.Drawing.Size(310, 17);
			this.AutoDownloadFiles.TabIndex = 5;
			this.AutoDownloadFiles.Text = "Tự động tải về các file .MP3 (đặt trong thư mục Downloads)";
			this.AutoDownloadFiles.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(4, 254);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(103, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "Các bước thực hiện:";
			// 
			// Logs
			// 
			this.Logs.Location = new System.Drawing.Point(121, 251);
			this.Logs.Multiline = true;
			this.Logs.Name = "Logs";
			this.Logs.Size = new System.Drawing.Size(794, 469);
			this.Logs.TabIndex = 1;
			// 
			// ShowAbout
			// 
			this.ShowAbout.Location = new System.Drawing.Point(921, 697);
			this.ShowAbout.Name = "ShowAbout";
			this.ShowAbout.Size = new System.Drawing.Size(75, 23);
			this.ShowAbout.TabIndex = 2;
			this.ShowAbout.Text = "Giới thiệu";
			this.ShowAbout.UseVisualStyleBackColor = true;
			this.ShowAbout.Click += new System.EventHandler(this.ShowAbout_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1008, 729);
			this.Controls.Add(this.AutoDownloadFiles);
			this.Controls.Add(this.AddLinksIntoIDM);
			this.Controls.Add(this.SourceUrls);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.DoProcess);
			this.Controls.Add(this.ShowAbout);
			this.Controls.Add(this.RemoveSource);
			this.Controls.Add(this.AddSource);
			this.Controls.Add(this.Logs);
			this.Controls.Add(this.SourceUrl);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MP3 HQ Links - Quynh Nguyen (Mr.) - VIEApps.net";
			this.Shown += new System.EventHandler(this.MainForm_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox SourceUrl;
		private System.Windows.Forms.Button AddSource;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListBox SourceUrls;
		private System.Windows.Forms.Button RemoveSource;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button DoProcess;
		private System.Windows.Forms.CheckBox AddLinksIntoIDM;
		private System.Windows.Forms.CheckBox AutoDownloadFiles;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox Logs;
		private System.Windows.Forms.Button ShowAbout;
	}
}


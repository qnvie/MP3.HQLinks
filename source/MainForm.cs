#region Related components
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace net.vieapps.MP3.HQLinks
{
	public partial class MainForm : Form
	{

		public MainForm()
		{
			this.InitializeComponent();
			this.AddLinksIntoIDM.Enabled = Helper.GetIDMPath() != null;
			this.LoadAlbumUrls();
		}

		#region Event handlers
		void MainForm_Shown(object sender, EventArgs e)
		{
			this.SourceUrl.Focus();
		}

		void SourceUrl_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 13)
				this.AddAlbumUrl();
		}

		void AddSource_Click(object sender, EventArgs e)
		{
			this.AddAlbumUrl();
		}

		void RemoveSource_Click(object sender, EventArgs e)
		{
			this.RemoveAlbumUrls();
		}

		void ShowAbout_Click(object sender, EventArgs e)
		{
			string msg = "MP3.HQLinks là công cụ phân tích và download nhạc chất lượng cao trực tiếp từ Zing MP3 mà không cần tài khoản VIP."
									+ "\r\n\r\n"
									+ "Tác giả: Quỳnh Nguyễn (Mr.) - VIEApps.net"
									+ "\r\n"
									+ "- Website: http://vieapps.net"
									+ "\r\n"
									+ "- Blog: http://quynhnguyen.chungta.com"
									+ "\r\n"
									+ "- Source Code: https://github.com/vieapps/MP3.HQLinks"
									+ "\r\n\r\n"
									+ "Version: 4 (Zing MP3 API v4)"
									+ "\r\n";
			MessageBox.Show(msg, "Giới thiệu", MessageBoxButtons.OK);
		}

		void DoProcess_Click(object sender, EventArgs e)
		{
			if (MainForm.IsProcessing)
			{
				DialogResult result = MessageBox.Show("Chắc chắn muốn huỷ bỏ?", "Huỷ", MessageBoxButtons.YesNo);
				if (result.Equals(DialogResult.Yes))
					MainForm.Stop();
			}
			else
				this.Start();
		}
		#endregion

		#region Add and remove url of albums
		void UpdateAlbumUrl(string url)
		{
			if (!String.IsNullOrEmpty(url) && this.SourceUrls.FindString(url) < 0)
				this.SourceUrls.Items.Add(url);
		}

		void LoadAlbumUrls()
		{
			string filePath = "MP3.HQLinks.txt";
			if (!File.Exists(filePath))
				return;

			string content = "";
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (StreamReader fileReader = new StreamReader(fileStream, Encoding.UTF8))
			{
				try
				{
					content = fileReader.ReadToEnd();
				}
				catch { }
			}

			if (String.IsNullOrEmpty(content))
				return;

			string[] links = content.Split("\r".ToCharArray());
			foreach (string link in links)
				this.UpdateAlbumUrl(link.Replace("\n", "").Trim());
		}

		void AddAlbumUrl()
		{
			string url = this.SourceUrl.Text.Trim().ToLower();
			if (url.Equals(""))
			{
				this.SourceUrl.Focus();
				return;
			}
			else if (!url.Contains("mp3.zing.vn"))
			{
				MessageBox.Show("Url của album phải nằm trong site Zing MP3", "Lỗi", MessageBoxButtons.OK);
				this.SourceUrl.Focus();
				return;
			}
			else if (!url.Contains("/mp3.zing.vn/album/") && !url.Contains("/mp3.zing.vn/playlist/"))
			{
				MessageBox.Show("Địa chỉ url phải là một album của Zing MP3  (phải chứa địa chỉ mp3.zing.vn/album/ hoặc mp3.zing.vn/playlist/)", "Lỗi", MessageBoxButtons.OK);
				this.SourceUrl.Focus();
				return;
			}

			this.UpdateAlbumUrl(this.SourceUrl.Text.Trim());
			this.SourceUrl.Focus();
			this.SourceUrl.Text = "";
		}

		void RemoveAlbumUrls()
		{
			if (this.SourceUrls.Items == null || this.SourceUrls.Items.Count < 1)
				return;

			DialogResult result = MessageBox.Show("Chắc chắn muốn xoá bớt?", "Xoá bớt", MessageBoxButtons.YesNo);
			if (!result.Equals(DialogResult.Yes))
				return;

			while (this.SourceUrls.SelectedItems.Count > 0)
			{
				int index = this.SourceUrls.FindString(this.SourceUrls.SelectedItems[0].ToString());
				if (index > -1)
					this.SourceUrls.Items.RemoveAt(index);
			}

			if (this.SourceUrls.Items.Count < 1)
				this.SourceUrl.Focus();
		}
		#endregion

		internal static Hashtable Albums = null;
		static bool IsProcessing = false;
		static CancellationTokenSource CancelToken = null;

		void Start()
		{
			// check
			if (this.SourceUrls.Items.Count < 1)
			{
				MessageBox.Show("Phải có ít nhất 01 địa chỉ của album nhạc", "Lỗi", MessageBoxButtons.OK);
				this.SourceUrl.Focus();
				return;
			}

			// prepare url of albums
			MainForm.Albums = new Hashtable();
			for (int index = 0; index < this.SourceUrls.Items.Count; index++)
				MainForm.Albums.Add(this.SourceUrls.Items[index].ToString(), "");

			// update controls
			MainForm.CancelToken = new CancellationTokenSource();
			MainForm.IsProcessing = true;
			Program.MainForm.PrepareControls(!MainForm.IsProcessing);
			this.Logs.Text = "";

			// process MP3 albums
			foreach (string uri in MainForm.Albums.Keys)
				Task.Run(async () =>
				{
					try
					{
						await Task.Delay(100);
						await MainForm.ProcessMP3AlbumAsync(uri, Program.MainForm.AutoDownloadFiles.Checked, Program.MainForm.AddLinksIntoIDM.Checked);
					}
					catch { }
				}, MainForm.CancelToken.Token).ConfigureAwait(false);

			// start monitoring thread
			Task.Run(async () =>
			{
				await MainForm.Monitor();
			}).ConfigureAwait(false);
		}

		static void Stop()
		{
			if (MainForm.CancelToken != null)
				MainForm.CancelToken.Cancel();
			MainForm.IsProcessing = false;
			Program.MainForm.UpdateLogs("----------------------------------" + "\r\n\r\n" + "Đã huỷ bỏ!" + "\r\n\r\n" + "----------------------------------" + "\r\n\r\n");
			Program.MainForm.PrepareControls(!MainForm.IsProcessing);
		}

		static async Task Monitor()
		{
			// wait for complete
			var completed = false;
			while (!completed)
			{
				if (!MainForm.IsProcessing)
					completed = true;

				else
				{
					foreach (var uri in MainForm.Albums.Keys)
					{
						var result = MainForm.Albums[uri] as string;
						completed = !String.IsNullOrEmpty(result) && (result.Equals("Completed") || result.Equals("Error"));
						if (!completed)
							break;
					}
					await Task.Delay(200);
				}
			}

			// update state
			MainForm.IsProcessing = false;
			Program.MainForm.PrepareControls(!MainForm.IsProcessing);
		}

		static async Task ProcessMP3AlbumAsync(string albumUri, bool downoadFiles, bool addLinksIntoIDM)
		{
			// check
			if (!MainForm.IsProcessing)
				return;

			// get url and update logs
			Program.MainForm.UpdateLogs("Bắt đầu phân tích thông tin từ địa chỉ \"" + albumUri + "\"\r\n");

			// process
			try
			{
				// get information of the album
				var albumInfo = albumUri.Contains("mp3.zing.vn")
					? await Helper.GetAlbumAsync(albumUri, MainForm.CancelToken.Token)
					: null;

				if (albumInfo == null)
					return;

				// re-check
				if (!MainForm.IsProcessing)
					return;

				// prepare logs
				var albumTitle = (albumInfo["title"] as JValue).Value.ToString();
				var logs = "Đã phân tích xong thông tin địa chỉ \"" + albumUri
					+ "\"\r\n"
					+ "- Tên album: " + albumTitle
					+ "\r\n";

				// get songs
				var songs = albumInfo["songs"] as JArray;
				if (songs != null && songs.Count > 0)
				{
					logs += "- Tổng cộng có " + songs.Count.ToString() + " bài hát/bản nhạc:";
					for (int songIndex = 0; songIndex < songs.Count; songIndex++)
					{
						var songInfo = songs[songIndex] as JObject;
						logs += "\r\n\t+ " + (songIndex + 1).ToString("#00") + ". " + (songInfo["title"] as JValue).Value.ToString();
					}
					logs += "\r\n";
				}

				// re-check
				if (!MainForm.IsProcessing)
					return;

				// update logs
				Program.MainForm.UpdateLogs(logs);

				using (var stream = new FileStream(@"Downloads\" + Helper.NormalizeFilename(albumTitle) + @".json", FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					var data = UTF8Encoding.UTF8.GetBytes(albumInfo.ToString(Formatting.Indented));
					stream.Write(data, 0, data.Length);
				}


				// add links into IDM
				if (addLinksIntoIDM && songs != null && songs.Count > 0)
				{
					Helper.AddMP3FilesIntoIDM(albumTitle, songs);
					Program.MainForm.UpdateLogs("- Đã đưa các bài hát/bản nhạc của album \"" + albumTitle + "\" vào IDM thành công\r\n");
				}

				// download files
				if (downoadFiles && songs != null && songs.Count > 0)
				{
					var folder = new DirectoryInfo("Downloads");
					if (!folder.Exists)
						folder.Create();

					await Helper.DownloadAsync(albumTitle, songs, MainForm.CancelToken.Token);
				}

				// update state
				MainForm.Albums[albumUri] = "Completed";
			}
			catch (Exception ex)
			{
				if (!(ex is OperationCanceledException))
					Program.MainForm.UpdateLogs("Đã xảy ra lỗi: " + ex.Message + " [" + ex.GetType().ToString() + "]" + "\r\n\r\n" + ex.StackTrace);
				MainForm.Albums[albumUri] = "Error";
			}
		}

		#region Update controls & logs
		public delegate void PrepareControlsDelegator(bool state);

		void PrepareControls(bool state)
		{
			if (base.InvokeRequired)
			{
				var method = new PrepareControlsDelegator(this.PrepareControls);
				base.Invoke(method, new object[] { state });
			}
			else
			{
				// enable/disable controls
				this.SourceUrl.Enabled = this.AddSource.Enabled 
					= this.SourceUrls.Enabled = this.RemoveSource.Enabled = this.ShowAbout.Enabled
					= this.AutoDownloadFiles.Enabled = this.AddLinksIntoIDM.Enabled = state;

				// IDM
				if (this.AddLinksIntoIDM.Enabled && Helper.GetIDMPath() == null)
					this.AddLinksIntoIDM.Enabled = this.AddLinksIntoIDM.Checked = false;

				// logs
				this.Logs.ReadOnly = !state;

				// do button
				this.DoProcess.Text = !state ? "Huỷ bỏ" : "Thực hiện";

				// update controls
				if (state)
				{
					this.SourceUrl.Focus();
					this.SourceUrls.Items.Clear();
				}
			}
		}

		public delegate void UpdateLogsDelegator(string logs);

		internal void UpdateLogs(string logs)
		{
			if (base.InvokeRequired)
			{
				var method = new UpdateLogsDelegator(this.UpdateLogs);
				base.Invoke(method, new object[] { logs });
			}
			else
			{
				this.Logs.Text = this.Logs.Text + logs + "\r\n";
				this.Logs.SelectionStart = this.Logs.TextLength;
				this.Logs.ScrollToCaret();
			}
		}
		#endregion

	}
}
#region Related components
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace net.vieapps.MP3.HQLinks
{

	internal static class Helper
	{

		#region Get external resource/webpage via HttpWebRequest object
		internal static CredentialCache GetWebCredential(string uri, string account, string password, bool useSecureProtocol, SecurityProtocolType secureProtocol)
		{
			if (useSecureProtocol)
				ServicePointManager.SecurityProtocol = secureProtocol;

			var credentialCache = new CredentialCache();
			credentialCache.Add(new Uri(uri), "Basic", new NetworkCredential(account, password));
			return credentialCache;
		}

		internal static WebProxy GetWebProxy(string proxyHost, int proxyPort, string proxyUsername, string proxyUserPassword, string[] proxyBypassList)
		{
			WebProxy proxy = null;
			if (!String.IsNullOrEmpty(proxyHost))
			{
				if (proxyBypassList == null || proxyBypassList.Length < 1)
					proxy = new WebProxy(proxyHost, proxyPort);
				else
				{
					var proxyAddress = new Uri("http://" + proxyHost + ":" + proxyPort.ToString());
					proxy = new WebProxy(proxyAddress, true, proxyBypassList);
				}
				if (!String.IsNullOrEmpty(proxyUsername) && !String.IsNullOrEmpty(proxyUserPassword))
					proxy.Credentials = new NetworkCredential(proxyUsername, proxyUserPassword);
			}
			return proxy;
		}

		public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest httpRequest, CancellationToken ct)
		{
			using (ct.Register(() => httpRequest.Abort(), useSynchronizationContext: false))
			{
				try
				{
					return await httpRequest.GetResponseAsync() as HttpWebResponse;
				}
				catch (WebException ex)
				{
					if (ct.IsCancellationRequested)
						throw new OperationCanceledException(ex.Message, ex, ct);
					else
						throw ex;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		internal static async Task<HttpWebResponse> GetWebResponseAsync(string method, string uri, Dictionary<string, string> headers = null, Cookie[] cookies = null, string body = null, string contentType = null, int timeout = 90, string userAgent = null, CredentialCache credential = null, WebProxy proxy = null, CancellationToken ct = default(CancellationToken))
		{
			// get the request object to handle on the remote resource
			var request = WebRequest.Create(uri) as HttpWebRequest;

			// set the properties
			request.Method = String.IsNullOrEmpty(method)
				? "GET"
				: method.ToUpper();
			request.Timeout = timeout * 1000;
			request.ServicePoint.Expect100Continue = false;
			request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			request.UserAgent = String.IsNullOrEmpty(userAgent)
				? "Mozilla/5.0 (iPhone; CPU iPhone OS 8_3 like Mac OS X; en-us) AppleWebKit/600.1.4 (KHTML, like Gecko) Version/8.0 Mobile/12F70 Safari/600.1.4"
				: userAgent;

			// headers
			if (headers != null)
				foreach (var header in headers)
					if (!header.Key.Equals("Accept-Encoding"))
						request.Headers.Add(header.Key, header.Value);

			// cookies
			if (cookies != null && cookies.Length > 0 && request.SupportsCookieContainer)
			{
				if (request.CookieContainer == null)
					request.CookieContainer = new CookieContainer();
				foreach (var cookie in cookies)
					request.CookieContainer.Add(cookie);
			}

			// compression
			request.Headers.Add("Accept-Encoding", "deflate,gzip");
			request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

			// credential
			if (credential != null)
			{
				request.Credentials = credential;
				request.PreAuthenticate = true;
			}

			// proxy
			if (proxy != null)
				request.Proxy = proxy;

			// data to post/put
			if ((method.Equals("POST") || method.Equals("PUT")) && !String.IsNullOrEmpty(body))
			{
				if (!String.IsNullOrEmpty(contentType))
					request.ContentType = contentType;

				using (var writer = new StreamWriter(await request.GetRequestStreamAsync()))
				{
					await writer.WriteAsync(body);
					writer.Close();
				}
			}

			// switch off certificate validation (http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-using)
			ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

			// make request and return response stream
			try
			{
				return await request.GetResponseAsync(ct);
			}
			catch (SocketException ex)
			{
				if (ex.Message.Contains("did not properly respond after a period of time"))
					throw new ConnectionTimeoutException(ex.InnerException);
				else
					throw ex;
			}
			catch (WebException ex)
			{
				string responseBody = "";
				if (ex.Status.Equals(WebExceptionStatus.ProtocolError))
				{
					using (var stream = (ex.Response as HttpWebResponse).GetResponseStream())
					{
						using (var reader = new StreamReader(stream, true))
						{
							responseBody = await reader.ReadToEndAsync();
						}
					}
				}
				throw new RemoteServerErrorException("Error occurred at remote server", responseBody, ex.Response != null && ex.Response.ResponseUri != null ? ex.Response.ResponseUri.AbsoluteUri : uri, ex);
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				request = null;
			}
		}

		internal static async Task<HttpWebResponse> GetWebResponseAsync(string method, string uri, Dictionary<string, string> headers, Cookie[] cookies, string body, string contentType, int timeout, string userAgent, string credentialAccount, string credentialPassword, bool useSecureProtocol, SecurityProtocolType secureProtocol, WebProxy proxy = null, CancellationToken ct = default(CancellationToken))
		{
			// credential
			CredentialCache credential = null;
			if (!String.IsNullOrEmpty(credentialAccount) && !String.IsNullOrEmpty(credentialPassword))
				credential = Helper.GetWebCredential(uri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol);

			// make request
			return await Helper.GetWebResponseAsync(method, uri, headers, cookies, body, contentType, timeout, userAgent, credential, proxy, ct);
		}

		internal static Task<HttpWebResponse> GetWebResponseAsync(string method, string uri, int timeout, CancellationToken ct = default(CancellationToken))
		{
			return Helper.GetWebResponseAsync(method, uri, null, null, null, null, timeout, null, null, null, false, SecurityProtocolType.Ssl3, null, ct);
		}

		internal static async Task<Tuple<string, List<Tuple<string, string>>>> GetWebPageAsync(string uri, Dictionary<string, string> headers = null, Cookie[] cookies = null, int timeout = 90, string userAgent = null, string credentialAccount = null, string credentialPassword = null, bool useSecureProtocol = false, SecurityProtocolType secureProtocol = SecurityProtocolType.Ssl3, WebProxy proxy = null, CancellationToken ct = default(CancellationToken))
		{
			// check uri
			if (String.IsNullOrEmpty(uri))
				return null;

			// get stream of external resource as HTML
			string html = "";
			var responseHeaders = new List<Tuple<string, string>>();
			using (var response = await Helper.GetWebResponseAsync("GET", uri, headers, cookies, null, null, timeout, userAgent, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, proxy, ct))
			{
				for (var index = 0; index < response.Headers.Count; index++)
					responseHeaders.Add(new Tuple<string, string>(response.Headers.Keys[index], response.Headers.Get(index)));
				using (var stream = response.GetResponseStream())
				{
					using (var reader = new StreamReader(stream, true))
					{
						html = reader.ReadToEnd();
					}
				}
			}

			// decode and return HTML
			return new Tuple<string, List<Tuple<string, string>>>(WebUtility.HtmlDecode(html), responseHeaders);
		}

		internal static Task<Tuple<string, List<Tuple<string, string>>>> GetWebPageAsync(string uri, Cookie[] cookies, CancellationToken ct = default(CancellationToken))
		{
			return Helper.GetWebPageAsync(uri, null, cookies, 90, null, null, null, false, SecurityProtocolType.Ssl3, null, ct);
		}

		internal static async Task<string> GetWebPageAsync(string uri, CancellationToken ct = default(CancellationToken))
		{
			var results = await Helper.GetWebPageAsync(uri, null, ct);
			return results.Item1;
		}
		#endregion

		#region Normalize filename
		static string ConvertVietnameseString(string @string, int mode)
		{
			string utf8Literal = "Ã  Ã¡ áº£ Ã£ áº¡ Ã€ Ã áº¢ Ãƒ áº  Ã¢ áº§ áº¥ áº© áº« áº­ Ã‚ áº¦ áº¤ áº¨ áºª áº¬ Äƒ áº± áº¯ áº³ áºµ áº· Ä‚ áº° áº® áº² áº´ áº¶ "
												+ "Ã² Ã³ á» Ãµ á» Ã’ Ã“ á»Ž Ã• á»Œ Ã´ á»“ á»‘ á»• á»— á»™ Ã” á»’ á» á»” á»– á»˜ Æ¡ á» á»› á»Ÿ á»¡ á»£ Æ  á»œ á»š á»ž á»  á»¢ "
												+ "Ã¨ Ã© áº» áº½ áº¹ Ãˆ Ã‰ áºº áº¼ áº¸ Ãª á» áº¿ á»ƒ á»… á»‡ ÃŠ á»€ áº¾ á»‚ á»„ á»† "
												+ "Ã¹ Ãº á»§ Å© á»¥ Ã™ Ãš á»¦ Å¨ á»¤ Æ° á»« á»© á»­ á»¯ á»± Æ¯ á»ª á»¨ á»¬ á»® á»° "
												+ "Ã¬ Ã­ á»‰ Ä© á»‹ ÃŒ Ã á»ˆ Ä¨ á»Š á»³ Ã½ á»· á»¹ á»µ á»² Ã á»¶ á»¸ á»´ Ä‘ Ä "
												+ "â€œ â€ â€“ Ã â€™ Á ÇŽ â€¦ aI";
			string[] utf8Literals = utf8Literal.Split(' ');

			string utf8Unicode = "à á ả ã ạ À Á Ả Ã Ạ â ầ ấ ẩ ẫ ậ Â Ầ Ấ Ẩ Ẫ Ậ ă ằ ắ ẳ ẵ ặ Ă Ằ Ắ Ẳ Ẵ Ặ "
												+ "ò ó ỏ õ ọ Ò Ó Ỏ Õ Ọ ô ồ ố ổ ỗ ộ Ô Ồ Ố Ổ Ỗ Ộ ơ ờ ớ ở ỡ ợ Ơ Ờ Ớ Ở Ỡ Ợ "
												+ "è é ẻ ẽ ẹ È É Ẻ Ẽ Ẹ ê ề ế ể ễ ệ Ê Ề Ế Ể Ễ Ệ "
												+ "ù ú ủ ũ ụ Ù Ú Ủ Ũ Ụ ư ừ ứ ử ữ ự Ư Ừ Ứ Ử Ữ Ự "
												+ "ì í ỉ ĩ ị Ì Í Ỉ Ĩ Ị ỳ ý ỷ ỹ ỵ Ỳ Ý Ỷ Ỹ Ỵ đ Đ "
												+ "“ ” – Á ’ Đ ă &nbsp; á";
			string[] utf8Unicodes = utf8Unicode.Split(' ');

			string utf8UnicodeComposite = "à á ả ã ạ À Á Ả Ã Ạ â ầ ấ ẩ ẫ ậ Â Ầ Ấ Ẩ Ẫ Ậ ă ằ ắ ẳ ẵ ặ Ă Ằ Ắ Ẳ Ẵ Ặ "
												+ "ò ó ỏ õ ọ Ò Ó Ỏ Õ Ọ ô ồ ố ổ ỗ ộ Ô Ồ Ố Ổ Ỗ Ộ ơ ờ ớ ở ỡ ợ Ơ Ờ Ớ Ở Ỡ Ợ "
												+ "è é ẻ ẽ ẹ È É Ẻ Ẽ Ẹ ê ề ế ể ễ ệ Ê Ề Ế Ể Ễ Ệ "
												+ "ù ú ủ ũ ụ Ù Ú Ủ Ũ Ụ ư ừ ứ ử ữ ự Ư Ừ Ứ Ử Ữ Ự "
												+ "ì í ỉ ĩ ị Ì Í Ỉ Ĩ Ị ỳ ý ỷ ỹ ỵ Ỳ Ý Ỷ Ỹ Ỵ đ Đ "
												+ "“ ” – Á ’ Đ ă &nbsp; á";
			string[] utf8UnicodeComposites = utf8UnicodeComposite.Split(' ');

			string ansi = "a a a a a A A A A A a a a a a a A A A A A A a a a a a a A A A A A A "
									+ "o o o o o O O O O O o o o o o o O O O O O O o o o o o o O O O O O O "
									+ "e e e e e E E E E E e e e e e e E E E E E E "
									+ "u u u u u U U U U U u u u u u u U U U U U U "
									+ "i i i i i I I I I I y y y y y Y Y Y Y Y d D "
									+ "\" \" - A ' D a &nbsp; a";
			string[] ansis = ansi.Split(' ');

			string decimalUnicode = "&#224; &#225; &#7843; &#227; &#7841; &#192; &#193; &#7842; &#195; &#7840; &#226; &#7847; &#7845; &#7849; &#7851; &#7853; &#194; &#7846; &#7844; &#7848; &#7850; &#7852; &#259; &#7857; &#7855; &#7859; &#7861; &#7863; &#258; &#7856; &#7854; &#7858; &#7860; &#7862; "
									+ "&#242; &#243; &#7887; &#245; &#7885; &#210; &#211; &#7886; &#213; &#7884; &#244; &#7891; &#7889; &#7893; &#7895; &#7897; &#212; &#7890; &#7888; &#7892; &#7894; &#7896; &#417; &#7901; &#7899; &#7903; &#7905; &#7907; &#416; &#7900; &#7898; &#7902; &#7904; &#7906; "
									+ "&#232; &#233; &#7867; &#7869; &#7865; &#200; &#201; &#7866; &#7868; &#7864; &#234; &#7873; &#7871; &#7875; &#7877; &#7879; &#202; &#7872; &#7870; &#7874; &#7876; &#7878; "
									+ "&#249; &#250; &#7911; &#361; &#7909; &#217; &#218; &#7910; &#360; &#7908; &#432; &#7915; &#7913; &#7917; &#7919; &#7921; &#431; &#7914; &#7912; &#7916; &#7918; &#7920; "
									+ "&#236; &#237; &#7881; &#297; &#7883; &#204; &#205; &#7880; &#296; &#7882; &#7923; &#253; &#7927; &#7929; &#7925; &#7922; &#221; &#7926; &#7928; &#7924; &#273; &#272; "
									+ "&#34; &#34; &#45; &#224; &#39; &#272; &#259; &#32; &#225;";
			string[] decimalUnicodes = decimalUnicode.Split(' ');

			string tcvn3 = "µ ¸ ¶ · ¹ µ ¸ ¶ · ¹ © Ç Ê È É Ë ¢ Ç Ê È É Ë ¨ » ¾ ¼ ½ Æ ¡ » ¾ ¼ ½ Æ "
										+ "ß ã á â ä ß ã á â ä « å è æ ç é ¤ å è æ ç é ¬ ê í ë ì î ¥ ê í ë ì î "
										+ "Ì Ð Î Ï Ñ Ì Ð Î Ï Ñ ª Ò Õ Ó Ô Ö £ Ò Õ Ó Ô Ö "
										+ "ï ó ñ ò ô ï ó ñ ò ô ­ õ ø ö ÷ ù ¦ õ ø ö ÷ ù "
										+ "× Ý Ø Ü Þ × Ý Ø Ü Þ ú ý û ü þ ú ý û ü þ ® § "
										+ "“ ” – ¸ ’ § ¨ &nbsp; ¸";
			string[] tcvn3s = tcvn3.Split(' ');

			// convert
			int decodeLength = -1;
			string result = @string.Trim();

			switch (mode)
			{
				case 0:				// UTF8 Literal to unicode
					decodeLength = utf8Unicodes.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (index < utf8Literals.Length)
							result = result.Replace(utf8Literals[index], utf8Unicodes[index]);
					}
					break;

				case 1:				// Unicode to UTF8 Literal
					decodeLength = utf8Literals.Length;
					for (int index = 0; index < decodeLength; index++)
						result = result.Replace(utf8Unicodes[index], utf8Literals[index]);
					break;

				case 2:				// Unicode to ANSI
					decodeLength = utf8Unicodes.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (index < ansis.Length)
							result = result.Replace(utf8Unicodes[index], ansis[index]);
					}
					break;

				case 3:				// UTF8 Literal to ANSI
					decodeLength = utf8Literals.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (index < ansis.Length)
							result = result.Replace(utf8Literals[index], ansis[index]);
					}
					break;

				case 4:				// Unicode to Decimal
					decodeLength = utf8Unicodes.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (index < decimalUnicodes.Length)
							result = result.Replace(utf8Unicodes[index], decimalUnicodes[index]);
					}
					break;

				case 5:				// Unicode Composite to ANSI
					decodeLength = utf8UnicodeComposites.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (index < ansis.Length)
							result = result.Replace(utf8UnicodeComposites[index], ansis[index]);
					}
					break;

				case 6:				// Decimal to Unicode
					decodeLength = decimalUnicodes.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (index < utf8Unicodes.Length)
							result = result.Replace(decimalUnicodes[index], utf8Unicodes[index]);
					}
					break;

				case 7:				// TCVN3 to Unicode
					// first, convert to decimal
					decodeLength = tcvn3s.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (tcvn3s[index].Equals(""))
							continue;
						if (index < decimalUnicodes.Length)
							result = result.Replace(tcvn3s[index], decimalUnicodes[index]);
					}
					// and then, convert from decimal to unicode
					decodeLength = decimalUnicodes.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (index < utf8Unicodes.Length)
							result = result.Replace(decimalUnicodes[index], utf8Unicodes[index]);
					}
					break;

				case 8:				// Unicode to Composite Unicode
					decodeLength = utf8Unicodes.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (index < utf8UnicodeComposites.Length)
							result = result.Replace(utf8Unicodes[index], utf8UnicodeComposites[index]);
					}
					break;

				case 9:				// Composite Unicode to Unicode
					decodeLength = utf8UnicodeComposites.Length;
					for (int index = 0; index < decodeLength; index++)
					{
						if (index < utf8Unicodes.Length)
							result = result.Replace(utf8UnicodeComposites[index], utf8Unicodes[index]);
					}
					break;

				default:
					break;
			}

			return result.Replace("&nbsp;", " ");
		}

		internal static string NormalizeFilename(string filename)
		{
			string wellFilename = Helper.ConvertVietnameseString(filename, 2);
			wellFilename = Helper.ConvertVietnameseString(wellFilename, 5);

			return String.IsNullOrEmpty(wellFilename)
					? null
					: wellFilename.Trim().Replace(@"\", "").Replace("/", "").Replace("*", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("|", "").Replace("%20", " ").Replace(":", "").Replace(" ft. ", " & ").Replace("\"", "'");
		}
		#endregion

		#region Parse MP3 album
		internal static async Task<JObject> GetAlbumAsync(string uri, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();

			if (String.IsNullOrEmpty(uri) || !uri.Contains("mp3.zing.vn"))
				return null;

			var response = await Helper.GetWebPageAsync(uri, null, ct);

			var albumHtml = response.Item1;
			if (albumHtml.Trim().Equals(""))
				return null;

			var start = -1;
			var end = -1;
			string title = null;

			start = albumHtml.IndexOf("<h1 class=\"txt-primary");
			if (start > 0)
			{
				start = albumHtml.IndexOf(">", start);
				end = albumHtml.IndexOf("</h1>", start);
				title = albumHtml.Substring(start + 1, end - start - 1).Trim();
			}

			start = albumHtml.IndexOf("data-xml=\"");
			if (start < 0)
				return null;

			start += 10;
			end = albumHtml.IndexOf("\"", start);
			if (end < 0)
				return null;

			var albumUri = albumHtml.Substring(start, end - start);

			List<Cookie> cookies = new List<Cookie>();
			response.Item2.Where(item => item.Item1.Equals("Set-Cookie")).ToList().ForEach(info =>
			{
				start = info.Item2.IndexOf("=");
				end = info.Item2.IndexOf(";", start + 1);
				cookies.Add(new Cookie()
				{
					Name = info.Item2.Substring(0, start),
					Value = info.Item2.Substring(start + 1, end - start - 1),
					Path = "/",
					Domain = "mp3.zing.vn"					
				});
			});

			response = await Helper.GetWebPageAsync(albumUri, cookies.Count > 0 ? cookies.ToArray() : null, ct);
			var albumJson = response.Item1;
			if (albumJson.Trim().Equals(""))
				return null;

			var json = JObject.Parse(albumJson);
			var songs = json["data"] as JArray;
			var tasks = new List<Task<JObject>>();
			foreach (var song in songs)
			{
				var id = (song["id"] as JValue).Value.ToString();
				tasks.Add(Helper.GetSongAsync(id, ct));
			}
			await Task.WhenAll(tasks);

			songs = new JArray();
			foreach (var task in tasks)
				songs.Add(task.Result);

			return new JObject() {
				{ "title", title },
				{ "uri", uri },
				{ "songs", songs }
			};
		}

		static async Task<JObject> GetSongAsync(string id, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();

			var uri = "http://api.mp3.zing.vn/api/mobile/song/getsonginfo?requestdata={\"id\":\"[id]\"}";
			var jsonData = await Helper.GetWebPageAsync(uri.Replace("[id]", id).Replace("\"", "%22"), ct);

			var json = JObject.Parse(jsonData);
			var title = (json["title"] as JValue).Value.ToString();

			//json = json["link_download"] as JObject;
			json = json["source"] as JObject;
			//uri = json["320"] != null ? (json["320"] as JValue).Value.ToString() : (json["128"] as JValue).Value.ToString();

			return new JObject() {
				{ "id", id },
				{ "title", title },
				{ "uri", json },
				{ "filename", Helper.NormalizeFilename(title) + ".mp3" }
			};
		}
		#endregion

		#region Download .MP3 files
		internal static async Task DownloadAsync(string album, JArray songs, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();

			Program.MainForm.UpdateLogs("- Bắt đầu download các bài hát/bản nhạc của album \"" + album + "\"\r\n");

			var folderPath = @"Downloads\" + Helper.NormalizeFilename(album) + @"\";
			var folder = new DirectoryInfo(folderPath);
			if (!folder.Exists)
				folder.Create();

			var tasks = new List<Task>();
			var counter = 0;
			foreach (var songInfo in songs)
			{
				counter++;
				var title = (songInfo["title"] as JValue).Value.ToString();
				var uri = songInfo["uri"] as JObject;
				var uri128 = (uri["128"] as JValue).Value.ToString();
				var uri320 = (uri["320"] as JValue).Value.ToString();
				var filename = (songInfo["filename"] as JValue).Value.ToString();
				if (filename.Equals(""))
					filename = Helper.NormalizeFilename(album + " - " + title);
				filename = counter.ToString("#00") + ". " + filename + (!filename.EndsWith("mp3") ? ".mp3" : "");

				//if (counter < 2)
				tasks.Add(Helper.DownloadMP3FileAsync(title, uri128, uri320, folderPath, filename, ct));
			}

			await Task.WhenAll(tasks);
			Program.MainForm.UpdateLogs("- Download các bài hát/bản nhạc của album \"" + album + "\" thành công\r\n");
		}

		static async Task DownloadMP3FileAsync(string title, string uri128, string uri320, string folderPath, string filename, CancellationToken ct)
		{
			await Task.Delay(321);
			try
			{
				var downloadUri = await Helper.GetMP3FileUriAsync(uri320, ct);
				using (var response = await Helper.GetWebResponseAsync("GET", downloadUri, 600, ct))
				{
					using (var webStream = response.GetResponseStream())
					{
						using (var fileStream = new FileStream(folderPath + filename, FileMode.Create, FileAccess.Write, FileShare.Read))
						{
							await webStream.CopyToAsync(fileStream, 4096, ct);
						}
					}
				}
				Program.MainForm.UpdateLogs("- Download bài hát/bản nhạc \"" + title + "\" thành công\r\n");
			}
			catch
			{
				try
				{
					var downloadUri = await Helper.GetMP3FileUriAsync(uri128, ct);
					using (var response = await Helper.GetWebResponseAsync("GET", downloadUri, 600, ct))
					{
						using (var webStream = response.GetResponseStream())
						{
							using (var fileStream = new FileStream(folderPath + filename, FileMode.Create, FileAccess.Write, FileShare.Read))
							{
								await webStream.CopyToAsync(fileStream, 4096, ct);
							}
						}
					}
					Program.MainForm.UpdateLogs("- Download bài hát/bản nhạc \"" + title + "\" thành công\r\n");
				}
				catch (Exception ex)
				{
					Program.MainForm.UpdateLogs("- Error occurred while downloading \"" + title + "\": " + ex.Message + "\r\n-Stack:" + ex.StackTrace + "\r\n\r\n");
				}
			}
		}

		static async Task<string> GetMP3FileUriAsync(string uri, CancellationToken ct)
		{
			try
			{
				using (var webResponse = await Helper.GetWebResponseAsync("GET", uri, null, null, null, null, 45, null, null, null, ct))
				{
					return webResponse != null && webResponse.ResponseUri != null
						? webResponse.ResponseUri.ToString()
						: null;
				}
			}
			catch
			{
				return null;
			}
		}
		#endregion

		#region Working with IDM
		static string _IDMPath = null;

		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll")]
		private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags, StringBuilder lpExeName, out int size);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr hHandle);

		[Flags]
		private enum ProcessAccessFlags : uint
		{
			All = 0x1f0fff,
			CreateThread = 2,
			DupHandle = 0x40,
			QueryInformation = 0x400,
			ReadControl = 0x20000,
			SetInformation = 0x200,
			Synchronize = 0x100000,
			Terminate = 1,
			VMOperation = 8,
			VMRead = 0x10,
			VMWrite = 0x20
		}

		internal static string GetIDMPath()
		{
			if (Helper._IDMPath == null)
			{
				Process[] processes = Process.GetProcesses();
				if ((processes != null) && (processes.Length > 0))
					foreach (Process process in processes)
					{
						int id = process.Id;
						StringBuilder lpExeName = new StringBuilder(0x400);
						IntPtr hprocess = Helper.OpenProcess(ProcessAccessFlags.QueryInformation, false, id);
						if (hprocess != IntPtr.Zero)
						{
							try
							{
								int capacity = lpExeName.Capacity;
								if (Helper.QueryFullProcessImageName(hprocess, 0, lpExeName, out capacity))
								{
									string processName = lpExeName.ToString();
									if (processName.EndsWith("IDMan.exe"))
									{
										Helper._IDMPath = processName;
										break;
									}
								}
							}
							catch { }
							finally
							{
								Helper.CloseHandle(hprocess);
							}
						}
					}
			}
			return Helper._IDMPath;
		}

		internal static void AddMP3FilesIntoIDM(string album, JArray songs)
		{
			// get path of IDMan.exe
			string idmPath = Helper.GetIDMPath();
			if (idmPath == null)
				return;

			// normalize folder name
			string folder = "";
			if (album != null && !album.Trim().Equals(""))
				folder = Helper.NormalizeFilename(album).Replace(".", "") + "\\";

			// add songs into IDM
			int counter = 0;
			foreach (JObject songInfo in songs)
			{
				// prepare
				counter++;
				string title = (songInfo["title"] as JValue).Value.ToString();
				string url = (songInfo["uri"] as JValue).Value.ToString();
				string filename = (songInfo["filename"] as JValue).Value.ToString();
				if (filename.Equals(""))
					filename = Helper.NormalizeFilename(album) + "-" + counter.ToString();
				filename = counter.ToString("#00") + ". " + filename + (!filename.EndsWith("mp3") ? ".mp3" : "");

				// add song into IDM via command line
				ProcessStartInfo info = new ProcessStartInfo
				{
					FileName = idmPath,
					Arguments = "/a /s /n /d \"" + url + "\" /f \"" + folder + filename + "\"",
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					StandardOutputEncoding = Encoding.Unicode,
					RedirectStandardError = false,
					UseShellExecute = false,
					ErrorDialog = false
				};
				new Process { StartInfo = info, EnableRaisingEvents = false }.Start();
				Thread.Sleep(50);
			}
		}
		#endregion

	}

	#region Exceptions
	[Serializable]
	public class ConnectionTimeoutException : Exception
	{
		public ConnectionTimeoutException() : base("Connection timeout") { }

		public ConnectionTimeoutException(Exception innerException) : base("Connection timeout", innerException) { }

		public ConnectionTimeoutException(string message, Exception innerException) : base(message, innerException) { }

		public ConnectionTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class RemoteServerErrorException : Exception
	{
		public string ResponseBody { get; internal set; }

		public string ResponseUri { get; internal set; }

		public RemoteServerErrorException() : base("Error occured while operating with remote server") { }

		public RemoteServerErrorException(Exception innerException) : base("Error occurred while operating with the remote server", innerException) { }

		public RemoteServerErrorException(string message, Exception innerException) : base(message, innerException) { }

		public RemoteServerErrorException(string message, string responseBody, Exception innerException) : this(message, responseBody, "", innerException) { }

		public RemoteServerErrorException(string message, string responseBody, string responseUri, Exception innerException) : base(message, innerException)
		{
			this.ResponseBody = responseBody;
			this.ResponseUri = responseUri;
		}

		public RemoteServerErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
	#endregion

}
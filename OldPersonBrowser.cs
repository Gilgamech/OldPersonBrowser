
//{using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
//}

namespace OldPersonNamespace {
    public class OldPersonBrowser : Form {
//{ Ints
        public int version = 250;
        public Button goButton;
		public TextBox urlBox = new TextBox();
		public RichTextBox outBox = new RichTextBox();
		public System.Drawing.Bitmap myBitmap;
		public System.Drawing.Graphics pageGraphics;
		public Panel pagePanel;
		public ContextMenuStrip contextMenu1;
		
		public string[] history = new string[0];
		//public List<string> history = new List<string>();
		public int historyIndex = 0;
		public string[] parsedHtml = new string[1];
		public string defaultSite = "http://www.n-gate.com/";
		//public string defaultSite = "http://www.Gilgamech.com/";
		public string appTitle = "Old Person Browser - Build ";
		public int displayLine = 0;
		public int goButtonWidth = 60;
		public int urlBoxHeight = 30;
		public int sideBufferWidth = 0;
			//outBox.Font = new Font("Calibri", 14);
		public int lineHeight = 14;
		public int WindowWidth = 600;
		public int WindowHeight = 300;

		public bool debuggingView = false;

//}
/* Lawnmower rendering engine
Add panels and put images & lines and shapes on (in?) them.
Fill outBox with the background color 
Set up tab indices
Add wordwrap etc to the menu
Add right-click menu
DOM says to use a object called "document" that has all the objects dotted off it.
*/

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new OldPersonBrowser());
        }// end Main

        public OldPersonBrowser() {
			this.Text = appTitle + version;
			this.Size = new Size(WindowWidth,WindowHeight);
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Resize += new System.EventHandler(this.OnResize);
			this.AutoScroll = true;
			this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
			Array.Resize(ref history, history.Length + 2);
			history[historyIndex] = "about:blank";
			historyIndex++;


			drawMenuBar();
			drawUrlBoxAndGoButton();
			drawOutBox();
   
        } // end OldPersonBrowser

//{Functions
		

		//Main
        public void loadNewPage() {

//Download HTML file
//Parse HTML to Document variable
//Write Document variable to page
//Interpret Javascript to modify Document variable
			//history.Add(urlBox.Text);
			history[historyIndex] = urlBox.Text;
			string imageUrl = "";
			string pageSource = "";
			displayLine = 0;
			// Download website, stick source in pageSource
			xhrRequest(ref pageSource, WebRequestMethods.Http.Get);

			// Do some replacing
			doSomeReplacing(ref pageSource);
			
			// Set form name to page title
			try {
				//this.Text = pageSource.Substring(pageSource.IndexOf("<title>")+7, pageSource.IndexOf("</title>") - pageSource.IndexOf("<title>")-7);
				this.Text = findIndexOf(pageSource,"<title>","</title>",7,-7);
			}catch{
				this.Text = appTitle + version;
			}// end try 

			
			//favicon 
			try {
				// <link rel="shortcut icon" href="/favicon.ico" type="image/vnd.microsoft.icon">
				//imageUrl = pageSource.Substring(pageSource.IndexOf("<link")+5, pageSource.IndexOf(">") - pageSource.IndexOf("<link"));
				imageUrl = findIndexOf(pageSource,"<link",">",5,0);
				//imageUrl = imageUrl.Substring(imageUrl.IndexOf("href=")+6, imageUrl.IndexOf('"') - imageUrl.IndexOf("href="));
				imageUrl = findIndexOf(imageUrl,"href=","",6,0);
				
			}catch{
				imageUrl = history[historyIndex] + "/favicon.ico";
			}// end try 
			try {
/*
using(Stream stream = Application.GetResourceStream(new Uri(imageUrl)).Stream)
{
    Icon myIcon = new System.Drawing.Icon(stream);
}
				WebClient client = new WebClient();
				Stream stream = client.OpenRead(imageUrl);
				stream.Flush();
				stream.Close();
				//this.Text += "Favicon: "+imageUrl;
*/

				WebRequest request = WebRequest.Create(imageUrl);
				request.Method = WebRequestMethods.Http.Get;// WebRequestMethods.Http.Get;
				//request.UserAgent = "OldPersonBrowser";
				WebResponse response = request.GetResponse();
				Stream stream = response.GetResponseStream();
				this.Icon = new Icon(stream);
				//pictureBox1.Image = Bitmap.FromStream(stream);

			}catch{
				//this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
				//this.Text = "Favicon missing:"+imageUrl+" - " + this.Text;
			}// end try 

			// Split head & body 
			//Goto <body then goto the next >
			try {
				//parsedHtml = pageSource.Substring(pageSource.IndexOf("<body"), pageSource.IndexOf("</body") - pageSource.IndexOf("<body")).Split('<');
				parsedHtml = findIndexOf(pageSource,"<body","</body",0,0).Split('<');
			}catch{
				parsedHtml = pageSource.Split('<');
			} // end try 
			drawPage(parsedHtml);
        }// end loadNewPage

		public string findIndexOf(string pageString,string startString,string endString,int startPlus,int endPlus){
			return pageString.Substring(pageString.IndexOf(startString)+startPlus, pageString.IndexOf(endString) - pageString.IndexOf(startString)+endPlus);
        }// end findIndexOf

		public void drawPage(string[] parsedHtml){
			//pagePanel.Paint += new PaintEventHandler(drawPanel);
			string tag = "div";
			//string buttonText = "";
			
			int werdStart = 0;
			int werdSpace = 0;
			int werdEnd = 0;
			
			
			//Should delete outBox and make a new one? This is easier.
			outBox.Height = ClientRectangle.Height - urlBoxHeight;
			outBox.Controls.Clear();
			outBox.Text = "";
			
			foreach (string werd in parsedHtml){
				outBox.Height += lineHeight; // 40? And add multples for word wrap?
				string append = "";
				werdSpace = 0;
				
				if (werd.IndexOf(">") >=0 ) {
					werdStart = werd.IndexOf(">")+1;
				} else {
					werdStart = 0;
				}
				
				if (werd.IndexOf("<") >=0 ) {
					werdEnd = werd.IndexOf("<");
				} else {
					werdEnd = werd.Length;
				}
				
				
				if (werd.IndexOf(" ") >=0) {
					werdSpace = werd.IndexOf(" ");
					if (werdSpace >= werdStart) {//wordStart.index > wordSpace.index (larger is after)
						try {
							tag = werd.Substring(0,werdStart);
						} catch {
							tag = "werdIndex:" +werd.IndexOf(">")+ "-werdSpace:"+werdSpace;
						}// end try
					}
					if (werdSpace <= werdStart) {//wordStart.index < wordSpace.index (smaller is before)
						try {
							tag = werd.Substring(0,werdSpace);
						} catch {
							tag = "werdIndex:" +werd.IndexOf(">")+ "-werdSpace:"+werdSpace;
						}// end try
					}
				} else {//wordSpace.index = -1
					try {
						tag = werd.Substring(0,werdStart-1);
					} catch {
						tag = "werdIndex:" +werd.IndexOf(">")+ "-werdSpace:"+werdSpace;
					}// end try
				}// end if werd

				outBox.SelectionColor = Color.Black;
				append = werd.Substring(werdStart, werdEnd - werdStart);

/*
				try {
					if (werdSpace == 0) {
					} else {
					}// end if werdStart

				} catch {
					tag = "werdIndex:" +werd.IndexOf(">")+ "-werdSpace:"+werdSpace;
				}// end try
*/

			tagSwitch(ref append, werd, tag);
				
				if (append != "") {
					outBox.AppendText(append);
				}
				
			}// end foreach string
			//this.Invalidate();
        }// end drawPage

		public void doSomeReplacing(ref string pageSource){
			// Do some replacing
			//First set
			pageSource = pageSource.Replace("&#8217;","'");
			pageSource = pageSource.Replace("&nbsp;"," ");
			pageSource = pageSource.Replace("&lt;","<");
			pageSource = pageSource.Replace("&gt;",">");
			pageSource = pageSource.Replace("&rsaquo;",">");
			pageSource = pageSource.Replace("&amp;","&");
			pageSource = pageSource.Replace("&quot;","\"");
			pageSource = pageSource.Replace("&apos;","'");
			pageSource = pageSource.Replace("&cent;","¢");
			pageSource = pageSource.Replace("&pound;","£");
			pageSource = pageSource.Replace("&yen;","¥");
			pageSource = pageSource.Replace("&euro;","€");
			pageSource = pageSource.Replace("&copy;","©");
			pageSource = pageSource.Replace("&reg;","®");
			//Second set
		pageSource = pageSource.Replace("&#0","&#");
		pageSource = pageSource.Replace("&#32;"," ");
		pageSource = pageSource.Replace("&#33;","!");
		pageSource = pageSource.Replace("&#34;","\"");
		pageSource = pageSource.Replace("&#35;","#");
		pageSource = pageSource.Replace("&#36;","$");
		pageSource = pageSource.Replace("&#37;","%");
		pageSource = pageSource.Replace("&#38;","		");
		pageSource = pageSource.Replace("&#39;","");
		pageSource = pageSource.Replace("&#40;","(");
		pageSource = pageSource.Replace("&#41;",")");
		pageSource = pageSource.Replace("&#42;","*");
		pageSource = pageSource.Replace("&#43;","#ERROR!");
		pageSource = pageSource.Replace("&#44;",",");
		pageSource = pageSource.Replace("&#45;","-");
		pageSource = pageSource.Replace("&#46;",".");
		pageSource = pageSource.Replace("&#47;","/");
		pageSource = pageSource.Replace("&#48;","0");
		pageSource = pageSource.Replace("&#49;","1");
		pageSource = pageSource.Replace("&#50;","2");
		pageSource = pageSource.Replace("&#51;","3");
		pageSource = pageSource.Replace("&#52;","4");
		pageSource = pageSource.Replace("&#53;","5");
		pageSource = pageSource.Replace("&#54;","6");
		pageSource = pageSource.Replace("&#55;","7");
		pageSource = pageSource.Replace("&#56;","8");
		pageSource = pageSource.Replace("&#57;","9");
		pageSource = pageSource.Replace("&#58;",":");
		pageSource = pageSource.Replace("&#59;",";");
		pageSource = pageSource.Replace("&#60;","<");
		pageSource = pageSource.Replace("&#61;","#ERROR!");
		pageSource = pageSource.Replace("&#62;",">");
		pageSource = pageSource.Replace("&#63;","?");
		pageSource = pageSource.Replace("&#64;","@");
		pageSource = pageSource.Replace("&#65;","A");
		pageSource = pageSource.Replace("&#66;","B");
		pageSource = pageSource.Replace("&#67;","C");
		pageSource = pageSource.Replace("&#68;","D");
		pageSource = pageSource.Replace("&#69;","E");
		pageSource = pageSource.Replace("&#70;","F");
		pageSource = pageSource.Replace("&#71;","G");
		pageSource = pageSource.Replace("&#72;","H");
		pageSource = pageSource.Replace("&#73;","I");
		pageSource = pageSource.Replace("&#74;","J");
		pageSource = pageSource.Replace("&#75;","K");
		pageSource = pageSource.Replace("&#76;","L");
		pageSource = pageSource.Replace("&#77;","M");
		pageSource = pageSource.Replace("&#78;","N");
		pageSource = pageSource.Replace("&#79;","O");
		pageSource = pageSource.Replace("&#80;","P");
		pageSource = pageSource.Replace("&#81;","Q");
		pageSource = pageSource.Replace("&#82;","R");
		pageSource = pageSource.Replace("&#83;","S");
		pageSource = pageSource.Replace("&#84;","T");
		pageSource = pageSource.Replace("&#85;","U");
		pageSource = pageSource.Replace("&#86;","V");
		pageSource = pageSource.Replace("&#87;","W");
		pageSource = pageSource.Replace("&#88;","X");
		pageSource = pageSource.Replace("&#89;","Y");
		pageSource = pageSource.Replace("&#90;","Z");
		pageSource = pageSource.Replace("&#91;","[");
		pageSource = pageSource.Replace("&#92;","\\");
		pageSource = pageSource.Replace("&#93;","]");
		pageSource = pageSource.Replace("&#94;","^");
		pageSource = pageSource.Replace("&#95;","_");
		pageSource = pageSource.Replace("&#96;","`");
		pageSource = pageSource.Replace("&#97;","a");
		pageSource = pageSource.Replace("&#98;","b");
		pageSource = pageSource.Replace("&#99;","c");
		pageSource = pageSource.Replace("&#100;","d");
		pageSource = pageSource.Replace("&#101;","e");
		pageSource = pageSource.Replace("&#102;","f");
		pageSource = pageSource.Replace("&#103;","g");
		pageSource = pageSource.Replace("&#104;","h");
		pageSource = pageSource.Replace("&#105;","i");
		pageSource = pageSource.Replace("&#106;","j");
		pageSource = pageSource.Replace("&#107;","k");
		pageSource = pageSource.Replace("&#108;","l");
		pageSource = pageSource.Replace("&#109;","m");
		pageSource = pageSource.Replace("&#110;","n");
		pageSource = pageSource.Replace("&#111;","o");
		pageSource = pageSource.Replace("&#112;","p");
		pageSource = pageSource.Replace("&#113;","q");
		pageSource = pageSource.Replace("&#114;","r");
		pageSource = pageSource.Replace("&#115;","s");
		pageSource = pageSource.Replace("&#116;","t");
		pageSource = pageSource.Replace("&#117;","u");
		pageSource = pageSource.Replace("&#118;","v");
		pageSource = pageSource.Replace("&#119;","w");
		pageSource = pageSource.Replace("&#120;","x");
		pageSource = pageSource.Replace("&#121;","y");
		pageSource = pageSource.Replace("&#122;","z");
		pageSource = pageSource.Replace("&#123;","{");
		pageSource = pageSource.Replace("&#124;","|");
		pageSource = pageSource.Replace("&#125;","}");
		pageSource = pageSource.Replace("&#126;","~");
		pageSource = pageSource.Replace("&#127;","");
		pageSource = pageSource.Replace("&#128;","?");
		pageSource = pageSource.Replace("&#129;","");
		pageSource = pageSource.Replace("&#130;","‚");
		pageSource = pageSource.Replace("&#131;","ƒ");
		pageSource = pageSource.Replace("&#132;","„");
		pageSource = pageSource.Replace("&#133;","…");
		pageSource = pageSource.Replace("&#134;","†");
		pageSource = pageSource.Replace("&#135;","‡");
		pageSource = pageSource.Replace("&#136;","ˆ");
		pageSource = pageSource.Replace("&#137;","‰");
		pageSource = pageSource.Replace("&#138;","Š");
		pageSource = pageSource.Replace("&#139;","‹");
		pageSource = pageSource.Replace("&#140;","Œ");
		pageSource = pageSource.Replace("&#141;","");
		pageSource = pageSource.Replace("&#142;","Ž");
		pageSource = pageSource.Replace("&#143;","");
		pageSource = pageSource.Replace("&#144;","");
		pageSource = pageSource.Replace("&#145;","‘");
		pageSource = pageSource.Replace("&#146;","’");
		pageSource = pageSource.Replace("&#147;","“");
		pageSource = pageSource.Replace("&#148;","”");
		pageSource = pageSource.Replace("&#149;","•");
		pageSource = pageSource.Replace("&#150;","–");
		pageSource = pageSource.Replace("&#151;","—");
		pageSource = pageSource.Replace("&#152;","˜");
		pageSource = pageSource.Replace("&#153;","™");
		pageSource = pageSource.Replace("&#154;","š");
		pageSource = pageSource.Replace("&#155;","›");
		pageSource = pageSource.Replace("&#156;","œ");
		pageSource = pageSource.Replace("&#157;","");
		pageSource = pageSource.Replace("&#158;","ž");
		pageSource = pageSource.Replace("&#159;","Ÿ");
		pageSource = pageSource.Replace("&#160;","");
		pageSource = pageSource.Replace("&#161;","¡");
		pageSource = pageSource.Replace("&#162;","¢");
		pageSource = pageSource.Replace("&#163;","£");
		pageSource = pageSource.Replace("&#164;","¤");
		pageSource = pageSource.Replace("&#165;","¥");
		pageSource = pageSource.Replace("&#166;","¦");
		pageSource = pageSource.Replace("&#167;","§");
		pageSource = pageSource.Replace("&#168;","¨");
		pageSource = pageSource.Replace("&#169;","©");
		pageSource = pageSource.Replace("&#170;","ª");
		pageSource = pageSource.Replace("&#171;","«");
		pageSource = pageSource.Replace("&#172;","¬");
		pageSource = pageSource.Replace("&#173;","­");
		pageSource = pageSource.Replace("&#174;","®");
		pageSource = pageSource.Replace("&#175;","¯");
		pageSource = pageSource.Replace("&#176;","°");
		pageSource = pageSource.Replace("&#177;","±");
		pageSource = pageSource.Replace("&#178;","²");
		pageSource = pageSource.Replace("&#179;","³");
		pageSource = pageSource.Replace("&#180;","´");
		pageSource = pageSource.Replace("&#181;","µ");
		pageSource = pageSource.Replace("&#182;","¶");
		pageSource = pageSource.Replace("&#183;","·");
		pageSource = pageSource.Replace("&#184;","¸");
		pageSource = pageSource.Replace("&#185;","¹");
		pageSource = pageSource.Replace("&#186;","º");
		pageSource = pageSource.Replace("&#187;","»");
		pageSource = pageSource.Replace("&#188;","¼");
		pageSource = pageSource.Replace("&#189;","½");
		pageSource = pageSource.Replace("&#190;","¾");
		pageSource = pageSource.Replace("&#191;","¿");
		pageSource = pageSource.Replace("&#192;","À");
		pageSource = pageSource.Replace("&#193;","Á");
		pageSource = pageSource.Replace("&#194;","Â");
		pageSource = pageSource.Replace("&#195;","Ã");
		pageSource = pageSource.Replace("&#196;","Ä");
		pageSource = pageSource.Replace("&#197;","Å");
		pageSource = pageSource.Replace("&#198;","Æ");
		pageSource = pageSource.Replace("&#199;","Ç");
		pageSource = pageSource.Replace("&#200;","È");
		pageSource = pageSource.Replace("&#201;","É");
		pageSource = pageSource.Replace("&#202;","Ê");
		pageSource = pageSource.Replace("&#203;","Ë");
		pageSource = pageSource.Replace("&#204;","Ì");
		pageSource = pageSource.Replace("&#205;","Í");
		pageSource = pageSource.Replace("&#206;","Î");
		pageSource = pageSource.Replace("&#207;","Ï");
		pageSource = pageSource.Replace("&#208;","Ð");
		pageSource = pageSource.Replace("&#209;","Ñ");
		pageSource = pageSource.Replace("&#210;","Ò");
		pageSource = pageSource.Replace("&#211;","Ó");
		pageSource = pageSource.Replace("&#212;","Ô");
		pageSource = pageSource.Replace("&#213;","Õ");
		pageSource = pageSource.Replace("&#214;","Ö");
		pageSource = pageSource.Replace("&#215;","×");
		pageSource = pageSource.Replace("&#216;","Ø");
		pageSource = pageSource.Replace("&#217;","Ù");
		pageSource = pageSource.Replace("&#218;","Ú");
		pageSource = pageSource.Replace("&#219;","Û");
		pageSource = pageSource.Replace("&#220;","Ü");
		pageSource = pageSource.Replace("&#221;","Ý");
		pageSource = pageSource.Replace("&#222;","Þ");
		pageSource = pageSource.Replace("&#223;","ß");
		pageSource = pageSource.Replace("&#224;","à");
		pageSource = pageSource.Replace("&#225;","á");
		pageSource = pageSource.Replace("&#226;","â");
		pageSource = pageSource.Replace("&#227;","ã");
		pageSource = pageSource.Replace("&#228;","ä");
		pageSource = pageSource.Replace("&#229;","å");
		pageSource = pageSource.Replace("&#230;","æ");
		pageSource = pageSource.Replace("&#231;","ç");
		pageSource = pageSource.Replace("&#232;","è");
		pageSource = pageSource.Replace("&#233;","é");
		pageSource = pageSource.Replace("&#234;","ê");
		pageSource = pageSource.Replace("&#235;","ë");
		pageSource = pageSource.Replace("&#236;","ì");
		pageSource = pageSource.Replace("&#237;","í");
		pageSource = pageSource.Replace("&#238;","î");
		pageSource = pageSource.Replace("&#239;","ï");
		pageSource = pageSource.Replace("&#240;","ð");
		pageSource = pageSource.Replace("&#241;","ñ");
		pageSource = pageSource.Replace("&#242;","ò");
		pageSource = pageSource.Replace("&#243;","ó");
		pageSource = pageSource.Replace("&#244;","ô");
		pageSource = pageSource.Replace("&#245;","õ");
		pageSource = pageSource.Replace("&#246;","ö");
		pageSource = pageSource.Replace("&#247;","÷");
		pageSource = pageSource.Replace("&#248;","ø");
		pageSource = pageSource.Replace("&#249;","ù");
		pageSource = pageSource.Replace("&#250;","ú");
		pageSource = pageSource.Replace("&#251;","û");
		pageSource = pageSource.Replace("&#252;","ü");
		pageSource = pageSource.Replace("&#253;","ý");
		pageSource = pageSource.Replace("&#254;","þ");
		pageSource = pageSource.Replace("&#255;","ÿ");
			//Third set
			pageSource = pageSource.Replace("&#x27;","'");
	}

		public void tagSwitch (ref string append, string werd, string tag) {
			int itemX = 0;
			int itemY = 0;
			int itemWidth = 0;
			int itemHeight = 0;
				switch (tag) {
					case "!--":
						append = "";
						break;
					case "!DOCTYPE":
						append = "";
						break;
					case "a":
						//Parse out the link
						string linkText = "";
						if (werd.IndexOf("href=\"") >=0 ) {
				//linkText = werd.Substring(werd.IndexOf("href"), werd.IndexOf("</body") - werd.IndexOf("<body"));
				//linkText = findIndexOf(werd,"<body","</body",0,0);
							linkText = werd.Substring(werd.IndexOf("href")+6,werd.Length-werd.IndexOf("href")-6);
							linkText = linkText.Substring(0,linkText.IndexOf('"'));
						} else if (werd.IndexOf("href='") >=0 ) {
							linkText = werd.Substring(werd.IndexOf("href")+6,werd.Length-werd.IndexOf("href")-6);
							linkText = linkText.Substring(0,linkText.IndexOf("'"));
						} 
						//Add the hostname if it's implied.
						if (linkText.Substring(0,1) == "/") {
							linkText = history[historyIndex] + linkText.Substring(1,linkText.Length-1);
						};
						//outBox.SelectionColor = Color.Blue;
						//append = append + "<link = \"" + linkText + "\">";

						//Add the link to the richtextbox.
						LinkLabel link = new LinkLabel();
						link.Text = append;
						link.LinkClicked += new LinkLabelLinkClickedEventHandler(this.TextBox_Link);
						LinkLabel.Link data = new LinkLabel.Link();
						data.LinkData = @linkText;
						link.Links.Add(data);
						link.AutoSize = true;
						link.Location = this.outBox.GetPositionFromCharIndex(this.outBox.TextLength);
						this.outBox.Controls.Add(link);
						this.outBox.SelectionStart = this.outBox.TextLength;
						break;
					case "abbr":
						append = "";
						break;
					case "acronym":
						append = "";
						break;
					case "address":
						append = "";
						break;
					case "applet":
						append = "";
						break;
					case "area":
						append = "";
						break;
					case "article":
						append = "";
						break;
					case "aside":
						append = "";
						break;
					case "audio":
						append = "";
						break;
					case "b":
						//Update global bold variable
						append = "";
						break;
					case "/b":
						//Update global bold variable
						break;
					case "base":
						append = "";
						break;
					case "basefont":
						append = "";
						break;
					case "bdi":
						append = "";
						break;
					case "bdo":
						append = "";
						break;
					case "big":
						append = "";
						break;
					case "blockquote":
						append = "";
						break;
					case "body":
						append = "";
						break;
					case "br":
						append = Environment.NewLine;
						break;
					case "button":
						itemX = this.outBox.GetPositionFromCharIndex(this.outBox.TextLength).X;
						itemY = this.outBox.GetPositionFromCharIndex(this.outBox.TextLength).Y;
						itemWidth = goButtonWidth;
						itemHeight = urlBoxHeight;
						drawButton(itemX, itemY, itemWidth, itemHeight, append);
						append = "";
						break;
					case "canvas":
						append = "";
						break;
					case "caption":
						append = "";
						break;
					case "center":
						append = "";
						break;
					case "circle":
						append = "";
						break;
					case "cite":
						append = "";
						break;
					case "code":
						append = "";
						break;
					case "col":
						append = "";
						break;
					case "colgroup":
						append = "";
						break;
					case "data":
						append = "";
						break;
					case "datalist":
						append = "";
						break;
					case "dd":
						append = "";
						break;
					case "del":
						append = "";
						break;
					case "details":
						append = "";
						break;
					case "dfn":
						append = "";
						break;
					case "dialog":
						append = "";
						break;
					case "dir":
						append = "";
						break;
					case "div":
						break;
					case "/div":
						append = "";
						break;
					case "dl":
						append = "";
						break;
					case "dt":
						append = "";
						break;
					case "em":
						append = "";
						break;
					case "embed":
						append = "";
						break;
					case "fieldset":
						append = "";
						break;
					case "figcaption":
						append = "";
						break;
					case "figure":
						append = "";
						break;
					case "font":
						append = "";
						break;
					case "footer":
						append = "";
						break;
					case "form":
						append = "";
						break;
					case "frame":
						append = "";
						break;
					case "frameset":
						append = "";
						break;
					case "h1":
						break;
					case "h2":
						break;
					case "h3":
						break;
					case "h4":
						break;
					case "h5":
						break;
					case "h6":
						break;
					case "head":
						append = "";
						break;
					case "header":
						append = "";
						break;
					case "hr":
						append = "";
						break;
					case "html":
						append = "";
						break;
					case "i":
						break;
					case "iframe":
						append = "";
						break;
					case "img":
						append = "";
						itemX = this.outBox.GetPositionFromCharIndex(this.outBox.TextLength).X;
						itemY = this.outBox.GetPositionFromCharIndex(this.outBox.TextLength).Y;
						itemWidth = goButtonWidth;
						itemHeight = urlBoxHeight;
						drawPanel(itemX, itemY, itemWidth, itemHeight);
						break;
					case "input":
						//input class="lsb" id="tsuid1" value="I'm Feeling Lucky" name="btnI" type="submit">
						itemX = this.outBox.GetPositionFromCharIndex(this.outBox.TextLength).X;
						itemY = this.outBox.GetPositionFromCharIndex(this.outBox.TextLength).Y;
						itemWidth = goButtonWidth;
						itemHeight = urlBoxHeight;
						drawPanel(itemX, itemY, itemWidth, itemHeight);
						break;
					case "ins":
						append = "";
						break;
					case "kbd":
						append = "";
						break;
					case "label":
						append = "";
						break;
					case "legend":
						append = "";
						break;
					case "li":
						//list item
						append = "- " + append;
						break;
					case "link":
						append = "";
						break;
					case "main":
						append = "";
						break;
					case "map":
						append = "";
						break;
					case "mark":
						append = "";
						break;
					case "meta":
						append = "";
						break;
					case "meter":
						append = "";
						break;
					case "nav":
						//Add nav bar?
						//append = "";
						break;
					case "noframes":
						append = "";
						break;
					case "noscript":
						append = "";
						break;
					case "object":
						append = "";
						break;
					case "ol":
						append = "";
						break;
					case "optgroup":
						append = "";
						break;
					case "option":
						append = "";
						break;
					case "output":
						append = "";
						break;
					case "p":
						break;
					case "p1": // Technically p1-p6 are nonstandard
						break;
					case "p2":
						break;
					case "p3":
						break;
					case "p4":
						break;
					case "p5":
						break;
					case "p6":
						break;
					case "param":
						append = "";
						break;
					case "picture":
						append = "";
						break;
					case "pre":
						append = "";
						break;
					case "progress":
						append = "";
						break;
					case "q":
						append = "";
						break;
					case "rp":
						append = "";
						break;
					case "rt":
						append = "";
						break;
					case "ruby":
						append = "";
						break;
					case "s":
						append = "";
						break;
					case "samp":
						append = "";
						break;
					case "script":
						append = "";
						break;
					case "section":
						break;
					case "select":
						append = "";
						break;
					case "small":
						append = "";
						break;
					case "source":
						append = "";
						break;
					case "span":
						break;
					case "strike":
						append = "";
						break;
					case "strong":
						append = "";
						break;
					case "style":
						//Update global style variables
						append = "";
						break;
					case "sub":
						append = "";
						break;
					case "summary":
						append = "";
						break;
					case "sup":
						append = "";
						break;
					case "svg":
						append = "";
						itemX = this.outBox.GetPositionFromCharIndex(this.outBox.TextLength).X;
						itemY = this.outBox.GetPositionFromCharIndex(this.outBox.TextLength).Y;
						itemWidth = goButtonWidth;
						itemHeight = urlBoxHeight;
						drawPanel(itemX, itemY, itemWidth, itemHeight);
						break;
					case "table":
						append = "";
						break;
					case "tbody":
						append = "";
						break;
					case "td":
						append = "";
						break;
					case "template":
						append = "";
						break;
					case "textarea":
						break;
					case "tfoot":
						append = "";
						break;
					case "th":
						append = "";
						break;
					case "thead":
						append = "";
						break;
					case "time":
						append = "";
						break;
					case "title":
						append = "";
						break;
					case "tr":
						append = "";
						break;
					case "track":
						append = "";
						break;
					case "tt":
						append = "";
						break;
					case "u":
						break;
					case "ul":
						//Start a list
						break;
					case "/ul":
						//End a list
						append = Environment.NewLine;
						break;
					case "var":
						append = "";
						break;
					case "video":
						append = "";
						break;
					case "wbr":
						append = "";
						break;
					default:
					
						if (debuggingView) {
						outBox.SelectionColor = Color.Black;
						if (tag.IndexOf("/") <0) {
							append = "Default - Tag: "+tag+" - werd: "+werd;
						}
						}
						break;
			} // end switch
		} // end tagSwitch

		public void urlBox_KeyUp(object sender, KeyEventArgs e) {
    switch (e.KeyCode) {
        case Keys.F5:
			loadNewPage();
			e.Handled = true;
            break;
        case Keys.Enter:
			loadNewPage();
			e.Handled = true;
            break;
    }
}

		protected override void OnPaint( PaintEventArgs e ) {

			Graphics pageGraphics = outBox.CreateGraphics();
			Bitmap myBitmap = new Bitmap(WindowWidth, WindowHeight);
			outBox.DrawToBitmap(myBitmap, new Rectangle(0, 0, myBitmap.Width, myBitmap.Height));


			//pageGraphics.DrawLine(Pens.Black, new Point(0, (outBox.Lines.Length + 1) * 10), new Point(500, (outBox.Lines.Length + 1) * 10));
			if (displayLine == 1) {
				//pageGraphics.Clear(outBox.BackColor);
				DrawRect(WindowWidth/2, WindowHeight/2, urlBoxHeight, goButtonWidth, ref pageGraphics);
			}

			pageGraphics.Dispose();
		}

		public void OnResize(object sender, System.EventArgs e) {
			//outBox.Height = ClientRectangle.Height - urlBoxHeight;
			outBox.Width = ClientRectangle.Width - sideBufferWidth;
			urlBox.Width = ClientRectangle.Width - goButtonWidth;
			goButton.Left = urlBox.Width;
		}
		
		//Draw Stuff
		public void DrawString(string drawString, int x, int y, ref Graphics graphicsObj){
			System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 16);
			System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
			System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
			graphicsObj.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
			drawFont.Dispose();
			drawBrush.Dispose();
		}// end DrawString

		public void DrawRect(int startX, int startY, int sizeX, int sizeY, ref Graphics graphicsObj){
			System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
			graphicsObj.FillRectangle(myBrush, new Rectangle(startX, startY, sizeX, sizeY));
			myBrush.Dispose();
		}
		
		public void drawPanel(int startX, int startY, int sizeX, int sizeY){
			Panel panel1 = new Panel();
			TextBox textBox1 = new TextBox();
			Label label1 = new Label();

			// Initialize the Panel control.
			panel1.Location = new Point(startX,startY);
			panel1.Size = new Size(sizeX, sizeY);
			// Set the Borderstyle for the Panel to three-dimensional.
			//panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			//panel1.TabIndex = 0;
			panel1.AutoScroll = true;
			// Initialize the Label and TextBox controls.
			label1.Location = new Point(12,16);
			label1.Text = "label1";
			label1.Size = new Size(104, 16);
			textBox1.Location = new Point(16,320);
			textBox1.Text = "";
			textBox1.Size = new Size(152, 20);

			// Add the Panel control to the form.
			// Add the Label and TextBox controls to the Panel.
			panel1.Controls.Add(label1);
			panel1.Controls.Add(textBox1);
/*
*/
			this.Controls.Add(panel1);
		}// end drawPanel

		public void drawButton(int startX, int startY, int sizeX, int sizeY,string buttonText){
			goButton = new Button();
			goButton.Text = buttonText;
			goButton.Location = new Point(startX, startY);
			goButton.Size = new Size(sizeX, sizeY);
			outBox.Controls.Add(goButton);

		}// end drawPanel

		public void drawOutBox(){
			int itemX = sideBufferWidth/2;
			int itemY = urlBoxHeight;
			int itemWidth = this.ClientRectangle.Width - sideBufferWidth;
			int itemHeight = this.ClientRectangle.Height - urlBoxHeight;

			outBox.Text = "Blank Page";
			outBox.Name = "outBox";
			outBox.Multiline = true;
			outBox.AcceptsTab = true;
			outBox.WordWrap = true;
			outBox.ReadOnly = true;
			outBox.DetectUrls = true;
			outBox.Font = new Font("Calibri", 14);
			outBox.Location = new Point(itemX, itemY);
			outBox.LinkClicked  += new LinkClickedEventHandler(Link_Click);
			outBox.Height = itemHeight;
			outBox.Width = itemWidth;
			//outBox.Dock = DockStyle.Fill;
			outBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;


			//outBox.BackColor = Color.Red;
			//outBox.ForeColor = Color.Blue;
			//outBox.RichTextBoxScrollBars = ScrollBars.Both;
			//outBox.AcceptsReturn = true;

			Controls.Add(outBox);
		}// end drawOutBox

		public void drawUrlBoxAndGoButton(){
			urlBox.Text = defaultSite;
			urlBox.Name = "urlBox";
			urlBox.Font = new Font("Calibri", 14);
			urlBox.Location = new Point(0, 0);
			urlBox.Height = urlBoxHeight;
			urlBox.Width = WindowWidth - goButtonWidth;
			urlBox.KeyUp += urlBox_KeyUp;
			Controls.Add(urlBox);

			goButton = new Button();
			goButton.Text = "Go";
			goButton.Size = new Size(goButtonWidth, urlBoxHeight);
			goButton.Location = new Point(WindowWidth - goButtonWidth, 0);
			goButton.Click += new EventHandler(goButton_Click);
			Controls.Add(goButton);
			
 	   }// end drawGoButton

		public void drawMenuBar (){
		this.Menu = new MainMenu();
        MenuItem item = new MenuItem("File");
        this.Menu.MenuItems.Add(item);
            item.MenuItems.Add("Save", new EventHandler(Save_Click));
            item.MenuItems.Add("Open", new EventHandler(Open_Click)); 
            item.MenuItems.Add("Page debug", new EventHandler(Debugging_Click)); 
        item = new MenuItem("Edit");
        this.Menu.MenuItems.Add(item);
            item.MenuItems.Add("Copy", new EventHandler(Copy_Click));
            item.MenuItems.Add("Paste", new EventHandler(Paste_Click)); 
        item = new MenuItem("View");
        this.Menu.MenuItems.Add(item);
            item.MenuItems.Add("WordWrap", new EventHandler(WordWrap_Click));
        item = new MenuItem("Navigation");
        this.Menu.MenuItems.Add(item);
            item.MenuItems.Add("Back", new EventHandler(Navigate_Back));
            item.MenuItems.Add("Forward", new EventHandler(Navigate_Forward));
            item.MenuItems.Add("Show History", new EventHandler(Show_History));
            item.MenuItems.Add("Show Scroll Position", new EventHandler(GetScroll_Position));
        item = new MenuItem("Help");
        this.Menu.MenuItems.Add(item);
            item.MenuItems.Add("About", new EventHandler(About_Click));
	   }// end drawMenuBar

		//Utility
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hwndLock, Int32 wMsg, Int32 wParam, ref Point pt);

        public static Point GetScrollPos(RichTextBox txtbox) {
            const int EM_GETSCROLLPOS = 0x0400 + 221;
            Point pt = new Point();

            SendMessage(txtbox.Handle, EM_GETSCROLLPOS, 0, ref pt);
            return pt;
        }

        public static void SetScrollPos(Point pt,RichTextBox txtbox) {
            const int EM_SETSCROLLPOS = 0x0400 + 222;

            SendMessage(txtbox.Handle, EM_SETSCROLLPOS, 0, ref pt);
        }        

        public void goButton_Click(object sender, EventArgs e) {
			loadNewPage();
        }// end goButton_Click

        public void GetScroll_Position(object sender, EventArgs e) {
			MessageBox.Show("x = "+GetScrollPos(outBox).X);
        }// end goButton_Click

		//Menu 
		public void Show_History(object sender, EventArgs e) {
			string outString = "History - You are at: "+historyIndex+ Environment.NewLine;
			//string outString = "History: "+ Environment.NewLine;
/*
			for (int idnex = 0; idnex < 10; idnex++){
				outString += idnex + ": "+ history[idnex] + Environment.NewLine;
			}

*/			
			int idnex = 0;
			foreach (string hist in history){
				outString += idnex + ": "+ hist + Environment.NewLine;
				idnex++;
			}
			MessageBox.Show(outString);
		}// end Save_Click
		
		public void Navigate_Back(object sender, EventArgs e) {
			historyIndex--;
			urlBox.Text = history[historyIndex];
			loadNewPage();
		}// end Save_Click
		
		public void Navigate_Forward(object sender, EventArgs e) {
			Array.Resize(ref history, history.Length + 1);
			historyIndex++;
			urlBox.Text = history[historyIndex];
			loadNewPage();
		}// end Save_Click
		
		public void Debugging_Click(object sender, EventArgs e) {
			if (debuggingView) {
				debuggingView = false;
				loadNewPage();
			} else {
				debuggingView = true;
				loadNewPage();
			}
		}// end Save_Click
		
		public void Link_Click (object sender, System.Windows.Forms.LinkClickedEventArgs e) {
			// Link
			// historyIndex++;
			// urlBox.Text = e.LinkText;
			// history[historyIndex] = urlBox.Text;
			// loadNewPage();
		} // end Link_Click

		public void About_Click (object sender, EventArgs e) {
			string AboutText = "Old Person Browser" + Environment.NewLine;
			AboutText += "(c) 2020 Gilgamech Technologies" + Environment.NewLine;
			AboutText += "" + Environment.NewLine;
			AboutText += "Report bugs (stuff not working):" + Environment.NewLine;
			AboutText += "OldPersonBrowser-Bugs@Gilgamech.com" + Environment.NewLine;
			AboutText += "Request new features on Patreon:" + Environment.NewLine;
			AboutText += "patreon.com/Gilgamech" + Environment.NewLine;
			MessageBox.Show(AboutText);
		} // end Link_Click
/*
Gilgamech is making web browsers, games, self-driving RC cars, and other technology sundries.
*/

		public void WordWrap_Click (object sender, EventArgs e) {
			// Link
			// historyIndex++;
			// urlBox.Text = e.LinkText;
			// history[historyIndex] = urlBox.Text;
			// loadNewPage();
		} // end Link_Click

		public void TextBox_Link (object sender, LinkLabelLinkClickedEventArgs e) {
			Array.Resize(ref history, history.Length + 1);
			historyIndex++;
			history[historyIndex] = e.Link.LinkData.ToString();
			urlBox.Text = history[historyIndex];
			loadNewPage();
		} // end TextBox_Link
		
		public void Save_Click(object sender, EventArgs e) {
			// save
					MessageBox.Show("You're saved");
		}// end Save_Click

		public void Open_Click(object sender, EventArgs e) {
			// save
				displayLine = 1;
				MessageBox.Show("You're opened");
		}// end Open_Click

		public void Copy_Click(object sender, EventArgs e) {
			// save
					MessageBox.Show("You're copied");
		}// end Copy_Click

		public void Paste_Click(object sender, EventArgs e) {
			// save
				MessageBox.Show("You're pasted");
				Graphics pageGraphics = outBox.CreateGraphics();
				Bitmap myBitmap = new Bitmap(WindowWidth, WindowHeight);
				outBox.DrawToBitmap(myBitmap, new Rectangle(0, 0, myBitmap.Width, myBitmap.Height));
				DrawRect(WindowWidth/2, WindowHeight/2, urlBoxHeight, goButtonWidth, ref pageGraphics);
		}// end Paste_Click
/*
public void MyPopupEventHandler(System.Object sender, System.EventArgs e) {
			ContextMenuStrip contextMenu1 = new ContextMenuStrip();
			contextMenu1.BackColor = Color.OrangeRed;
			contextMenu1.ForeColor = Color.Black;
			contextMenu1.Text = "File Menu";
			contextMenu1.Font = new Font("Georgia", 16);
			this.ContextMenuStrip = contextMenu1;
			contextMenu1.Show();

    // Define the MenuItem objects to display for the TextBox.
    MenuItem menuItem1 = new MenuItem("&Copy");
    MenuItem menuItem2 = new MenuItem("&Find and Replace");
    // Define the MenuItem object to display for the PictureBox.
    MenuItem menuItem3 = new MenuItem("C&hange Picture");

    // Clear all previously added MenuItems.
    contextMenu1.MenuItems.Clear();
 
    if(contextMenu1.SourceControl == outBox)
    {
       // Add MenuItems to display for the TextBox.
       contextMenu1.MenuItems.Add(menuItem1);
       contextMenu1.MenuItems.Add(menuItem2);
    }
    else if(contextMenu1.SourceControl == urlBox)
    {
       // Add the MenuItem to display for the PictureBox.
       contextMenu1.MenuItems.Add(menuItem3);
    }

}
*/
		//Image
		public void DownloadImage(string imageUrl) {
		//this.imageUrl = imageUrl;
		try {
		  WebClient client = new WebClient();
		  Stream stream = client.OpenRead(imageUrl);
		  myBitmap = new Bitmap(stream);
		  stream.Flush();
		  stream.Close();
		}
		catch (Exception e) {
		  Console.WriteLine(e.Message);
		}
		}

		private Bitmap GetImage() {
		return myBitmap;
		}

		public void DisplayImage() {
		pagePanel.BackgroundImage = myBitmap;
		}

		public void SaveImage(string filename, ImageFormat format) {
		if (myBitmap != null) {
		  myBitmap.Save(filename, format);
		}
		}

		//XHR
		public void xhrRequest(ref string response_out, string Method) {
			try {
				// SSL stuffs
				//ServicePointManager.Expect100Continue = true;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(history[historyIndex]);
				request.Method = Method;// WebRequestMethods.Http.Get;
				request.ContentType = "application/json;charset=utf-8";
				request.Accept = "application/json";
				request.UserAgent = "OldPersonBrowser";
				WebResponse response = request.GetResponse();
				StreamReader sr = new StreamReader(response.GetResponseStream());

				string response_text = sr.ReadToEnd();
				if (response_text == null) {
					response_out = "problem with getting data";
				} else {
					response_out = response_text;
				}
				sr.Close();
			}
			catch (Exception ex) {
				//MessageBox.Show("Wrong request!" + ex.Message, "Error");
				response_out = ex.Message;
			}
		}// end xhrRequest
/*
		public void picMinimize_Click(object sender, EventArgs e) {
           try
           {
               panelUC.Visible = false;                      //change visible status of your form, etc.
               this.WindowState = FormWindowState.Minimized; //minimize
               minimizedFlag = true;                         //set a global flag
           }
           catch (Exception) {

           }

		}

		public void mainForm_Resize(object sender, EventArgs e) {
            //check if form is minimized, and you know that this method is only called if and only if the form get a change in size, meaning somebody clicked in the taskbar on your application
			if (minimizedFlag == true) {
				panelUC.Visible = true;      //make your panel visible again! thats it
				minimizedFlag = false;       //set flag back
			}
		}
*/
//}
    }// end OldPersonBrowser
}// end OldPersonNamespace


/*Bibliography
https://stackoverflow.com/questions/1236642/windows-form-c-sharp-without-visual-studio
https://stackoverflow.com/questions/43815600/post-or-get-request-with-json-response-in-c-sharp-windows-form-app
https://www.codeproject.com/tips/1118211/create-windows-form-application-without-visual-stu
https://stackoverflow.com/questions/32716174/call-and-consume-web-api-in-winform-using-c-net
https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client?WT.mc_id=DT-MVP-5003235
https://www.codeproject.com/tips/804660/how-to-parse-html-using-csharp
https://web.archive.org/web/20081006160223/http://radio.javaranch.com/balajidl/2006/01/18/1137606354980.html
https://www.c-sharpcorner.com/uploadfile/mahesh/textbox-in-C-Sharp/
https://stackoverflow.com/questions/11165537/passing-textboxs-text-to-another-form-in-c
https://docs.microsoft.com/en-us/dotnet/framework/winforms/advanced/how-to-draw-text-on-a-windows-form
https://www.tutorialspoint.com/Chash-program-to-split-and-join-a-string
https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/foreach-in
https://www.c-sharpcorner.com/UploadFile/9b86d4/how-to-return-multiple-values-from-a-function-in-C-Sharp/
https://stackoverflow.com/questions/614818/in-c-what-is-the-difference-between-public-private-protected-and-having-no
https://stackoverflow.com/questions/1361033/what-does-stathread-do#1361048
https://stackoverflow.com/questions/1504871/options-for-initializing-a-string-array
https://stackoverflow.com/questions/5104175/how-do-you-change-the-text-in-the-titlebar-in-windows-forms
https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.form.size?view=netcore-3.1
https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/listed-alphabetically
https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/target-compiler-option (How to hide the console window - change the compiler target into a winexe)
https://stackoverflow.com/questions/14985570/persistent-graphics-winforms#14985634
https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/overriding-the-onpaint-method?view=netframeworkdesktop-4.8
https://www.codeproject.com/Articles/1355/Professional-C-Graphics-with-pageGraphics
https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.form.autoscroll?redirectedfrom=MSDN&view=netcore-3.1#System_Windows_Forms_Form_AutoScroll
https://stackoverflow.com/questions/2778109/standard-windows-menu-bars-in-windows-forms
https://www.codeproject.com/questions/656352/draw-line-in-rich-textbox
https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.textboxbase.multiline
https://stackoverflow.com/questions/8536958/how-to-add-a-line-to-a-multiline-textbox
http://www.java2s.com/Code/CSharpAPI/System.Windows.Forms/FormOnResize.htm
https://stackoverflow.com/questions/4755204/adding-line-break
https://stackoverflow.com/questions/19011948/how-to-add-scrollbars-in-c-sharp-form
https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.richtextbox?redirectedfrom=MSDN&view=netcore-3.1
https://stackoverflow.com/questions/2527700/change-color-of-text-within-a-winforms-richtextbox
https://docs.microsoft.com/en-us/dotnet/api/system.string.contains?view=netcore-3.1
https://stackoverflow.com/questions/37762806/split-string-at-first-space-and-get-2-sub-strings-in-c-sharp
https://docs.microsoft.com/en-us/dotnet/api/system.string.indexof?view=netcore-3.1
https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel
https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.panel?view=netcore-3.1
https://www.c-sharpcorner.com/uploadfile/mahesh/panel-in-C-Sharp/
https://stackoverflow.com/questions/8624737/c-sharp-scrolling-a-panel-in-windows-forms
https://stackoverflow.com/questions/7748137/is-it-possible-to-select-text-on-a-windows-form-label
https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/switch
https://stackoverflow.com/questions/19975617/press-enter-in-textbox-to-and-execute-button-command
https://stackoverflow.com/questions/18059306/windows-forms-textbox-enter-key
https://stackoverflow.com/questions/1964298/adding-an-f5-hotkey-in-c-sharp
https://www.codeproject.com/articles/24920/c-image-download
https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.control.backgroundimage?view=netcore-3.1
https://stackoverflow.com/questions/12738312/c-sharp-change-the-icon-on-the-top-left-of-winform
https://stackoverflow.com/questions/33659663/how-to-set-user-agent-with-system-net-webrequest-in-c-sharp
https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.contextmenu?view=netframework-4.8
https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/win32icon-compiler-option
https://stackoverflow.com/questions/10125034/how-to-assign-a-custom-icon-for-an-application-which-is-compiled-from-source-fil
https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/win32icon-compiler-option
https://redketchup.io/icon-editor
https://stackoverflow.com/questions/12738312/c-sharp-change-the-icon-on-the-top-left-of-winform
https://stackoverflow.com/questions/21752166/getting-exception-parameter-is-not-valid-when-minimizing-the-form-to-the-taskb
https://stackoverflow.com/questions/435607/how-can-i-make-a-hyperlink-work-in-a-richtextbox
https://stackoverflow.com/questions/9855292/links-inside-rich-textbox
https://stackoverflow.com/questions/4081226/c-sharp-winforms-how-to-disable-the-scrollbar-of-richtextbox
https://stackoverflow.com/questions/19476730/how-to-delete-object
https://stackoverflow.com/questions/13888558/removing-dynamic-controls-from-panel
https://www.4dots-software.com/codeblog/dotnet/how-to-set-or-get-scroll-position-of-richtextbox.php
https://stackoverflow.com/questions/26969446/check-if-pageup-or-pagedown-is-pressed#26969610
https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/for
https://stackoverflow.com/questions/4840802/change-array-size
https://social.msdn.microsoft.com/Forums/vstudio/en-US/30e42195-73ab-4ef4-bc91-62aa89896326/error-the-type-or-namespace-name-list-could-not-be-found-are-you-missing-a-using-directive-or-an
https://www.c-sharpcorner.com/article/c-sharp-list/
https://docs.microsoft.com/en-us/dotnet/api/system.array.resize?view=netcore-3.1
https://www.w3schools.com/TAGs/
https://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references
http://www.nationalfinder.com/html/char-asc.htm
https://stackoverflow.com/questions/33814308/if-statements-and-or
https://stackoverflow.com/questions/4071025/load-an-image-from-a-url-into-a-picturebox
https://stackoverflow.com/questions/7873453/getting-icon-from-resourcestream
*/






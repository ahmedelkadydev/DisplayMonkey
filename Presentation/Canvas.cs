﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Globalization;

namespace DisplayMonkey
{
	public class Canvas
	{
		public Canvas()
		{
		}
		
		public Canvas(int canvasId)
		{
			string sql = string.Format("SELECT TOP 1 * FROM Canvas WHERE CanvasId={0}", canvasId);
			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0)
				{
					DataRow r = ds.Tables[0].Rows[0];
					InitFromRow(r);
				}
			}

            if (this.DisplayId != 0)
                this.Display = new Display(this.DisplayId);
		}

		public static Canvas InitFromDisplay(int displayId)
		{
			Canvas canvas = null;

			string sql = string.Format(
                "SELECT c.* FROM Display d INNER JOIN Canvas c on c.CanvasId=d.CanvasId WHERE DisplayId={0};",
				displayId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (0 == ds.Tables[0].Rows.Count)
					throw new Exception("Canvas not found");

				canvas = new Canvas()
				{
					DisplayId = displayId,
                    Display = new Display(displayId),
				};
				canvas.InitFromRow(ds.Tables[0].Rows[0]);
			}

			canvas.Panels = Panel.List(canvas.CanvasId);

			return canvas;
		}
		
		public void InitFromRow(DataRow r)
		{
			CanvasId = DataAccess.IntOrZero(r["CanvasId"]);
			Height = DataAccess.IntOrZero(r["Height"]);
			Width = DataAccess.IntOrZero(r["Width"]);
			BackgroundColor = DataAccess.StringOrBlank(r["BackgroundColor"]);
			if (BackgroundColor == "")
				BackgroundColor = "transparent";
			BackgroundImage = DataAccess.IntOrZero(r["BackgroundImage"]);
			Name = DataAccess.StringOrBlank(r["Name"]);
			if (Name == "")
				Name = string.Format("Canvas {0}", CanvasId);
		}

		public static List<Canvas> List
		{
			get
			{
				List<Canvas> list = new List<Canvas>();
				string sql = "SELECT * FROM Canvas ORDER BY 1";
				using (DataSet ds = DataAccess.RunSql(sql))
				{
					list.Capacity = ds.Tables[0].Rows.Count;

					// list registered canvases
					foreach (DataRow r in ds.Tables[0].Rows)
					{
						Canvas canvas = new Canvas(DataAccess.IntOrZero(r["CanvasId"]));
						list.Add(canvas);
					}
				}
				return list;
			}
		}
		
		public List<Panel> Panels = new List<Panel>();

		public int BackgroundImage = 0;
		public string BackgroundColor = "";
		public bool IsAppleMobileSupported = true;  // TODO
		public int CanvasId = 0;
		public int Height = 0;
		public int Width = 0;
		public int DisplayId = 0;
        public string Name = "";
        public int FullScreenPanelId = 0;

        public Display Display { get; private set; }

		public int InitialMaxIdleInterval
		{
			get
			{
                return Display.GetIdleInterval(DisplayId);
			}
		}

		public virtual string Head 
		{
			get 
			{
				StringBuilder head = new StringBuilder();

				Location location = new Location(DisplayId);
				
				// add meta
                head.AppendFormat(CultureInfo.InvariantCulture, "<meta name=\"server-latitude\" content=\"{0}\" />\n", ServerGeoData.Latitude);
                head.AppendFormat(CultureInfo.InvariantCulture, "<meta name=\"server-longitude\" content=\"{0}\" />\n", ServerGeoData.Longitude);
                head.AppendFormat(CultureInfo.InvariantCulture, "<meta name=\"server-offset-gmt\" content=\"{0}\" />\n", ServerGeoData.OffsetGMT);
                head.AppendFormat(CultureInfo.InvariantCulture, "<meta name=\"server-external-ip\" content=\"{0}\" />\n", ServerGeoData.ServerExternalIPAddress.ToString());
                if (IsAppleMobileSupported)
				{
					head.Append("<meta name=\"apple-mobile-web-app-capable\" content=\"yes\" />\n");
                    head.Append("<meta name=\"apple-mobile-web-app-status-bar-style\" content=\"black\" />\n");
                }

                // add styles
                head.Append("<link rel=\"stylesheet\" href=\"styles/style.css\" type=\"text/css\" />\n");
                head.Append("<style type=\"text/css\">\n");
                foreach (Panel p in Panels)
                {
                    head.Append(p.Style);
                }
                head.Append("</style>\n");

				// scripts
				foreach (string js in _js_libs)
				{
					head.AppendFormat("<script src=\"{0}\" type=\"text/javascript\" charset=\"utf-8\"></script>\n", js);
				}
				head.Append("<script type=\"text/javascript\" charset=\"utf-8\"><!--\r\n_canvas=new Canvas({\n");
                head.AppendFormat(CultureInfo.InvariantCulture, "displayId:{0},\n", DisplayId);
                head.AppendFormat(CultureInfo.InvariantCulture, "hash:{0},\n", Display.GetHash(DisplayId));
                head.AppendFormat(CultureInfo.InvariantCulture, "temperatureUnit:'{0}',\n", location.TemperatureUnit);
				head.AppendFormat(CultureInfo.InvariantCulture, "dateFormat:'{0}',\n", location.DateFormat);
				head.AppendFormat(CultureInfo.InvariantCulture, "timeFormat:'{0}',\n", location.TimeFormat);
				head.AppendFormat(CultureInfo.InvariantCulture, "latitude:{0},\n", location.Latitude);
				head.AppendFormat(CultureInfo.InvariantCulture, "longitude:{0},\n", location.Longitude);
                head.AppendFormat(CultureInfo.InvariantCulture, "woeid:{0},\n", location.Woeid);
                head.AppendFormat(CultureInfo.InvariantCulture, "culture:'{0}',\n", location.Culture);
                head.AppendFormat(CultureInfo.InvariantCulture, "localTime:'{0}',\n", location.LocalTime);
                head.AppendFormat(CultureInfo.InvariantCulture, "initialIdleInterval:{0},\n", this.InitialMaxIdleInterval);
				head.AppendFormat(CultureInfo.InvariantCulture, "width:{0},\n", this.Width);
                head.AppendFormat(CultureInfo.InvariantCulture, "height:{0},\n", this.Height);
                if (this.BackgroundImage > 0) 
					head.AppendFormat(CultureInfo.InvariantCulture, "backImage:{0},\n", this.BackgroundImage);
				if (this.BackgroundColor != "") 
					head.AppendFormat(CultureInfo.InvariantCulture, "backColor:'{0}',\n", this.BackgroundColor);
                head.AppendFormat(CultureInfo.InvariantCulture, "showErrors:{0},\n", this.Display.ShowErrors ? "true" : "false");
                head.Append("});\n--></script>\n<style></style>\n");
				
				return head.ToString();
			} 
		}

		public string Body
		{
			get
			{
				StringBuilder body = new StringBuilder();
                body.AppendFormat(CultureInfo.InvariantCulture,
                    "<div id=\"segments\" style=\"width:{0}px;height:{1}px;\">\n",
                    Width,
                    Height
                    );
                foreach (Panel panel in Panels)
				{
					if (panel.GetType() == typeof(FullScreenPanel))
						body.Insert(0, panel.Element);
					else
						body.Append(panel.Element);
				}
				body.Append("</div>");

				return body.ToString();
			}
		}

        #region Private Members

		private static string[] _js_libs = new string[] {
			"js/pt/prototype.js", 
			"js/pt/prototype_ccs.js", 
			"js/pt/ajaxpanel.js", 
			"js/pt/scriptaculous.js",
			"js/moment.min.js",
			"js/scroller.js",
			"js/clock.js",
            "js/youtube.js",
            "js/outlook.js",
            "js/picture.js",
            "js/video.js",
            "js/iframe.js",
            "scripts/jquery-2.0.3.min.js",
            //"js/mediaelement.min.js",
            //"js/jquery.tubular.1.0.js",

            // comes last:
			"js/canvas.js"
		};

		#endregion

	}
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml;


namespace DisplayMonkey
{
	public class Clock : Frame
	{
		public Clock(int frameId, int panelId, int displayId)
		{
			PanelId = panelId;
			_templatePath = HttpContext.Current.Server.MapPath("~/files/frames/clock.htm");
			string sql = string.Format(
				"SELECT TOP 1 * FROM CLOCK WHERE FrameId={0};",
				frameId
				);

			using (DataSet ds = DataAccess.RunSql(sql))
			{
				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					DataRow dr = ds.Tables[0].Rows[0];
					FrameId = DataAccess.IntOrZero(dr["FrameId"]);
					ShowDate = (bool)dr["ShowDate"];
					ShowTime = (bool)dr["ShowTime"];
				}
			}
		}

		public override string Html
		{
			get
			{
				string html = "No data has been retrieved. Please specify WOEID and temperature unit in display location.";
				try
				{
					// load template
					string template = File.ReadAllText(_templatePath);

					// fill template
					if (FrameId > 0)
					{
						html = string.Format(template, ShowDate, ShowTime);
					}
				}

				catch (Exception ex)
				{
					html = ex.Message;
				}

				// return html
				return html;
			}
		}

		/*
			<style type="text/css" scoped>
			...
			</style>
		 * */

		public bool ShowDate;
		public bool ShowTime; 
	}
}
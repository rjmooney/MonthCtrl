//
// Copyright (c) 2004 Robert Mooney <rjmooney@impetus.us>
//
// Permission to use, copy, modify, and distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
// ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
// OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Form1
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private MonthCtrl.MonthCtrl monthCtrl1;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			/*
			// Initialize the Month Control
			monthCtrl1.DaysInWeek = 7;
			monthCtrl1.WeeksInCalendar = 5;
			monthCtrl1.StartDay = DayOfWeek.Monday;
			monthCtrl1.HeaderForegroundBrush = new SolidBrush(Color.Black);
			monthCtrl1.HeaderBackgroundBrush = new SolidBrush(Color.LightGray);
			monthCtrl1.HeaderFont = new Font("Tahoma", 10);
			monthCtrl1.Font = new Font("Tahoma", 10);
			monthCtrl1.ForeColor = Color.Black;
			monthCtrl1.DefaultPen = new Pen(new SolidBrush(Color.Black), 1);
			monthCtrl1.SelectedForegroundBrush = new SolidBrush(SystemColors.HighlightText);
			monthCtrl1.SelectedBackgroundBrush = new SolidBrush(SystemColors.Highlight);
			monthCtrl1.DisplayDate = new DateTime(2004, 3, 29, 0, 0, 0, 0);
			monthCtrl1.SelectedDate = new DateTime(2004, 3, 30, 0, 0, 0, 0);
			*/
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.monthCtrl1 = new MonthCtrl.MonthCtrl();
			this.SuspendLayout();
			// 
			// monthCtrl1
			// 
			this.monthCtrl1.Location = new System.Drawing.Point(8, 8);
			this.monthCtrl1.Name = "monthCtrl1";
			this.monthCtrl1.Size = new System.Drawing.Size(952, 632);
			this.monthCtrl1.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(968, 646);
			this.Controls.Add(this.monthCtrl1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}
	}
}

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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace MonthCtrl
{
	/// <summary>
	/// Summary description for MonthCtrl.
	/// </summary>
	public class MonthCtrl : System.Windows.Forms.UserControl
	{
		private System.DateTime m_dtDisplay;				// The display date
		private System.DateTime m_dtSelected;				// The selected date
		private System.DateTime m_dtRealStart;				// The actual start of the calendar (first day in the grid)
		private System.DateTime m_dtRealEnd;				// The actual end of the calendar (last day in the grid)
		private System.DateTime m_dtVirtualStart;			// The first day of the display month
		private System.DateTime m_dtVirtualEnd;				// The last day of the display month
		private System.Drawing.Brush m_brshHeader;			// The brush to use for drawing the header
		private System.Drawing.Brush m_brshHeaderFore;		// The brush to use for drawing strings in the header
		private System.Drawing.Brush m_brshSelected;		// The brush to use for drawing the selected day
		private System.Drawing.Brush m_brshSelectedFore;	// The brush to use for drawing strings in the selected day
		private System.Drawing.Font m_fntHeader;			// The font to use for drawing strings in the header
		private System.Drawing.Pen m_pnDefault;				// The default drawing pen
		private Rectangle m_rctCalendar;					// The bounding rectangle of the calendar
		private string[] m_arrDayNames;						// The string representations of the days of the week (TODO: base on locale)
		private string[] m_arrMonthNames;					// The string representations of the months of the year (TODO: base on locale)
		private int m_nDaysInWeek;							// The number of days in the display week
		private int m_nStartDay;							// The start day of the week (0=Sunday, 1=Monday, 2=Tueday, etc.)
		private int m_nWeeksInGrid;							// The number of weeks in the grid
		private int m_nDaysInGrid;							// The number of days in the grid
		private bool m_bDisplayNonCurrentMonthDates;		// Display dates not in the current month?

		struct day_metadata_t								// Metadata for a day
		{
			public System.DateTime dtDate;					// The date represented
			public Rectangle rctBounding;					// The bounding rectangle of the day
		};
		private day_metadata_t[] m_arrGrid;					// "Days of the month" grid

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MonthCtrl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// Initialize defaults
			m_brshHeader = new SolidBrush(SystemColors.ControlLight);
			m_brshHeaderFore = new SolidBrush(SystemColors.WindowText);
			m_brshSelected = new SolidBrush(SystemColors.Highlight);
			m_brshSelectedFore = new SolidBrush(SystemColors.HighlightText);
			m_fntHeader = new Font("Tahoma", 10);
			m_pnDefault = new Pen(Color.Black, 1);
			m_arrDayNames = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
			m_arrMonthNames = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
			m_nStartDay = 0;
			m_nDaysInWeek = 7;
			m_nWeeksInGrid = 5;
			m_nDaysInGrid = m_nDaysInWeek * m_nWeeksInGrid;
			m_bDisplayNonCurrentMonthDates = true;

			// Start the calendar on today
			SetDisplayDate(DateTime.Now);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// MonthCtrl
			// 
			this.Name = "MonthCtrl";
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.monthCtrl_Paint);

		}
		#endregion

		/// <summary>
		/// Determine if the specified date is in the grid
		/// The control does not need to be painted for this to be accurate
		/// </summary>
		/// <param name="dtDate">The date to check</param>
		/// <returns>True if the date is displayed on the grid, False otherwise</returns>
		private bool IsDateInGrid(DateTime dtDate)
		{
			// If the specified date is within the dates stored as the first and last dates displayed
			if (dtDate >= m_dtRealStart && dtDate <= m_dtRealEnd)
			{
				// The date is in the display range
				return true;
			}

			// Date is not in the display range
			return false;
		}

		/// <summary>
		/// Calculate the bounding rectangle for a day in the calendar
		/// </summary>
		/// <param name="dt">The date whose rectangle will be calculated</param>
		/// <param name="rct">The resulting rectangle</param>
		/// <returns>True if specified date is in the display range, False otherwise</returns>
		private bool GetBoundingRect(DateTime dt, out Rectangle rct)
		{
			DateTime dtDate = dt;	// Store the specified date

			// If the date requested is not in the grid, return FALSE
			if (!IsDateInGrid(dtDate))
			{
				// Instantiate a new (empty) rectangle for return
				rct = new Rectangle(0, 0, 0, 0);

				// Failure
				return false;
			}

			int nCurrentDayNdx;		// The index on the grid of the current day
			int nDayHeight;			// The height of a day
			int nDayWidth;			// The width of a day

			// Store the index of the specified date
			TimeSpan ts = dtDate.Subtract(m_dtRealStart);
			nCurrentDayNdx = (int)ts.TotalDays;

			// Calculate the height of a day in the calendar
			nDayHeight = m_rctCalendar.Height / m_nWeeksInGrid;

			// Calculate the width of a day in the calendar
			nDayWidth = m_rctCalendar.Width / m_nDaysInWeek;

			// Calculate the bounding rectange of the specified date
			int left   = m_rctCalendar.Left + (nCurrentDayNdx % m_nDaysInWeek) * nDayWidth;
			int top    = m_rctCalendar.Top + (nCurrentDayNdx / m_nDaysInWeek) * nDayHeight;
			int right  = m_rctCalendar.Left + ((nCurrentDayNdx % m_nDaysInWeek) + 1) * nDayWidth;
			int bottom = m_rctCalendar.Top + ((nCurrentDayNdx / m_nDaysInWeek) + 1) * nDayHeight;

			// Store the bounding rectangle
			rct = new Rectangle(left, top, right - left, bottom - top);

			// Success!
			return true;
		}

		/// <summary>
		/// Update the grid array.
		/// </summary>
		private void CalculateGrid()
		{
			// (re)Initialize the grid
			m_arrGrid = new day_metadata_t[m_nDaysInGrid];

			// Store the first day of the calendar
			DateTime dtDate = m_dtRealStart;		// The current date
			Rectangle rctDay;						// The current day's bounding rectangle

			// Store the metadata for the range of days displayed
			//
			// Enumerate each day in the grid
			for (int n = 0; n < m_nDaysInGrid; ++n)
			{
				// Store the bounding rectangle for each day in the grid
				GetBoundingRect(dtDate, out rctDay);

				// Add the current grid entry
				m_arrGrid[n].dtDate = dtDate;
				m_arrGrid[n].rctBounding = rctDay;

				// Add a day to the current day
				dtDate = dtDate.AddDays(1);
			}
		}

		/// <summary>
		/// Set the date displayed by the calendar.
		/// </summary>
		/// <param name="dt">The date to display</param>
		private void SetDisplayDate(DateTime dt)
		{
			// Store the display date
			m_dtDisplay = dt;

			int nDayOfWeek;			// The day of the week the current month starts on

			// The day of the week (index) of the first day of the display month
			nDayOfWeek = (int)m_dtDisplay.DayOfWeek;

			// Adjust for the specified start day
			nDayOfWeek -= m_nStartDay;

			// Wrap the start day to the end of the column, if need be
			if (nDayOfWeek < 0)
			{
				nDayOfWeek += m_nDaysInWeek;
			}

			int nMonth;			// An adjusted month
			int nYear;			// An adjusted year
			int nDay;			// The current day
			int nOffsetStart;	// The offset from the display date where the grid should start

			// Store the month and year of the display date
			nMonth = m_dtDisplay.Month;	
			nYear  = m_dtDisplay.Year;
			nDay   = m_dtDisplay.Day;

			// Store the offset from the display date where the grid should start
			nOffsetStart = m_dtDisplay.Day - nDayOfWeek;

			// Default to starting the grid on the offset date.  
			// If the offset is 0, the display date is the first of the month XXX no longer true.
			nDay = nOffsetStart == 0 ? 1 : nOffsetStart;

			// If the offset is less than 1, we need to draw the previous month
			if (nOffsetStart < 1)
			{
				// Calculate the start date using the previous month
				//
				// Store the previous month and current year
				nMonth -= 1;

				// If the previous month is December
				if (nMonth <= 0)
				{
					// Set the month and roll back the year
					nMonth = 12;
					--nYear;
				}

				// Store the starting day of the previous month
				nDay = DateTime.DaysInMonth(nYear, nMonth) + nOffsetStart;

				// Reset the offset, since it is no longer pertinent
				nOffsetStart = 0;
			}

			// Store the start date of the month
			m_dtRealStart = m_dtVirtualStart = new DateTime(nYear, nMonth, nDay, 0, 0, 0, 0);
	
			// If the previous month was drawn
			if (nMonth != m_dtDisplay.Month)
			{
				// We drew the previous month, so the virtual start is the first of the current month
				m_dtVirtualStart = new DateTime(m_dtDisplay.Year, m_dtDisplay.Month, 1, 0, 0, 0, 0);
			}

			int nDaysInDisplayMonth;		// The number of days in the display month
			int nDaysLeftInDisplayMonth;	// The number of days left in the display month

			// The number of days in the display month
			nDaysInDisplayMonth = DateTime.DaysInMonth(m_dtDisplay.Year, m_dtDisplay.Month);

			// Store the number of days left in the current month
			nDaysLeftInDisplayMonth = nDaysInDisplayMonth - nOffsetStart + 1;

			// Reset the month, day and year
			nMonth = m_dtDisplay.Month;
			nYear  = m_dtDisplay.Year;
			nDay = m_dtDisplay.Day;

			int nDaysInNextMonth;			// The number of days in the next month
			int nLastDayOfNextMonth = 0;	// The last day of the next month
			int nDaysRemainingInGrid;		// The number of days available in the grid

			// Initialize the number of days in the grid that we have to work with
			nDaysRemainingInGrid = m_nDaysInGrid;

			// If there are more days in the grid than there are days left in the month,
			// the next month will be displayed.
			if (nDaysRemainingInGrid > nDaysLeftInDisplayMonth)
			{
				// Calculate the end date using the next month
				//
				// The first day of the month is day 1
				nDay = 1;

				// Fill in the grid month by month
				for (;;)
				{
					// If the next month is	January
					if (++nMonth > 12)
					{
						// Reset the index
						nMonth = 1;
						++nYear;
					}

					// Store the number	of days	in the next	month
					nDaysInNextMonth = DateTime.DaysInMonth(nYear, nMonth);

					// Calculate the last day of the next month	(0 = current month uses	all	remaining cells)
					nLastDayOfNextMonth = nDaysRemainingInGrid - nDaysLeftInDisplayMonth;

					// If the last day of next month is less than the number of days in the next month, break
					if (nLastDayOfNextMonth	<= nDaysInNextMonth)
					{
						break;
					}

					// Calculate the number of days remaining in the grid
					nDaysRemainingInGrid -= nDaysInNextMonth;
					nDaysLeftInDisplayMonth = 0;
				}

				// Store the end date of the month
				// If the current month uses all remaining cells, store the last day of the current month 
				m_dtRealEnd = m_dtVirtualEnd = nLastDayOfNextMonth <= 0 ? new DateTime(m_dtDisplay.Year, m_dtDisplay.Month, m_dtDisplay.Year, 23, 59, 59, 999) : new DateTime(nYear, nMonth, nLastDayOfNextMonth, 23, 59, 59, 999);

				// If we're not displaying non-current months, set the start and end dates displayed to 
				// the first and last day of the month, respectively.
				if (!m_bDisplayNonCurrentMonthDates)
				{
					// Set the start and end dates displayed to the first and last day of the month
					m_dtVirtualStart = new DateTime(m_dtDisplay.Year, m_dtDisplay.Month, m_dtDisplay.Day, 0, 0, 0, 0);
					m_dtVirtualEnd = new DateTime(m_dtDisplay.Year, m_dtDisplay.Month, nDaysInDisplayMonth, 23, 59, 59, 999);
				}

				// If we are displaying less than the number of days remaining in the month,
				// set the end date.
			} 
			else
			{
				// The real ending date is the start date, plus the number of days left in the grid
				m_dtRealEnd = m_dtRealStart.AddDays(nDaysRemainingInGrid - 1);
				m_dtRealEnd = new DateTime(m_dtRealEnd.Year, m_dtRealEnd.Month, m_dtRealEnd.Day, 23, 59, 59, 999);

				// The virtual ending date is the virtual start date, plus the number of days remaining in the month
				m_dtVirtualEnd = m_dtVirtualStart.AddDays(nDaysLeftInDisplayMonth - 1);
				m_dtVirtualEnd = new DateTime(m_dtVirtualEnd.Year, m_dtVirtualEnd.Month, m_dtVirtualEnd.Day, 23, 59, 59, 999);
			}

			/*
			// If the window has been created, so has the scroll bar
			if (m_bUseScrollBar && m_bWindowCreated)
			{
				// Set the position of the scroll bar to the current week of the year
				m_scrlVertical.SetScrollPos((int)(m_dtDisplay - m_dtCalendarStart).GetTotalDays() / m_nDaysInWeek - 1);

				// The scroll position has been set
				m_bScrollPositionSet = TRUE;
			}

			// If the window has been created, redraw the control
			if (m_bWindowCreated)
			{
				// Redraw the control
				InvalidateRect(NULL);
			}
			*/
		}

		/// <summary>
		/// Paint the control
		/// </summary>
		private void monthCtrl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			// Clear the window
			e.Graphics.Clear(SystemColors.Control);

			// Store the calendar header's bounding rectangle
			//
			// Start with the calendar's bounding rectangle
			Rectangle rctHeader = this.Bounds;			// The calendar header's bounding rectangle

			// Reduce the header to N% of the height of the calendar's bounding rectangle
			rctHeader.Height = (int)(rctHeader.Height * .04);

			// Store the calendar's bounding rectangle
			m_rctCalendar = new Rectangle(this.Bounds.Left, rctHeader.Bottom, this.Width - m_nDaysInWeek, this.Height - rctHeader.Height);				// The calendar's bounding rectangle

			// Fill in the header
			e.Graphics.FillRectangle(m_brshHeader, rctHeader);
			e.Graphics.DrawLine(new Pen(new SolidBrush(Color.LightGray), 1), rctHeader.Left, rctHeader.Bottom - 1, rctHeader.Right, rctHeader.Bottom - 1);

			// Draw the days of the week
			//
			// Start with the bounding rectangle of the header
			Rectangle rctCurrentDay = rctHeader;			// The bounding rectangle for the current day

			// Calculate the width for one day in the week
			// The resulting rectangle is the first day of the week
			rctCurrentDay.Width = rctHeader.Width / m_nDaysInWeek - 1;

			// Store a mutable version of the start day
			int nStartDay = m_nStartDay;					// Modifiable start day

			// Enumerate the days in the display week
			for (int nCurrentDayNdx = 0; nCurrentDayNdx < m_nDaysInWeek; ++nCurrentDayNdx)
			{
				// Calculate the current index to use for the display name
				int nNdx = nCurrentDayNdx + m_nStartDay;	// The index of the current day's name

				// If the index exceeds the last valid index, reset the index to 0 and set
				// the starting day such that the next day displayed is the first in the
				// day name array.
				if (nNdx >= m_arrDayNames.Length)
				{
					nNdx = 0;
					nStartDay = -nCurrentDayNdx;
				}

				// Prepare to align the text, centered in the day's bounding rectangle
				StringFormat stringFormat = new StringFormat();		// The format of the string to be drawn
				stringFormat.Alignment = stringFormat.LineAlignment = StringAlignment.Center;

				// Draw the current day of the week
				e.Graphics.DrawString(m_arrDayNames[nNdx], m_fntHeader, m_brshHeaderFore, rctCurrentDay, stringFormat);

				// Draw a day separator for every day but the last
				if (nCurrentDayNdx < m_nDaysInWeek - 1)
				{
					// Draw a day separator
					e.Graphics.DrawLine(new Pen(new SolidBrush(Color.LightGray), 1), rctCurrentDay.Right, rctCurrentDay.Top, rctCurrentDay.Right, rctCurrentDay.Bottom);
				}

				// Move to the next day's bounding rectangle
				rctCurrentDay.Offset(rctCurrentDay.Width, 0);
			}
			
			// Calculate the grid
			CalculateGrid();

			// If the date selected is not in the grid, set the selected date to the display date
			if (!IsDateInGrid(m_dtSelected))
			{
				// Set the selected date to the display date
				m_dtSelected = m_dtDisplay;
			}

			// Enumerate the days in the grid
			for (int n = 0; n < m_arrGrid.Length; ++n)
			{
				// Store the bounding rectangle
				Rectangle rctDayBounding = m_arrGrid[n].rctBounding;

				Rectangle rctTemp;				// Temporary bounding rectangle

				// Note: This will paint outside of the rectangle boundary a bit.
				//
				// Account for any addition space generated by column/row boundaries
				// that are not a multiple of the number of day/weeks.
				rctTemp = new Rectangle(rctDayBounding.Left, rctDayBounding.Top, m_rctCalendar.Right / m_nDaysInWeek, m_rctCalendar.Bottom / m_nWeeksInGrid);

				// Color every other month
				//
				// Even months
				if (m_arrGrid[n].dtDate.Month % 2 == 0)
				{
					// Fill the bounding rectangle of the current day index
					e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0xee, 0xee, 0xee)), rctTemp); 

				// Odd months
				} else
				{
					// Fill the bounding rectangle of the current day index
					e.Graphics.FillRectangle(new SolidBrush(Color.White), rctTemp);
				}

				Rectangle rctDateBar;			// Bounding rectangle for the "date bar"

				// Initialize the rectangle for the "date bar" (where the month/date is placed)
				rctDateBar = new Rectangle(rctDayBounding.Left, rctDayBounding.Top, rctDayBounding.Width, 15);
				rctDateBar.Inflate(-2, 0);
				rctDateBar.Y += 3;

				// If the current date is the selected date, select the day
				if (m_arrGrid[n].dtDate.Date == m_dtSelected.Date)
				{
					// Paint the selection cursor
					e.Graphics.FillRectangle(m_brshSelected, rctDateBar);
				}

				string strDay;					// The text representation of the current day

				// If the day header format was not defined, use the default
				//				if (m_strDayHeaderFormat == "")
			{
				// The first day displayed of the month includes the month name
				if (n == 0 || m_arrGrid[n].dtDate.Day == 1)
				{
					// Include the month name
					strDay = string.Format("{0} {1}", m_arrMonthNames[m_arrGrid[n].dtDate.Month - 1], m_arrGrid[n].dtDate.Day);

					// All other days contain only the day
				} 
				else
				{
					// Don't include the month name
					strDay = string.Format("{0}", m_arrGrid[n].dtDate.Day);
				}

				// If the day header format was defined, use it to format the current date
			} 
				/*
					else
					{
						// Format the current date (e.g. "%A, %B %d" = "Friday, November 15")
						strDay = m_arrGrid[n].dtDate.Format(m_strDayHeaderFormat);
					}
					*/

				// Prepare to align the text, centered in the day's bounding rectangle
				StringFormat stringFormat = new StringFormat();		// The format of the string to be drawn
				stringFormat.Alignment = StringAlignment.Far;
				stringFormat.LineAlignment = StringAlignment.Center;
				stringFormat.FormatFlags = StringFormatFlags.NoClip;

				// Draw the current day text
				e.Graphics.DrawString(strDay, this.Font, m_arrGrid[n].dtDate.Date == m_dtSelected.Date ? m_brshSelectedFore : new SolidBrush(this.ForeColor), rctDateBar, stringFormat);
			}

			// Calculate the height of a day in the calendar
			int nDayHeight = m_rctCalendar.Height / m_nWeeksInGrid;

			// Calculate the width of a day in the calendar
			int nDayWidth = m_rctCalendar.Width / m_nDaysInWeek;

			// Draw the calendar grid
			//
			// Calculate the column spacing
			for (int nPos = m_rctCalendar.Left + nDayWidth; nPos <= m_rctCalendar.Right - nDayWidth; nPos += nDayWidth)
			{
				// Draw the current column
				e.Graphics.DrawLine(m_pnDefault, nPos,  m_rctCalendar.Top, nPos, m_rctCalendar.Bottom);
			}

			// Calculate the row spacing
			for (int nPos = m_rctCalendar.Top + nDayHeight; nPos <= m_rctCalendar.Bottom - nDayHeight; nPos += nDayHeight)
			{
				// Draw the current column
				e.Graphics.DrawLine(m_pnDefault, m_rctCalendar.Left, nPos, m_rctCalendar.Right, nPos);
			}

			// Success!
			return;
		}

		/// <summary>
		/// Get or set the on-screen date
		/// </summary>
		public DateTime DisplayDate
		{
			get { return m_dtDisplay; }
			set { SetDisplayDate(value); }
		}

		/// <summary>
		/// Get or set the selected date
		/// </summary>
		public DateTime SelectedDate
		{
			get { return m_dtSelected; }
			set { m_dtSelected = value; }
		}

		/// <summary>
		/// Get or set the header background color
		/// </summary>
        public Brush HeaderBackgroundBrush
		{
			get { return m_brshHeader; }
			set { m_brshHeader = value; }
		}

		/// <summary>
		/// Get or set header foreground color
		/// </summary>
		public Brush HeaderForegroundBrush
		{
			get { return m_brshHeaderFore; }
			set { m_brshHeaderFore = value; }
		}

		/// <summary>
		/// Get or set the selected date background color
		/// </summary>
		public Brush SelectedBackgroundBrush
		{
			get { return m_brshSelected; }
			set { m_brshSelected = value; }
		}

		/// <summary>
		/// Get or set selected date foreground color
		/// </summary>
		public Brush SelectedForegroundBrush
		{
			get { return m_brshSelectedFore; }
			set { m_brshSelectedFore = value; }
		}

		/// <summary>
		/// Get or set the header font
		/// </summary>
		public Font HeaderFont
		{
			get { return m_fntHeader; }
			set { m_fntHeader = value; }
		}

		/// <summary>
		/// Get or set the default pen
		/// </summary>
		public Pen DefaultPen
		{
			get { return m_pnDefault; }
			set { m_pnDefault = value; }
		}

		/// <summary>
		/// Get or set the day the calendar starts on
		/// </summary>
		public DayOfWeek StartDay
		{
			get { return (DayOfWeek)m_nStartDay; }
			set 
			{
				// Store the start day of the week
				m_nStartDay = (int)value;
				
				// Recalculate the display dates
				SetDisplayDate(m_dtDisplay);
			}
		}

		/// <summary>
		/// Get or set the number of days in a week
		/// </summary>
		public int DaysInWeek
		{
			get { return m_nDaysInWeek; }
			set 
			{ 
				m_nDaysInWeek = value; 
				m_nDaysInGrid = m_nDaysInWeek * m_nWeeksInGrid; 
			}
		}

		/// <summary>
		/// Get or set the number of weeks in the calendar
		/// </summary>
		public int WeeksInCalendar
		{
			get { return m_nWeeksInGrid; }
			set 
			{ 
				m_nWeeksInGrid = value; 
				m_nDaysInGrid = m_nDaysInWeek * m_nWeeksInGrid;
			}
		}
	}
}

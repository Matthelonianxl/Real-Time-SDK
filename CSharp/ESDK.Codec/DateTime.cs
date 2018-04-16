﻿/*|-----------------------------------------------------------------------------
 *|            This source code is provided under the Apache 2.0 license      --
 *|  and is provided AS IS with no warranty or guarantee of fit for purpose.  --
 *|                See the project's LICENSE.md for details.                  --
 *|           Copyright Thomson Reuters 2018. All rights reserved.            --
 *|-----------------------------------------------------------------------------
 */

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ThomsonReuters.Eta.Codec
{
    /// <summary>
    /// Represents the date and time (month, day, year, hour, minute, second, 
    /// millisecond, microsecond, and nanosecond) in a bandwidth-optimized fashion. 
    /// This time value is represented as Greenwich Mean Time (GMT) unless noted otherwise
    /// </summary>
    sealed public class DateTime : IEquatable<DateTime>
	{
		internal Date _date;
		internal Time _time;
		private System.DateTime _calendar;
		private MatchCollection matcher;

		// for value(String) method
		private string trimmedVal;

		// Date + time to milli
		private static readonly Regex datetimePattern1 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+):(\\d+):(\\d+):(\\d+)"); // m/d/y h:m:s:milli
		private static readonly Regex datetimePattern2 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d my y h m s milli
		private static readonly Regex datetimePattern3 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d month y h m s milli
		private static readonly Regex datetimePattern4 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); //m/d/y h m s milli
		private static readonly Regex datetimePattern5 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+):(\\d+):(\\d+):(\\d+)"); // d m y h:m:s:milli
		private static readonly Regex datetimePattern6 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+):(\\d+):(\\d+):(\\d+)"); // d month year h:m:s:milli

		// date + time to nano
		private static readonly Regex datetimePattern7 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+):(\\d+):(\\d+):(\\d+):(\\d+):(\\d+)"); // m/d/y h:m:s:milli:micro:nano
		private static readonly Regex datetimePattern8 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d my y h m s milli micro nano
		private static readonly Regex datetimePattern9 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d month y h m s milli micro nano
		private static readonly Regex datetimePattern10 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); //m/d/y h m s milli micro nano
		private static readonly Regex datetimePattern11 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+):(\\d+):(\\d+):(\\d+):(\\d+):(\\d+)"); // d m y h:m:s:milli:micro:nano
		private static readonly Regex datetimePattern12 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+):(\\d+):(\\d+):(\\d+):(\\d+):(\\d+)"); // d month year h:m:s:milli:micro:nano

		private static readonly Regex datetimePattern13 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+):(\\d+):(\\d+):(\\d+):(\\d+)"); // m/d/y h:m:s:milli:micro
		private static readonly Regex datetimePattern14 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d my y h m s milli micro
		private static readonly Regex datetimePattern15 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d month y h m s milli micro
		private static readonly Regex datetimePattern16 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); //m/d/y h m s milli micro
		private static readonly Regex datetimePattern17 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+):(\\d+):(\\d+):(\\d+):(\\d+)"); // d m y h:m:s:milli:micro
		private static readonly Regex datetimePattern18 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+):(\\d+):(\\d+):(\\d+):(\\d+)"); // d month year h:m:s:milli:micro

		private static readonly Regex datetimePattern19 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+):(\\d+):(\\d+)"); // m/d/y h:m:s
		private static readonly Regex datetimePattern20 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d my y h m s
		private static readonly Regex datetimePattern21 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d month y h m s
		private static readonly Regex datetimePattern22 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); //m/d/y h m s
		private static readonly Regex datetimePattern23 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+):(\\d+):(\\d+)"); // d m y h:m:s
		private static readonly Regex datetimePattern24 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+):(\\d+):(\\d+)"); // d month year h:m:s

		private static readonly Regex datetimePattern25 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+):(\\d+)"); // m/d/y h:m
		private static readonly Regex datetimePattern26 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d my y h m
		private static readonly Regex datetimePattern27 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+)\\s(\\d+)"); // d month y h m
		private static readonly Regex datetimePattern28 = new Regex("(\\d+)/(\\d+)/(\\d+)\\s(\\d+)\\s(\\d+)"); //m/d/y h m
		private static readonly Regex datetimePattern29 = new Regex("(\\d+)\\s(\\d+)\\s(\\d+)\\s(\\d+):(\\d+)"); // d m y h:m
		private static readonly Regex datetimePattern30 = new Regex("(\\d+)\\s(\\w+)\\s(\\d+)\\s(\\d+):(\\d+)"); // d month year h:m

        /// <summary>
        /// Creates <seealso cref="DateTime"/>.
        /// </summary>
        /// <returns> <seealso cref="DateTime"/> object
        /// </returns>
        /// <seealso cref="DateTime"/>
        public DateTime()
		{
			_date = new Date();
			_time = new Time();
		}

        /// <summary>
		/// Sets all members in DateTime to 0.
		/// </summary>
		public void Clear()
		{
			_date.Clear();
			_time.Clear();
		}

        /// <summary>
        /// Sets <seealso cref="DateTime"/> to blank. Sets all members in <seealso cref="DateTime"/> to
        /// their respective blank values.
        /// <para>
        /// <ul>
        /// <li>
        /// For date, all values are set to 0.</li>
        /// <li>
        /// For time, hour, minute, and second are set to 255 millisecond is set
        /// to 65535, microsecond and nanosecond are set to 2047.</li>
        /// </ul>
        /// </para>
        /// </summary>
        public void Blank()
		{
			_date.Blank();
			_time.Blank();
		}

        /// <summary>
		/// Returns true if all members in <seealso cref="DateTime"/> are set to the values used to signify
		/// blank. A blank <seealso cref="DateTime"/> contains hour, minute, and second values of 255, 
		/// a millisecond value of 65535, microsecond and nanosecond values of 2047,  and 
		/// day, month and year value of 0.
		/// </summary>
		/// <returns> true if <seealso cref="DateTime"/> is blank, false otherwise. </returns>
		public bool IsBlank
		{
            get
            {
                return (_date.IsBlank & _time.IsBlank);
            }
		}

        /// <summary>
		/// Checks equality of two <seealso cref="DateTime"/> objects.
		/// </summary>
		/// <param name="thatDateTime"> the other date and time to compare to this one
		/// </param>
		/// <returns> true if dates and times are equal, false otherwise </returns>
		public bool Equals(DateTime thatDateTime)
		{
			return (thatDateTime != null && _time.Equals(thatDateTime.Time()) && _date.Equals(thatDateTime.Date()));
		}

        /// <summary>Determines whether the specified <c>Object</c> is equal to the current <c>Object</c>.</summary>
        /// <param name="other">The <c>Object</c> to compare with the this <c>Object</c>.</param>
        /// <returns><c>true</c> if the specified <c>Object</c> is equal to the current <c>Object</c>;
        /// otherwise, <c>false</c>.</returns>
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (other is DateTime)
                return Equals((DateTime)other);

            return false;
        }

        /// <summary>Serves as a hash function for a particular type.</summary>
        /// <returns>A hash code for the current <c>Object</c>.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
		/// Verifies the contents of a populated <seealso cref="DateTime"/> object. Determines
		/// whether day is valid for the specified month (e.g., a day greater than 31
		/// is considered invalid for any month) as determined by the specified year
		/// (to calculate whether it is a leap year). Also validates the range of
		/// hour, minute, second, millisecond, microsecond, and nanosecond members. 
		/// If <seealso cref="DateTime"/> is blank or valid, true is returned; false otherwise.
		/// </summary>
		/// <returns> true if <seealso cref="DateTime"/> is blank or valid, false otherwise. </returns>
		public bool IsValid
		{
			get
			{
				return _time.IsValid && _date.IsValid;
			}
		}

        /// <summary>
        /// Decode date and time.
        /// </summary>
        /// <param name="iter"> DecodeIterator with buffer to decode from and appropriate
        ///            version information set.
        /// </param>
        /// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> if success,
        ///         <seealso cref="CodecReturnCode.INCOMPLETE_DATA"/> if failure,
        ///         <seealso cref="CodecReturnCode.BLANK_DATA"/> if data is blank value
        /// </returns>
        /// <seealso cref="DecodeIterator"/>
        /// <seealso cref="CodecReturnCode"/>
        public CodecReturnCode Decode(DecodeIterator iter)
		{
			return Decoders.DecodeDateTime(iter, this);
		}

        /// <summary>
		/// Used to encode a date and time into a buffer.
		/// </summary>
		/// <param name="iter"> <seealso cref="EncodeIterator"/> with buffer to encode into. Iterator
		///            should also have appropriate version information set.
		/// </param>
		/// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> if success,
		///         <seealso cref="CodecReturnCode.FAILURE"/> if failure,
		///         <seealso cref="CodecReturnCode.BUFFER_TOO_SMALL"/> if the encode buffer is too small
		/// </returns>
		/// <seealso cref="EncodeIterator"/>
		/// <seealso cref="CodecReturnCode"/>
		public CodecReturnCode Encode(EncodeIterator iter)
		{
			return Encoders.PrimitiveEncoder.EncodeDateTime((EncodeIterator)iter, this);
		}

        /// <summary>
		/// Sets date time from a number equal to milliseconds since the January 1, 1970 (midnight UTC/GMT) epoch.
		/// Must be a positive number.
		/// </summary>
		/// <param name="value"> number equal to milliseconds since epoch
		/// </param>
		/// <returns> <see cref="CodecReturnCode.SUCCESS"/> on success,
		///         <see cref="CodecReturnCode.INVALID_ARGUMENT"/> if value is invalid.  </returns>
		public CodecReturnCode Value(string value)
		{
			try
			{
				if (string.ReferenceEquals(value, null))
				{
					return CodecReturnCode.INVALID_ARGUMENT;
				}

				trimmedVal = value.Trim();
				if (trimmedVal.Length == 0)
				{
					// blank
					Blank();
					return CodecReturnCode.SUCCESS;
				}

				if (MatchDTToNano(value) != CodecReturnCode.SUCCESS)
				{
					if (MatchDTToMicro(value) != CodecReturnCode.SUCCESS)
					{
						if (MatchDTToMilli(value) != CodecReturnCode.SUCCESS)
						{
							if (MatchDTToSec(value) != CodecReturnCode.SUCCESS)
							{
								if (MatchDTToSec(value) != CodecReturnCode.SUCCESS)
								{
									if (MatchDTToMin(value) != CodecReturnCode.SUCCESS)
									{
										if (_time.Value(value) != CodecReturnCode.SUCCESS)
										{
											return _date.Value(value);
										}
									}
								}
							}
						}
					}
				}

			}
			catch (Exception)
			{
				return CodecReturnCode.INVALID_ARGUMENT;
			}

			return CodecReturnCode.SUCCESS;
		}

        /// <summary>
		/// Convert DateTime to a String. Returns a String as "%d %b %Y
		/// hour:minute:second:milli:micro:nano" (e.g., 30 NOV 2010 15:24:54:627:529:436).
		/// </summary>
		/// <returns> the string representation of this <seealso cref="DateTime"/> </returns>
		public override string ToString()
		{
			if (IsBlank)
			{
				return "";
			}
			StringBuilder oBuffer = new StringBuilder();

			if (_date.IsBlank)
			{
				oBuffer.Append("");
			}
			else if (!_date.IsValid)
			{
				oBuffer.Append("Invalid dateTime");
			}
			else
			{
				/* normal date */
				/* put this into the same format as marketfeed uses where if any portion is blank, it is represented as spaces */
				if (_date.Day() != 0)
				{
					oBuffer.Append(string.Format("{0:D2} ", _date.Day()));
				}
				else
				{
					oBuffer.Append("");
				}

				if (_date.Month() != 0)
				{
					oBuffer.Append(string.Format("{0} ", ThomsonReuters.Eta.Codec.Date.Months[_date.Month() - 1]));
				}
				else
				{
					oBuffer.Append("");
				}

				if (_date.Year() != 0)
				{
					oBuffer.Append(string.Format("{0,4:D}", _date.Year()));
				}
				else
				{
					oBuffer.Append("");
				}
			}

			// time
			if (!_time.IsBlank)
			{
				if (!_date.IsBlank)
				{
					oBuffer.Append(string.Format("{0}{1}", " ", _time.ToString()));
				}
				else
				{
					oBuffer.Append(string.Format("{0}", _time.ToString()));
				}
			}

			return oBuffer.ToString();

		}

        /// <summary>
		/// <seealso cref="Date"/> portion of the <seealso cref="DateTime"/>.
		/// </summary>
		/// <returns> the date </returns>
		public Date Date()
		{
			return _date;
		}

        /// <summary>
		/// <seealso cref="Time"/> portion of the <seealso cref="DateTime"/>.
		/// </summary>
		/// <returns> the time </returns>
		public Time Time()
		{
			return _time;
		}

        /// <summary>
		/// Set the date time to now in the local time zone.
		/// </summary>
		public void LocalTime()
		{
			_calendar = new System.DateTime();

			SyncFromCalendar();
		}

        /// <summary>
		///  This method will perform a deep copy of this Object to destDateTime.
		/// </summary>
		///  <param name="destDateTime"> the destination DataTime Object.
		/// </param>
		///  <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
		///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if the destDateTime is null.  </returns>
		public CodecReturnCode Copy(DateTime destDateTime)
		{
			if (null == destDateTime)
			{
				return CodecReturnCode.INVALID_ARGUMENT;
			}

			_date.Copy(destDateTime.Date());
			_time.Copy(destDateTime.Time());

			return CodecReturnCode.SUCCESS;
		}

        /// <summary>
        /// Set the date time to now in the GMT zone.
        /// </summary>
        public void GmtTime()
		{
            _calendar = System.DateTime.UtcNow;

			SyncFromCalendar();
		}

        /// <summary>
		/// Day of the month (0 - 31 where 0 indicates blank).
		/// </summary>
		/// <param name="day"> the day to set
		/// </param>
		/// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
		///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if day is invalid.  </returns>
		public CodecReturnCode Day(int day)
		{
			return _date.Day(day);
		}

        /// <summary>
        /// <seealso cref="Date"/> portion of the <seealso cref="DateTime"/>.
        /// </summary>
        /// <returns> the date </returns>
        public int Day()
		{
			return _date.Day();
		}

        /// <summary>
		/// Month of the year (0 - 12 where 0 indicates blank).
		/// </summary>
		/// <param name="month"> the month to set
		/// </param>
		/// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
		///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if month is invalid.  </returns>
		public CodecReturnCode Month(int month)
		{
			return _date.Month(month);
		}

        /// <summary>
		/// Month of the year (0 - 12 where 0 indicates blank).
		/// </summary>
		/// <returns> the month </returns>
		public int Month()
		{
			return _date.Month();
		}

        /// <summary>
        /// Year (0 - 4095 where 0 indicates blank).
        /// </summary>
        /// <param name="year"> the year to set
        /// </param>
        /// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
        ///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if year is invalid.  </returns>
        public CodecReturnCode Year(int year)
		{
			return _date.Year(year);
		}

        /// <summary>
        /// Year (0 - 4095 where 0 indicates blank).
        /// </summary>
        /// <returns> the year </returns>
        public int Year()
		{
			return _date.Year();
		}

        /// <summary>
        /// The hour of the day (0 - 23 where 255 indicates blank).
        /// </summary>
        /// <param name="hour"> the hour to set
        /// </param>
        /// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
        ///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if hour is invalid.  </returns>
        public CodecReturnCode Hour(int hour)
		{
			return _time.Hour(hour);
		}

        /// <summary>
		/// The hour of the day (0 - 23 where 255 indicates blank).
		/// </summary>
		/// <returns> the hour </returns>
		public int Hour()
		{
			return _time.Hour();
		}

        /// <summary>
		/// The minute of the hour (0 - 59 where 255 indicates blank).
		/// </summary>
		/// <param name="minute"> the minute to set
		/// </param>
		/// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
		///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if minute is invalid.  </returns>
		public CodecReturnCode Minute(int minute)
		{
			return _time.Minute(minute);
		}

        /// <summary>
        /// The minute of the hour (0 - 59 where 255 indicates blank).
        /// </summary>
        /// <returns> the minute </returns>
        public int Minute()
		{
			return _time.Minute();
		}

        /// <summary>
		/// The second of the minute (0 - 59 where 255 indicates blank).
		/// </summary>
		/// <param name="second"> the second to set
		/// </param>
		/// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
		///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if second is invalid.  </returns>
		public CodecReturnCode Second(int second)
		{
			return _time.Second(second);
		}

        /// <summary>
		/// The second of the minute (0 - 60 where 255 indicates blank and 60 is to account for leap second).
		/// </summary>
		/// <returns> the second </returns>
		public int Second()
		{
			return _time.Second();
		}

        /// <summary>
		/// The millisecond of the second (0 - 999 where 65535 indicates blank).
		/// </summary>
		/// <param name="millisecond"> the millisecond to set
		/// </param>
		/// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
		///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if millisecond is invalid.  </returns>
		public CodecReturnCode Millisecond(int millisecond)
		{
			return _time.Millisecond(millisecond);
		}

        /// <summary>
		/// The millisecond of the second (0 - 999 where 65535 indicates blank).
		/// </summary>
		/// <returns> the millisecond </returns>
		public int Millisecond()
		{
			return _time.Millisecond();
		}

        /// <summary>
		/// The microsecond of the millisecond (0 - 999 where 2047 indicates blank).
		/// </summary>
		/// <param name="microsecond"> the microsecond to set
		/// </param>
		/// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
		///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if microsecond is invalid.  </returns>
		public CodecReturnCode Microsecond(int microsecond)
		{
			return _time.Microsecond(microsecond);
		}

        /// <summary>
		/// The microsecond of the millisecond (0 - 999 where 2047 indicates blank).
		/// </summary>
		/// <returns> the microsecond </returns>
		public int Microsecond()
		{
			return _time.Microsecond();
		}

        /// <summary>
        /// The nanosecond of the microsecond (0 - 999 where 2047 indicates blank).
        /// </summary>
        /// <param name="nanosecond"> the nanosecond to set
        /// </param>
        /// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
        ///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if nanosecond is invalid.  </returns>
        public CodecReturnCode Nanosecond(int nanosecond)
		{
			return _time.Nanosecond(nanosecond);
		}

        /// <summary>
		/// The nanosecond of the microsecond (0 - 999 where 2047 indicates blank).
		/// </summary>
		/// <returns> the nanosecond </returns>
		public int Nanosecond()
		{
			return _time.Nanosecond();
		}

        /// <summary>
		/// Sets date time from a number equal to milliseconds since the January 1, 1970 (midnight UTC/GMT) epoch.
		/// Must be a positive number.
		/// </summary>
		/// <param name="value"> number equal to milliseconds since epoch
		/// </param>
		/// <returns> <seealso cref="CodecReturnCode.SUCCESS"/> on success,
		///         <seealso cref="CodecReturnCode.INVALID_ARGUMENT"/> if value is invalid.  </returns>
		public CodecReturnCode Value(long value)
		{
			if (value < 0)
			{
				return CodecReturnCode.INVALID_ARGUMENT;
			}

			if (_calendar == null)
			{
				_calendar = new System.DateTime();
			}

			_calendar.AddMilliseconds(value);

			SyncFromCalendar();

			return CodecReturnCode.SUCCESS;
		}

        /// <summary>
		/// Returns this date time value as milliseconds since the January 1, 1970 (midnight UTC/GMT) epoch.
		/// </summary>
		/// <returns> milliseconds since epoch </returns>
		public long MillisSinceEpoch()
		{
			if (_calendar == null)
			{
				_calendar = new System.DateTime();
			}

			SyncToCalendar();

			return _calendar.Ticks;
		}


        // match to minute
        private CodecReturnCode MatchDTToMin(string value)
        {
            CodecReturnCode ret = CodecReturnCode.SUCCESS;
            matcher = datetimePattern25.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[3].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(0);
                ret = _time.Millisecond(0);
                ret = _time.Microsecond(0);
                ret = _time.Nanosecond(0);

                return CodecReturnCode.SUCCESS;
            }

            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern26.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[3].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(0);
                    ret = _time.Millisecond(0);
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern27.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }

                    }

                    ret = _time.Hour(int.Parse(group[3].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(0);
                    ret = _time.Millisecond(0);
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }

            matcher = datetimePattern28.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[3].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(0);
                ret = _time.Millisecond(0);
                ret = _time.Microsecond(0);
                ret = _time.Nanosecond(0);

                return CodecReturnCode.SUCCESS;
            }
            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern29.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[3].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(0);
                    ret = _time.Millisecond(0);
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern30.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }

                    ret = _time.Hour(int.Parse(group[3].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(0);
                    ret = _time.Millisecond(0);
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
                else
                {
                    return CodecReturnCode.INVALID_ARGUMENT;
                }
            }
            return CodecReturnCode.INVALID_ARGUMENT;
        }

        // match to second
        private CodecReturnCode MatchDTToSec(string value)
        {
            CodecReturnCode ret = CodecReturnCode.SUCCESS;
            matcher = datetimePattern19.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[3].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(int.Parse(group[5].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Millisecond(0);
                ret = _time.Microsecond(0);
                ret = _time.Nanosecond(0);

                return CodecReturnCode.SUCCESS;
            }

            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern20.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[3].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(0);
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern21.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }

                    }

                    ret = _time.Hour(int.Parse(group[3].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(0);
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }

            matcher = datetimePattern22.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[3].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(int.Parse(group[5].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Millisecond(0);
                ret = _time.Microsecond(0);
                ret = _time.Nanosecond(0);

                return CodecReturnCode.SUCCESS;
            }
            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern23.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[3].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(0);
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern24.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }

                    ret = _time.Hour(int.Parse(group[3].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(0);
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
                else
                {
                    return CodecReturnCode.INVALID_ARGUMENT;
                }
            }
            return CodecReturnCode.INVALID_ARGUMENT;
        }

        // match to millisecond
        private CodecReturnCode MatchDTToMilli(string value)
        {
            CodecReturnCode ret = CodecReturnCode.SUCCESS;
            matcher = datetimePattern1.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[5].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(int.Parse(group[6].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Millisecond(int.Parse(group[7].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Microsecond(0);
                ret = _time.Nanosecond(0);

                return CodecReturnCode.SUCCESS;
            }

            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern2.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern3.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }

                    }

                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }
            matcher = datetimePattern4.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[5].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(int.Parse(group[6].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Millisecond(int.Parse(group[7].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Microsecond(0);
                ret = _time.Nanosecond(0);

                return CodecReturnCode.SUCCESS;
            }
            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern5.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern6.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }

                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(0);
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
                else
                {
                    return CodecReturnCode.INVALID_ARGUMENT;
                }
            }
            return CodecReturnCode.INVALID_ARGUMENT;
        }

        // matches to microsecond time
        private CodecReturnCode MatchDTToMicro(string value)
        {
            CodecReturnCode ret = CodecReturnCode.SUCCESS;
            matcher = datetimePattern13.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[5].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(int.Parse(group[6].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Millisecond(int.Parse(group[7].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Microsecond(int.Parse(group[8].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Nanosecond(0);

                return CodecReturnCode.SUCCESS;
            }

            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern14.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(int.Parse(group[8].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern15.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }

                    }

                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(int.Parse(group[8].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }

            matcher = datetimePattern16.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[5].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(int.Parse(group[6].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Millisecond(int.Parse(group[7].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Microsecond(int.Parse(group[8].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Nanosecond(0);

                return CodecReturnCode.SUCCESS;
            }
            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern17.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(int.Parse(group[8].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern18.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }

                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(int.Parse(group[8].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Nanosecond(0);

                    return CodecReturnCode.SUCCESS;
                }
                else
                {
                    return CodecReturnCode.INVALID_ARGUMENT;
                }
            }
            return CodecReturnCode.INVALID_ARGUMENT;
        }

        // matches to nanosecond time
        private CodecReturnCode MatchDTToNano(string value)
        {
            CodecReturnCode ret = CodecReturnCode.SUCCESS;

            matcher = datetimePattern7.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[5].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(int.Parse(group[6].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Millisecond(int.Parse(group[7].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Microsecond(int.Parse(group[8].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Nanosecond(int.Parse(group[9].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }

                return CodecReturnCode.SUCCESS;
            }

            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern8.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(int.Parse(group[8].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Nanosecond(int.Parse(group[9].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern9.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }

                    }

                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(int.Parse(group[8].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Nanosecond(int.Parse(group[9].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }

                    return CodecReturnCode.SUCCESS;
                }
            }

            matcher = datetimePattern10.Matches(trimmedVal);

            if (matcher.Count > 0)
            {
                Match match = matcher[0];
                GroupCollection group = match.Groups;
                int a = int.Parse(group[1].ToString());
                int b = int.Parse(group[2].ToString());
                int c = int.Parse(group[3].ToString());

                if (a > 255) // assume year here is greater than MAX UINT8
                {
                    // 1974/04/14
                    ret = _date.Day(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else if (c < 100) // assume year here is less than 100, then add 1900
                {
                    // 04/14/74
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c + 1900);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                else
                {
                    // 04/14/1974
                    ret = _date.Day(b);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Month(a);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _date.Year(c);
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                }
                ret = _time.Hour(int.Parse(group[4].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Minute(int.Parse(group[5].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Second(int.Parse(group[6].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Millisecond(int.Parse(group[7].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Microsecond(int.Parse(group[8].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }
                ret = _time.Nanosecond(int.Parse(group[9].ToString()));
                if (ret != CodecReturnCode.SUCCESS)
                {
                    return ret;
                }

                return CodecReturnCode.SUCCESS;
            }
            if (char.IsDigit(trimmedVal[3]))
            {
                matcher = datetimePattern11.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    int b = int.Parse(group[2].ToString());
                    int c = int.Parse(group[3].ToString());

                    if (a > 255) // assume year here is greater than MAX UINT8
                    {
                        // 1974/04/14
                        ret = _date.Day(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        // 04/14/1974
                        ret = _date.Day(b);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(int.Parse(group[8].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Nanosecond(int.Parse(group[9].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }

                    return CodecReturnCode.SUCCESS;
                }
            }
            else if (char.IsUpper(trimmedVal[3]) || char.IsLower(trimmedVal[3]))
            {
                matcher = datetimePattern12.Matches(trimmedVal);

                if (matcher.Count > 0)
                {
                    Match match = matcher[0];
                    GroupCollection group = match.Groups;
                    int a = int.Parse(group[1].ToString());
                    string strMon = group[2].ToString();
                    int c = int.Parse(group[3].ToString());
                    if (c < 100) // assume year here is less than 100, then add 1900
                    {
                        // 04/14/74
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c + 1900);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        ret = _date.Day(a);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Month(TranslateMonth(strMon));
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                        ret = _date.Year(c);
                        if (ret != CodecReturnCode.SUCCESS)
                        {
                            return ret;
                        }
                    }

                    ret = _time.Hour(int.Parse(group[4].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Minute(int.Parse(group[5].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Second(int.Parse(group[6].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Millisecond(int.Parse(group[7].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Microsecond(int.Parse(group[8].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }
                    ret = _time.Nanosecond(int.Parse(group[9].ToString()));
                    if (ret != CodecReturnCode.SUCCESS)
                    {
                        return ret;
                    }

                    return CodecReturnCode.SUCCESS;
                }
                else
                {
                    return CodecReturnCode.INVALID_ARGUMENT;
                }
            }
            return CodecReturnCode.INVALID_ARGUMENT;
        }

        private int TranslateMonth(string monthStr)
        {
            int i, month = 0;

            for (i = 0; i < 12; i++)
            {
                if (ThomsonReuters.Eta.Codec.Date.Months[i].Equals(monthStr, StringComparison.CurrentCultureIgnoreCase))
                {
                    month = i + 1;
                    break;
                }
            }

            return month;
        }


        // syncs up this date time from the calendar object
        private void SyncFromCalendar()
		{
			_date.Day(_calendar.Day);
			_date.Month(_calendar.Month + 1);
			_date.Year(_calendar.Year);
			_time.Hour(_calendar.Hour);
			_time.Minute(_calendar.Minute);
			_time.Second(_calendar.Second);
			_time.Millisecond(_calendar.Millisecond);
			_time.Microsecond(0);
			_time.Nanosecond(0);
		}

		// syncs up this date time into the calendar object
		private void SyncToCalendar()
		{
            _calendar.AddDays(_date.Day());
            _calendar.AddMonths(_date.Month() - 1);
            _calendar.AddYears(_date.Year());
            _calendar.AddHours(_time.Hour());
            _calendar.AddMinutes(_time.Minute());
            _calendar.AddSeconds(_time.Second());
            _calendar.AddMilliseconds(_time.Millisecond());
		}

	}
}
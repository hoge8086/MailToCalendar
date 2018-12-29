using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentParser
{
    public class Appointment
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public bool AllDay { get; private set; }

        public Appointment(DateTime start, DateTime end, bool allDay)
        {
            Start = start;
            End = end;
            AllDay = allDay;
        }

        public class Builder
        {
            private Date startDate = null;
            private Time startTime = null;
            private Date endDate = null;
            private Time endTime = null;

            private Time defaultStartTime = new Time(8, 30);
            private Time defaultEndTime =  new Time(17, 0);

            public void append(Date date)
            {
                if(startDate == null && startTime == null)
                {
                    startDate = date;
                }
                else if((startDate != null || startTime != null) && endDate == null)
                {
                    endDate = date;
                }
            }
            public void append(Time time)
            {
                if(startTime == null && endDate == null)
                {
                    startTime = time;
                }
                else if((startTime != null || endDate != null) && endTime == null)
                {
                    endTime = time;
                }
            }

            public Appointment build()
            {
                bool allDay = false;
                Date startDate = this.startDate;
                Time startTime = this.startTime;
                Date endDate = this.endDate;
                Time endTime = this.endTime;

                //時刻の指定がない場合は終日
                if (startTime == null && endTime == null)
                    allDay = true;

                //開始日がnull
                if (startDate == null)
                {
                    startDate = Date.Today;
                }
                else
                {
                    //開始日内の不定値を補正する
                    startDate = startDate.ResolveUnknownDateAfter(Date.Today);
                }

                //開始時刻がnull
                if (startTime == null)
                    startTime = defaultStartTime;
                
                //終了日がnull
                if (endDate == null)
                {
                    endDate = startDate;
                }
                else
                {
                    //終了日内の不定値を補正する
                    endDate = endDate.ResolveUnknownDateAfter(startDate);
                }
                //終了時刻がnull
                if (endTime == null)
                {
                    if(startDate.Equals(endDate) && (startTime.CompareTo(defaultEndTime) > 0))
                        endTime = startTime;
                    else
                        endTime = defaultEndTime;
                }
                else if(startDate.Equals(endDate) && (this.startTime == null && defaultStartTime.CompareTo(endTime) > 0))
                {
                    endTime = startTime;
                }

                DateTime start = CreateDataTime(startDate, startTime);
                DateTime end = CreateDataTime(endDate, endTime);

                if(start > end)
                {
                    throw new ArgumentException("終了日が開始日がより前に設定されています.");
                }
                return new Appointment(start, end, allDay);
            }

            //Dateオブジェクト、TimeオブジェクトからDateTimeオブジェクトを生成する
            static private DateTime CreateDataTime(Date date, Time time)
            {
                try
                {
                    return new DateTime(
                        date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
                }catch
                {
                    throw new ArgumentException("不正な日時が使用されています.");
                }
            }
        }
    }
}

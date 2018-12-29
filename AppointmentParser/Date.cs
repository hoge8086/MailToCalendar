using System;

namespace AppointmentParser
{

    //日付クラス
    public class Date : System.IComparable
    {
        //DataTimeのラッパーとして内部を実装
        //private DateTime m_date;
        public int Year { get; private set; }
        public int Month { get; private set; }
        public int Day { get; private set; }

        //年月日のgetter
        //public int Year { get { return m_year; } }
        //public int Month { get { return m_month; } }
        //public int Day { get { return m_day; } }

        //今日の日付を取得するデリケード型
        public delegate DateTime GetNowDelegate();
        //今日の日付を取得するデリケード
        static private GetNowDelegate m_GetNow = () => { return DateTime.Now; };
        //今日の日付を取得するメソッドを入れ替える関数
        static public void SetNowGetter(GetNowDelegate nowGetter) { m_GetNow = nowGetter; }

        //今日の日付を返す
        static public Date Today { get { return new Date(m_GetNow().Year, m_GetNow().Month, m_GetNow().Day); } }

        //コンストラクタ
        public Date(int year, int month, int day)
        {
            //m_date = new DateTime(year, month, day);
            Year = year;
            Month = month;
            Day = day;
        }

        //日付を文字列へ変換
        public override string ToString()
        {
            //return m_date.ToString("d");
            return string.Format("{0}/{1}/{2}", Year, Month, Day);
        }

        //Equals()メソッドのオーバーロード
        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            Date date = (Date)obj;
            //return m_date.Equals(date.m_date);
            return (Year == date.Year) && (Month == date.Month) && (Day == date.Day);
        }

        //ハッシュ値を取得
        public override int GetHashCode()
        {
            //return m_date.GetHashCode();
            return Year + Month + Day;
        }

        //比較演算子オーバーロード
        public int CompareTo(object obj)
        {
            //nullより大きい
            if (obj == null)
            {
                return 1;
            }

            //違う型とは比較できない
            if (this.GetType() != obj.GetType())
            {
                throw new ArgumentException("別の型とは比較できません。", "obj");
            }

            //return m_date.CompareTo(((Date)obj).m_date);
            Date date = (Date)obj;
            double x = Year * 10000 + Month * 100 + Day;
            double y = date.Year * 10000 + date.Month * 100 + date.Day;
            return (int)(x - y);
        }

        //日付の不明な部分を、指定日以降の日付に補正する
        public Date ResolveUnknownDateAfter(Date afterDate)
        {
            int day = this.Day;
            int month = this.Month;
            int year = this.Year;

            if (Day == 0)
            {
                //日が省略される場合は、必ず月も年も省略される
                System.Diagnostics.Debug.Assert(Month== 0);
                System.Diagnostics.Debug.Assert(Year == 0);

                day = afterDate.Day;
            }

            if (Month == 0)
            {
                //月が省略される場合は、必ず年も省略される
                System.Diagnostics.Debug.Assert(Year == 0);

                month = afterDate.Month;
                if(Day < afterDate.Day)
                {
                    month++;
                    if (month > 12)
                        month = 1;
                }
            }

            if (Year == 0)
            {
                year = afterDate.Year;

                if (new Date(year, month, day).CompareTo(afterDate) < 0)
                    year++;
            }

            return new Date(year, month, day);
        }

        //文字列からDateオブジェクトを取得する関数
        //・年月日が省略された場合、UNKNOWN(0)で埋める
        static public Date Parse(string textOfDate)
        {
            //テキスト内に含まれる数値を取得
            int[] numbers = Utils.ParseNumbers(textOfDate);

            int[] date = new int[3] { 0, 0, 0 };
            for (int i=0, j=numbers.Length-1; i<3 && j>=0; i++, j--)
                date[2-i] = numbers[j];

            Date d = new Date(date[0], date[1], date[2]);

            if (((d.Year != 0) && (d.Year < 1900)) ||
                (d.Month < 0)  ||
                (d.Month > 12) ||
                (d.Day < 0)    ||
                (d.Day > 31))
            {
                return null;
            }

            return d;
        }

    }
}

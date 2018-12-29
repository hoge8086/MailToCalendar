using System;

namespace AppointmentParser
{
    //時刻クラス
    public class Time : System.IComparable
    {
        //DataTimeのラッパーとして内部を実装
        //private DateTime m_time;

        public int Hour { get; private set; }
        public int Minute { get; private set; }

        //現在時刻を取得するデリケード型
        public delegate DateTime GetNowDelegate();
        //現在時刻を取得するデリケード
        static private GetNowDelegate m_GetNow = () => { return DateTime.Now; };
        //現在時刻を取得するメソッドを入れ替える関数
        static public void SetNowGetter(GetNowDelegate nowGetter) { m_GetNow = nowGetter; }
        //現在時刻を返す
        static public Time Now { get { return new Time(m_GetNow().Hour, m_GetNow().Minute); } }

        //コンストラクタ
        public Time(int hour, int minute)
        {
            //m_time = new DateTime(2000, 1, 1, hour, minute, 0);
            this.Hour = hour;
            this.Minute = minute;
        }

        //文字列へ変換する
        public override string ToString()
        {
            //return m_time.ToString("t");
            return string.Format("{0}:{1}", Hour, Minute);
        }

        //Equals()メソッドのオーバーライド
        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            Time time = (Time)obj;
            //return m_time.Equals(date.m_time);
            return (Hour == time.Hour) && (Minute == time.Minute);
        }

        //ハッシュ値を取得する
        public override int GetHashCode()
        {
            return Hour * 100 + Minute;//m_time.GetHashCode();
        }

        //比較演算子のオーバーロード
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

            Time time = (Time)obj;
            int x = Hour * 100 + Minute;
            int y = time.Hour * 100 + time.Minute;
            return x - y;

        }

        //文字列からTimeオブジェクトを生成する
        static public Time Parse(string textOfTime)
        {
            //テキスト内に含まれる数値を取得する
            int[] numbers = Utils.ParseNumbers(textOfTime);

            int hour = 0;
            int minute = 0;
            if (numbers.Length > 0) hour = numbers[0];
            if (numbers.Length > 1) minute = numbers[1];


            if (hour < 0 || hour >= 24 || minute < 0 || minute > 59)
                return null;

            return new Time(hour, minute);            
        }

    }
}

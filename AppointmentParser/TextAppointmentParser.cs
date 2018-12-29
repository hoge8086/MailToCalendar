using System;
using System.Collections.Generic;

using System.Text.RegularExpressions;

namespace AppointmentParser
{
    //テキスト予定解析クラス
    public class TextAppointmentParser
    {
        //インデックスにより比較可能なオブジェクトクラス
        private class IndexComparable : IComparable
        {
            public int m_index;
            public int CompareTo(object obj)
            {
                //nullより大きい
                if (obj == null)
                {
                    return 1;
                }

                IndexComparable m = obj as IndexComparable;
                //違う型とは比較できない
                if (m == null)
                {
                    throw new ArgumentException("別の型とは比較できません。", "obj");
                }
                return m_index.CompareTo(m.m_index);
            }
        }

        //正規表現でマッチしたインデックスを持つ日付クラス
        private class DateWithMatchedIndex : IndexComparable
        {
            public Date m_date;
            public DateWithMatchedIndex(int index, Date date) { m_index = index; m_date = date; }
            public override string ToString() { return "(" + m_index.ToString() + ") " + m_date.ToString(); }
            static public IndexComparable Parse(Match match)
            {
                Date date = Date.Parse(match.Value);
                if (date == null)
                    return null;
                return new DateWithMatchedIndex(match.Index, date);
            }
        }

        //正規表現でマッチしたインデックスを持つ時刻クラス
        private class TimeWithMatchedIndex : IndexComparable
        {
            public Time m_time;
            public TimeWithMatchedIndex(int index, Time time) { m_index = index; m_time = time; }
            public override string ToString() { return "(" + m_index.ToString() + ") " + m_time.ToString(); }
            static public IndexComparable Parse(Match match)
            {
                Time time = Time.Parse(match.Value);
                if (time == null)
                    return null;
                return new TimeWithMatchedIndex(match.Index, time);
            }
        }

        //日付パーサー
        private ParserUsingMatchObjConverter<IndexComparable> m_parserOfTime
            = new ParserUsingMatchObjConverter<IndexComparable>(TimeWithMatchedIndex.Parse);

        //時刻パーサー
        private ParserUsingMatchObjConverter<IndexComparable> m_parserOfDate
            = new ParserUsingMatchObjConverter<IndexComparable>(DateWithMatchedIndex.Parse);

        //コンストラクタ
        public TextAppointmentParser()// string[] dateRegexp, string[] timeRegexp)
        {
            string[] timeRegexp =
            {
                @"\d{1,2}[ 　]*時(?!間)[ 　]*(\d{1,2}[ 　]*分)?",    // ex: "9時", "0時23分", "23時5分"
                @"\d{1,2}[ 　]*[:：][ 　]*\d{1,2}"                 // ex: "9:00", "0:23", "23:5"
            };

            string[] dateRegexp =
            {
                @"(\d{4}[ 　]*/)?[ 　]*\d{1,2}[ 　]*/[ 　]*\d{1,2}",                 // ex: "2017/05/15", "5/15"
                @"(\d{4}[ 　]*／)?[ 　]*\d{1,2}[ 　]*／[ 　]*\d{1,2}",                 // ex: "2017／05／15", "5／15"
                @"(\d{4}[ 　]*年)?[ 　]*(\d{1,2}[ 　]*月)?[ 　]*\d{1,2}[ 　]*日(?!間)"     // ex: "2017年05月15日", "5月15日", "15日"
            };

            //日付パーサーへ正規表現の登録
            foreach (string regexp in dateRegexp)
                m_parserOfDate.AddRegexp(regexp);

            //時刻パーサーへ正規表現の登録
            foreach (string regexp in timeRegexp)
                m_parserOfTime.AddRegexp(regexp);
        }

        //全角数字を半角数字に
        static public string ConvertTextZenToHanNum(string text)
        {
            return Regex.Replace(text, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString());
        }

        //スペース(全角半角)を取り除く
        static public string RemoveSpace(string text)
        {
            return Regex.Replace(text, @"(?<!\d)[ 　]+(?!\d)", "");
        }

        //テキストから予定を解析するメソッド
        public Appointment Parse(string text)
        {
            //全角数字を半角数字に
            string PreprocessedText = ConvertTextZenToHanNum(text);
            //スペース(全角半角)を取り除く(数字と数字の間のスペースは区切りがあるので削除しない)
            PreprocessedText = RemoveSpace(PreprocessedText);

            List<IndexComparable> listDateTime = new List<IndexComparable>();
            //テキストから日付を抽出
            listDateTime.AddRange(m_parserOfDate.Parse(PreprocessedText));
            //テキストから時刻を抽出
            listDateTime.AddRange(m_parserOfTime.Parse(PreprocessedText));

            //日付・時刻をテキスト内の出現順にソート
            listDateTime.Sort();

            var builder = new Appointment.Builder();

            for(int i=0; i<listDateTime.Count; i++)
            {
                IndexComparable m = listDateTime[i];
                if(m is DateWithMatchedIndex)       //日付の場合
                {
                    builder.append(((DateWithMatchedIndex)m).m_date);
                }
                else if(m is TimeWithMatchedIndex)  //時刻の場合
                {
                    builder.append(((TimeWithMatchedIndex)m).m_time);
                }
            }

            return builder.build();
        }

    }
}

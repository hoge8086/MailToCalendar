using System.Collections.Generic;

using System.Text.RegularExpressions;

namespace AppointmentParser
{
    //----------------------------------------------------------------------------------------
    //  パーサークラスサブセット
    //
    //  概要: テキスト内の文字列でターゲットのクラスに変換可能な文字列を
    //        ターゲットクラスへ変換し、そのListを返す。
    //
    //  実装:
    //    テキスト内から正規表現にマッチした文字列(あるいは、Matchオブジェクト)を
    //    コンストラクタで指定したConverterにかけることでターゲットのオブジェトを生成し、
    //    そのListを取得するクラス
    //----------------------------------------------------------------------------------------

    //パーサークラス(ターゲットクラスが文字列なのでコンバーターはなし)
    public class BaseParser
    {
        //正規表現のリスト
        private List<string> m_Regexp = new List<string>();

        //コンストラクタ
        public BaseParser() { }

        //正規表現を追加する
        public void AddRegexp(string regexp)
        {
            m_Regexp.Add(regexp);
        }

        //リスト内の正規表現すべてに一致する正規表現を生成する
        protected string GetRegexp()
        {
            return "(" + string.Join(")|(", m_Regexp) + ")";
        }

        //テキスト内から正規表現に一致する文字列を取得する
        public List<string> Parse(string text)
        {
            List<string> matches = new List<string>();
            Regex regexp = new Regex(GetRegexp(), RegexOptions.IgnoreCase | RegexOptions.Singleline);

            for (Match m = regexp.Match(text); m.Success; m = m.NextMatch())
            {
                //マッチした文字列を追加
                matches.Add(m.Value);
            }

            return matches;

        }
    }

    //マッチした文字列からTargetClassオブジェクトを生成するパーサー
    public class ParserUsingStringConverter<TargetClass> : BaseParser
    {
        //文字列からTargetClassクラスへの変換メソッド型
        public delegate TargetClass ParseDelegate(string str);
        //文字列からTypeクラスへの変換メソッド
        private ParseDelegate m_parser;

        //コンストラクタ
        public ParserUsingStringConverter(ParseDelegate parser)
        {
            //文字列からTargetClassクラスへの変換メソッドを登録
            m_parser = parser;
        }

        //テキスト内から正規表現に一致するTargetClassオブジェトを取得する
        new public List<TargetClass> Parse(string text)
        {
            Regex regexp = new Regex(GetRegexp(), RegexOptions.IgnoreCase | RegexOptions.Singleline);

            List<TargetClass> matches = new List<TargetClass>();
            for (Match m = regexp.Match(text); m.Success; m = m.NextMatch())
            {
                try
                {
                    //マッチした文字列からTargetClassクラスへ変換
                    TargetClass obj = m_parser(m.Value);
                    if (obj != null)
                        matches.Add(obj);
                }catch
                {
                    //パースに失敗した文字列は無視する
                }
            }
            return matches;
        }
    }

    //マッチオブジェクトからTargetClassオブジェクトを生成するパーサー
    public class ParserUsingMatchObjConverter<TargetClass> : BaseParser
    {
        //マッチオブジェクトからTargetClassオブジェクトへの変換メソッド型
        public delegate TargetClass ParseDelegate(Match match);
        //マッチオブジェクトからTargetClassオブジェクトへの変換メソッド
        ParseDelegate m_parser;

        //コンストラクタ
        public ParserUsingMatchObjConverter(ParseDelegate parser)
        {
            //マッチオブジェクトからTargetClassオブジェクトへの変換メソッドを登録
            m_parser = parser;
        }

        //テキスト内から正規表現に一致するTargetClassオブジェトを取得する
        new public List<TargetClass> Parse(string text)
        {
            Regex regexp = new Regex(GetRegexp(), RegexOptions.IgnoreCase | RegexOptions.Singleline);

            List<TargetClass> matches = new List<TargetClass>();
            for (Match m = regexp.Match(text); m.Success; m = m.NextMatch())
            {
                try
                {
                    //マッチオブジェクトからTargetClassクラスへ変換
                    TargetClass obj = m_parser(m);
                    if (obj != null)
                        matches.Add(obj);
                }catch
                {
                    //パースに失敗した文字列は無視する
                }
            }

            return matches;
        }
    }
}

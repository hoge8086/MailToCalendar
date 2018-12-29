namespace AppointmentParser
{
    //テキスト場所解析クラス
    public class TextPlaceParser
    {
        //場所パーサー
        private BaseParser m_parserOfPlace
            = new BaseParser();

        //コンストラクタ
        public TextPlaceParser()
        {
            string[] placeRegexp =
            {
                @"(?<=(場所|会場))[ 　\n\r:：\]］】]*[^\r\n]+",                  // ex: "場所 東京駅", "会場 : 東京 六本木駅", "場所：     東京 六本木 5-2-3 ビル前"
            };

            //場所パーサーへ正規表現の登録
            foreach (string regexp in placeRegexp)
                m_parserOfPlace.AddRegexp(regexp);
        }

        //テキストから予定を解析するメソッド
        public string Parse(string text)
        {
            //場所
            var places = m_parserOfPlace.Parse(text);
            if (places.Count <= 0)
                return "";

            // 先読みで量指定子が使えないのでTrimする
            char[] trimed = { ' ', '　', '\n', '\r', ':', '：', ']', '］', '】' };
            return places[0].Trim(trimed);
        }

    }
}

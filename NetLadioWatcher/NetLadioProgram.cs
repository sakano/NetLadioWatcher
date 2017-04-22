using System;

namespace NetLadioWatcher
{
    public class NetLadioProgram : IEquatable<NetLadioProgram>
    {
        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// DJ名
        /// </summary>
        public string DJ { get; internal set; }

        /// <summary>
        /// ジャンル
        /// </summary>
        public string Genre { get; internal set; }

        /// <summary>
        /// 放送内容
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// 関連URL
        /// </summary>
        public string RelatedURL { get; internal set; }

        /// <summary>
        /// マウント
        /// </summary>
        public string Mount { get; internal set; }

        /// <summary>
        /// 放送開始時刻
        /// </summary>
        public DateTime StartTime { get; internal set; }

        /// <summary>
        /// リスナ数
        /// </summary>
        public int Listener { get; internal set; }

        /// <summary>
        /// 延べリスナ数
        /// </summary>
        public int TotalListener { get; internal set; }

        /// <summary>
        /// 最大リスナ数
        /// </summary>
        public int MaxListener { get; internal set; }

        /// <summary>
        /// ビットレート
        /// </summary>
        public int BitRate { get; internal set; }

        /// <summary>
        /// サンプリングレート
        /// </summary>
        public int SampleRate { get; internal set; }

        /// <summary>
        /// チャンネル数
        /// </summary>
        public int Channel { get; internal set; }

        /// <summary>
        /// 音声フォーマット
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// 曲名
        /// </summary>
        public string Song { get; internal set; }

        /// <summary>
        /// 詳細URL
        /// </summary>
        public string DetailURL { get; internal set; }

        /// <summary>
        /// 放送サーバホスト名
        /// </summary>
        public string ServerHost { get; internal set; }

        /// <summary>
        /// 放送サーバポート番号
        /// </summary>
        public int ServerPort { get; internal set; }

        /// <summary>
        /// 再生URL
        /// </summary>
        public string URL
        {
            get => $"http://{ServerHost}:{ServerPort}{Mount}.m3u";
        }

        public override bool Equals(object other) => other != null && other is NetLadioProgram program && DetailURL == program.DetailURL && StartTime == program.StartTime;

        public bool Equals(NetLadioProgram other) => other != null && DetailURL == other.DetailURL && StartTime == other.StartTime;

        public override int GetHashCode() => DetailURL.GetHashCode() ^ StartTime.GetHashCode();

        public NetLadioProgram Clone() => (NetLadioProgram)MemberwiseClone();

        public override string ToString() => $"{DetailURL} {Title}";
    }
}

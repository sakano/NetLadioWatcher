using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetLadioWatcher
{
    sealed public class NetLadioWatcher : IDisposable
    {
        /// <summary>
        /// 新しい番組が始まったときに呼ばれます
        /// </summary>
        public event EventHandler<ProgramEventArgs> Begun;

        /// <summary>
        /// 番組が終わったときに呼ばれます
        /// </summary>
        public event EventHandler<ProgramEventArgs> Finished;

        /// <summary>
        /// 番組表の更新に失敗したときに呼ばれます
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        private static readonly string url = "http://yp.ladio.net/stats/list.v2.zdat";

        private readonly Encoding shiftJisEncoding;

        private readonly int updateInterval = 15000;

        private readonly HttpClient client = new HttpClient() {
            Timeout = TimeSpan.FromSeconds(1000)
        };

        private CancellationTokenSource cancellationSource;
        private object cancellationLock = new object();

        private HashSet<NetLadioProgram> programs = new HashSet<NetLadioProgram>();
        private readonly object programsLock = new object();

        /// <summary>
        /// 放送中の番組一覧を取得する
        /// </summary>
        /// <returns>放送中の番組一覧</returns>
        public HashSet<NetLadioProgram> GetPrograms()
        {
            lock (programsLock) {
                return new HashSet<NetLadioProgram>(programs);
            }
        }


        /// <summary>
        /// 指定した更新間隔のNetLadioWatcherを初期化します
        /// </summary>
        /// <param name="shiftJisEncoding">Shift_JISの文字エンコーディング</param>
        /// <param name="updateInterval">番組表を更新する間隔</param>
        public NetLadioWatcher(Encoding shiftJisEncoding, int updateInterval) : this(shiftJisEncoding)
        {
            this.updateInterval = updateInterval;
        }

        /// <summary>
        /// NetLadioWatcherを初期化します
        /// </summary>
        /// <param name="shiftJisEncoding">Shift_JISの文字エンコーディング</param>
        public NetLadioWatcher(Encoding shiftJisEncoding)
        {
            this.shiftJisEncoding = shiftJisEncoding;
        }

        /// <summary>
        /// 番組表の監視を開始する
        /// </summary>
        public void Start()
        {
            lock (cancellationLock) {
                if (cancellationSource != null) {
                    throw new InvalidOperationException($"{nameof(NetLadioWatcher)} is alread running.");
                }

                lock (programsLock) {
                    programs.Clear();
                }
                cancellationSource = new CancellationTokenSource();
                var token = cancellationSource.Token;
                Task.Factory.StartNew(Update, token, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        /// <summary>
        /// 番組表の監視を終了する
        /// </summary>
        public void Stop()
        {
            lock (cancellationLock) {
                if (cancellationSource != null) {
                    cancellationSource.Dispose();
                    cancellationSource = null;
                }
            }
        }

        private async void Update(object cancellationToken)
        {
            var token = (CancellationToken)cancellationToken;
            try {
                while (true) {
                    var response = await client.GetAsync(url).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var gzip = new GZipStream(stream, CompressionMode.Decompress, false))
                    using (var reader = new StreamReader(gzip, shiftJisEncoding)) {
                        var program = new NetLadioProgram();
                        var readingPrograms = new HashSet<NetLadioProgram>();
                        string line;
                        lock (cancellationLock) {
                            if (token.IsCancellationRequested) {
                                break;
                            }
                            lock (programsLock) {
                                while ((line = reader.ReadLine()) != null) {
                                    if (line.Length == 0) {
                                        readingPrograms.Add(program);
                                        if (!programs.Contains(program)) {
                                            Begun?.Invoke(this, new ProgramEventArgs(program));
                                        }
                                        program = new NetLadioProgram();
                                    }

                                    int index = line.IndexOf("=");
                                    if (index > 0) {
                                        var key = line.Substring(0, index);
                                        var value = line.Substring(index + 1);
                                        UpdateProgram(program, key, value);
                                    }
                                }

                                foreach (var p in programs.Where(p => !readingPrograms.Contains(p))) {
                                    Finished?.Invoke(this, new ProgramEventArgs(p));
                                }

                                programs = readingPrograms;
                            }
                        }
                    }

                    await Task.Delay(updateInterval);
                }
            } catch (Exception e) {
                lock (cancellationLock) {
                    if (!token.IsCancellationRequested) {
                        Error?.Invoke(this, new ErrorEventArgs(new NetLadioUpdateFailedException("Failed to get program list", e)));
                    }
                }
                return;
            } finally {
                lock (cancellationLock) {
                    if (cancellationSource != null) {
                        cancellationSource.Dispose();
                        cancellationSource = null;
                    }
                }
            }
        }

        private void UpdateProgram(NetLadioProgram program, string key, string value)
        {
            switch (key) {
                case "SURL":
                    program.DetailURL = value;
                    break;
                case "TIMS":
                    program.StartTime = DateTime.Parse(value, new CultureInfo("ja-JP"), DateTimeStyles.AssumeLocal);
                    break;
                case "SRV":
                    program.ServerHost = value;
                    break;
                case "PRT":
                    program.ServerPort = int.Parse(value);
                    break;
                case "MNT":
                    program.Mount = value;
                    break;
                case "TYPE":
                    program.Type = value;
                    break;
                case "NAM":
                    program.Title = value;
                    break;
                case "GNL":
                    program.Genre = value;
                    break;
                case "DESC":
                    program.Description = value;
                    break;
                case "DJ":
                    program.DJ = value;
                    break;
                case "SONG":
                    program.Song = value;
                    break;
                case "URL":
                    program.RelatedURL = value;
                    break;
                case "CLN":
                    program.Listener = int.Parse(value);
                    break;
                case "CLNS":
                    program.TotalListener = int.Parse(value);
                    break;
                case "MAX":
                    program.MaxListener = int.Parse(value);
                    break;
                case "BIT":
                    program.BitRate = int.Parse(value);
                    break;
                case "SMPL":
                    program.SampleRate = int.Parse(value);
                    break;
                case "CHS":
                    program.Channel = int.Parse(value);
                    break;
            }
        }

        public void Dispose() => Stop();
    }
}

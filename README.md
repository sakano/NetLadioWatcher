# NetLadioWatcher
ねとらじの放送開始と放送終了を通知するPCLライブラリ(C#)

# 使用例
```csharp
namespace Example
{
  class Program
  {
    static void Main(string[] args)
    {
      // ねとらじ監視オブジェクトを作成
      using (var watcher = new NetLadioWatcher.NetLadioWatcher(Encoding.GetEncoding("shift_jis"))) {
        watcher.Begun += (sender, e) => {
          // 新しい番組が始まったときに呼ばれる
          
          // 番組の情報はNetLadioProgramとして取得できます。取得できる情報はクラス定義を参照してください
          NetLadioWatcher.NetLadioProgram p = e.Program;
          Console.WriteLine("新しい番組が始まりました。番組名:{0}、開始時刻:{1}", p.Title, p.StartTime);
        };

        watcher.Finished += (sender, e) => {
          // 番組が終了したときに呼ばれる
          NetLadioWatcher.NetLadioProgram p = e.Program;
          Console.WriteLine("番組が終了しました。番組名:{0} ", p.Title);
        };

        // ねとらじの監視を開始
        watcher.Start();

        // 入力があるまで監視しながら待機
        Console.ReadLine();

        // ねとらじの監視を終了
        watcher.Stop();
      }
    }
  }
}
```

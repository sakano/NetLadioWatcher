using System;
using System.Text;

namespace NetLadioWatcherTest
{
    class Program
    {
        static string ToStirng(NetLadioWatcher.NetLadioProgram p)
        {
            return $"{p.Title} :: {p.URL} / {p.StartTime}, {p.DJ}";
        }

        static void Main(string[] args)
        {
            using (var watcher = new NetLadioWatcher.NetLadioWatcher(Encoding.GetEncoding("shift_jis"))) {
                watcher.Begun += (sender, e) => {
                    var p = e.Program;
                    Console.WriteLine("{0}{1}", "Begun: ", ToStirng(p));
                };

                watcher.Finished += (sender, e) => {
                    var p = e.Program;
                    Console.WriteLine("{0}{1}", "Finished: ", ToStirng(p));
                };

                watcher.Error += (sender, e) => {
                    Console.WriteLine("{0}", $"Error: {e.GetException().GetType().ToString()} : {e.GetException().Message}");
                    Console.WriteLine("{0}", $"InnerException: {e.GetException().InnerException.GetType().ToString()} : {e.GetException().InnerException.Message}");
                };

                watcher.Start();

                var loopFlag = true;
                while (loopFlag) {
                    var line = Console.ReadLine();
                    switch (line.ToLower()) {
                        case "start":
                            Console.WriteLine("watcher.Start()");
                            watcher.Start();
                            break;
                        case "stop":
                            Console.WriteLine("watcher.Stop()");
                            watcher.Stop();
                            break;
                        case "list":
                            Console.WriteLine("list");
                            foreach (var p in watcher.GetPrograms()) {
                                Console.WriteLine("{0}", ToStirng(p));
                            }
                            break;
                        case "exit":
                            Console.WriteLine("exit");
                            loopFlag = false;
                            break;
                    }
                }
            }
        }
    }
}
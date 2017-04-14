using System;

namespace NetLadioWatcher
{
    public class ErrorEventArgs : EventArgs
    {
        private readonly Exception e;

        public Exception GetException() => e;

        internal ErrorEventArgs(Exception e) => this.e = e;
    }

    public class ProgramEventArgs : EventArgs
    {
        public NetLadioProgram Program { get; private set; }

        internal ProgramEventArgs(NetLadioProgram program)
        {
            Program = program.Clone();
        }
    }
}

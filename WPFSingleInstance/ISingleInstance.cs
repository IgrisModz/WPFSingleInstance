using System.Collections.Generic;

namespace WPFSingleInstance
{
    public interface ISingleInstance
    {
        bool SignalExternalCommandLineArgs(IList<string> args);
    }
}

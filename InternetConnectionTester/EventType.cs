using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetConnectionTester
{
    public enum EventType
    {
		SystemStartUp = 0, SystemShutDown = 1, ConnectionLoss = 2, GotConnection = 3, ProgramStartUp = 4, ProgramShutDown = 5 
    }
}

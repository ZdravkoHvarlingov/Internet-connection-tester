using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalyser
{
    public class ProgramEvent
    {
        private EventType typeOfEvent;
        private DateTime time;

        public ProgramEvent(EventType typeOfEvent, DateTime time)
        {
            this.typeOfEvent = typeOfEvent;
            this.time = time;
        }

        public EventType TypeOfEvent
        {
            get { return this.typeOfEvent; }
            private set {; }
        }

        public DateTime Time
        {
            get { return this.time; }
            private set {; }
        }
    }
}

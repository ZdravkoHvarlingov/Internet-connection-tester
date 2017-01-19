using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalyser
{
    public class InternetConnectionTester
    {
        private string computerName;
        private string userName;
        private TimeSpan dataTimeSpan;
        private TimeSpan timeWithoutConnection;
        private TimeSpan timeWithConnection;

        public InternetConnectionTester(string computerName, string userName)
        {
            timeWithConnection = new TimeSpan(0, 0, 0);
            timeWithoutConnection = new TimeSpan(0, 0, 0);
            dataTimeSpan = new TimeSpan(0, 0, 0);
            
            this.computerName = computerName;
            this.userName = userName;
        }

        public TimeSpan DataTimeSpan => this.dataTimeSpan;
        public string ComputerName => this.computerName;
        public string UserName => this.userName;
        public void AddEventsToAnalyse(List<ProgramEvent> eventsToBeAdded, TimeSpan timeSpanOfEvents)
        {
            for (int index = 1; index < eventsToBeAdded.Count; index++)
            {
                TimeSpan eventTime;
                if (index != eventsToBeAdded.Count - 1)
                {
                    eventTime =
                        eventsToBeAdded[index + 1].Time - eventsToBeAdded[index].Time;
                }
                else
                {
                    eventTime =
                        (eventsToBeAdded[0].Time + timeSpanOfEvents) - eventsToBeAdded[index].Time;
                }
                if (eventsToBeAdded[index].TypeOfEvent == EventType.ConnectionLoss)
                {
                    timeWithoutConnection += eventTime;
                }
                else
                {
                    timeWithConnection += eventTime;
                }
            }

            dataTimeSpan += (timeSpanOfEvents - (eventsToBeAdded[1].Time - eventsToBeAdded[0].Time));
        }

        public Decimal TimeWithInternetInPercentages()
        {
            decimal percent = (Decimal)(timeWithConnection.TotalSeconds / dataTimeSpan.TotalSeconds);
            percent *= 100;

            percent = Math.Round(percent, 2);

            return percent;
        }
    }
}

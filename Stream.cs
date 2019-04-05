using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskWpf
{

    public class Stream
    {
        int id;
        String state;
        String startTime;
        public Stream()
        {

        }
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public String State
        {
            get { return state; }
            set { state = value; }
        }
        public String StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }
    }
}

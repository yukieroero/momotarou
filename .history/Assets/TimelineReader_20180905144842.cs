using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Timeline {
    public class TimelineReader
    {
        private TextAsset csvFile;
        private List <string[]>timeLineBody = new List<string[]>();
        private List <string[]>timeLineHead = new List<string[]>();
        private int height = 0;
        private Dictionary<string, string> identifier = new Dictionary<string, string>() {
            {"header", "[head]"},
            {"body", "[body]"},
        };
        public TimelineReader(TextAsset csvFile)
        {
            this.csvFile = csvFile;
            StringReader reader = new StringReader(csvFile.text);
            bool header = false;
            bool body = false;
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                if (line == identifier["header"]) header = true; body=false;
                else if (line == identifier["body"]) header = false; body=true;
                TimeLineBody.Add(line.Split(',')); // リストに入れる
                height++; // 行数加算
            }
        }

        public List<string[]> TimeLineBody
        {
            get
            {
                return timeLineBody;
            }
        }
        public List<string[]> TimeLineHead
        {
            get
            {
                return timeLineBody;
            }
        }
    }
}

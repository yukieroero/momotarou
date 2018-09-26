using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TimelineReader {
    public class TimelineReader
    {
        private TextAsset csvFile;
        private List <string[]>timeLineDatas = new List<string[]>();
        public TimelineReader(TextAsset csvFile)
        {
            this.csvFile = csvFile;
            StringReader reader = new StringReader(csvFile.text);
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                timeLineDatas.Add(line.Split(',')); // リストに入れる
                height++; // 行数加算
            }
        }

        public List<string> TimeLineDatas
        {
            get
            {
                return timeLineDatas;
            }
    }
}

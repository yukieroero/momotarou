using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Timeline {
    public class TimelineReader
    {
        private TextAsset csvFile;
        private List <string[]>timeLineDatas = new List<string[]>();
        private int height = 0;
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
            height = 0;
        }

        public IEnumerable<string[]> next()
        {
            Debug.Log(timeLineDatas[height][0]);
            yield return timeLineDatas[height];
            height++;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Timeline {
    public class Reader
    {
        private TextAsset csvFile;
        private List <string[]>timeLineDatas = new List<string[]>();
        private int height = 0;
        public Reader(TextAsset csvFile)
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

        IEnumerable<string[]> next()
        {
            yield return timeLineDatas[height];
            height++;
        }
    }
}

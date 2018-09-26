using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Timeline {

    public class HeadHandler {
        private Dictionary <string, Person> persons = new Dictionary<string, Person>();
        private List<string> headerCommandList = new List<string> {
            // 登場人物の定義
            "person",
        };
        public HeadHandler(List<string[]> headDatas) {
            foreach (string[] line in headDatas) {
                string command = line[0];
                if (headerCommandList.Contains(command)) {
                    switch (command) {
                        case "person":
                            string identifier = line[1];
                            GetPersons().Add(identifier, new Person(line));
                            break;
                    }
                }
            }
        }

        public Dictionary<string, Person> GetPersons()
        {
            return persons;
        }
    }
    public class BodyHandler {
        private List<string> bodyCommandList = new List<string> {
            // 登場人物の定義
            "scene",
        };
        private int index = 0;
        private List<string[]> datas;
        public BodyHandler (List<string[]> bodyDatas) {
            datas = bodyDatas;
        }
        public void play() {
            string[] line = datas[index];
            string command = line[0];
            if (bodyCommandList.Contains(command)) {
                switch (command) {
                    case "scene":
                        string path = line[3];
                        setBackground(path);
                        break;
                }
                this.incrementIndex();
            } else {
                this.next();
            }

        }
        public void next() {
            this.incrementIndex();
            this.play();
        }
        public void incrementIndex() {
            index++;
        }
    }

    public class AbsCommand {
        public AbsCommand() {

        }
    }
    public class Person : AbsCommand {
        private string identifier;
        private string name;
        private string icon;
        private string image;
        // person,oldwoman,おばあさん,,oldwoman/normal
        public Person(string[] personDatas) {
            identifier = personDatas[1];
            name = personDatas[2];
            icon = personDatas[3];
            image = personDatas[4];
        }
    }
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
                if (line == identifier["header"]) {
                    header = true;
                    body=false;
                } else if (line == identifier["body"]) {
                    header = false;
                    body = true;
                } else if (line.Length > 0) {
                    if (header) GetTimeLineHead().Add(line.Split(',')); // リストに入れる
                    else if (body) GetTimeLineBody().Add(line.Split(',')); // リストに入れる
                }
                height++; // 行数加算
            }
        }

        public List<string[]> GetTimeLineBody()
        {
            return timeLineBody;
        }

        public List<string[]> GetTimeLineHead()
        {
            return timeLineBody;
        }
    }
}

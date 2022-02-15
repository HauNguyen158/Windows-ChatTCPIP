using System;
namespace ChatTCPIP
{
    class Messenger
    {
        public string image { set; get; }
        public string name { set; get; }
        public string text { set; get; }
        public string receiver { set; get; }
        public bool all { set; get; }
        public DateTime time { set; get; }
        public int emoji { set; get; }
        public string file { set; get; }
        public string fileName { set; get; }
        public bool trans { set; get; }
    }
}

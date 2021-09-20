namespace TCManager
{
    internal class CM
    {
        public string username { get; set; }
        public string message { get; set; }
        public bool isMod { get; set; }
        public bool isSub { get; set; }

        public override string ToString()
        {
            string temp = "";
            if(isMod)
            {
                temp += "[MOD]";
            }
            if(isSub) {
                temp += "[SUB]";
            }
            temp += username + " : " + message;
            return temp;
        }
    }
}
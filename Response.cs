using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ServerVote
{
    class Response
    {
        public string response;
        public string message;

        public static Response Read(string text)
        {
            return JsonConvert.DeserializeObject<Response>(text);
        }
    }
}

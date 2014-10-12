using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Bson;

namespace MvcTNPT.Models
{
    public class User
    {
        public User()
        {
        }

        public ObjectId id { get; set; }
        [BsonElementAttribute("first")]
        public string FirstName { get; set; }
        [BsonElementAttribute("last")]
        public string LastName { get; set; }
        [BsonElementAttribute("birth")]
        public DateTime Birth { get; set; }
        [BsonElementAttribute("death")]
        public DateTime Death { get; set; }
        [BsonElementAttribute("awards")]
        public Array Awards { get; set; }
        [BsonElementAttribute("contribs")]
        public Array Contribs { get; set; }
        [BsonElementAttribute("title")]
        public string title { get; set; }


    }
}
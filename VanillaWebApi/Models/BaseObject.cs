using System.Collections.Generic;

namespace VanillaWebApi.Models
{
    // BaseObject & actions inspired by this excellent article from Phil Sturgeon https://philsturgeon.uk/api/2017/06/19/representing-state-in-rest-and-graphql/
    public class BaseObject
    {
        public BaseObject()
        {
            actions = new List<ActionItem>();
        }

        public List<ActionItem> actions { get; set; }
        public string self { get; set; }
    }

    public class ActionItem
    {
        public ActionItem()
        {
            fields = new List<Field>();
        }

        public string name { get; set; }
        public string title { get; set; }
        public string method { get; set; }
        public string href { get; set; }
        public string type { get; set; }
        public List<Field> fields { get; set; }
    }

    public class Field
    {
        public string name { get; set; }
        public string type { get; set; }
    }
}
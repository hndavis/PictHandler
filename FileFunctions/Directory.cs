using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFunctions
{
    public class Directory
    {
        public String Location { get { return _location; } }
        public String Name { get { return _name; } } 

        List<string> _contentItems = new List<string>();

        string _location;
        string _name;
        Directory(String location, String name)
        {
            _location = location;
            _name = name;

        }

        public List<string>  ContentItems { get { return _contentItems; } }


    }
}

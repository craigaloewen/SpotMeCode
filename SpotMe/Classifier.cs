using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotMe
{
    /// <summary>
    /// Stores all the information necessary to describe a classifier of an exercise's form
    /// </summary>
    public class Classifier
    {
        public string name;

        public Classifier(string inName)
        {
            name = inName;
        }
    }
}

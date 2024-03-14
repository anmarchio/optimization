using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Commandline.Tests.Categories
{
    /// <summary>
    /// Klasse um Halcon Test zu kategorisieren
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class Halcon : CategoryAttribute
    {
    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests.Categories
{
    /// <summary>
    /// Klasse um längere Tests zu kategorisieren, die 5 Sekunden oder länger dauern.
    /// </summary> 
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class LongTest : CategoryAttribute
    {
    }
}

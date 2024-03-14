using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests.Categories
{
    /// <summary>
    /// Klasse um kurze Tests zu kategorisieren, die kurzer als 5 Sekunden dauern
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ShortTest : CategoryAttribute
    {
    }
}


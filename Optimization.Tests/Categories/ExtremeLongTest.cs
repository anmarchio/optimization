using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests.Categories
{
    /// <summary>
    /// Klasse um sehr lange Tests zu kategorisieren(Die wurden nicht fertig durchgeführt, weil es so lange gedauert hat oder weil bekannt ist und da steht, dass die so lange dauern)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ExtremeLongTest : CategoryAttribute
    {
    }
}

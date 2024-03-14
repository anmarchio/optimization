using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests.Categories
{
    /// <summary>
    /// author: braml
    /// Class for tests that can not be executed on the server,
    /// because of restrictions of used frameworks or license agreements.
    /// Tests with that category should be executed locally,
    /// meaning on a machine with a local solution and the needed frameworks and interfaces
    /// </summary> 
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class LocalTest : CategoryAttribute
    {
    }
}

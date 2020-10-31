using NStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Randgen.Test
{
    public class StringGeneratorTests
    {
        [Fact]
        public void Test1()
        {
            var taken = new List<string>();
            for (int i = 0; i < 20; i++)
            {
                var generator = new StringGenerator("$d$d", 0);

                var items = generator.Take(5, taken.ToArray());
                taken.AddRange(items);
            }

            Assert.Equal(RangeEx.Create(0, 100), taken.Select(x => int.Parse(x)).OrderBy(x => x));
        }
    }
}

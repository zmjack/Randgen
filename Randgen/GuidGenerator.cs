using NStandard;
using System;
using System.Collections.Generic;

namespace Randgen
{
    public class GuidGenerator : IGenerator<Guid>
    {
        private Guid prevGeneratedCode;

        public Guid[] Take(int count)
        {
            lock (this)
            {
                var ret = new List<Guid>();
                for (int i = 0; i < count; i++)
                {
                    Guid code;
                    do { code = Guid.NewGuid(); }
                    while (code.Equals(prevGeneratedCode));

                    prevGeneratedCode = code;
                    ret.Add(code);
                }
                return ret.ToArray();
            }
        }

        public Guid TakeOne()
        {
            lock (this)
            {
                Guid code;
                do { code = Guid.NewGuid(); }
                while (code.Equals(prevGeneratedCode));

                prevGeneratedCode = code;
                return code;
            }
        }

    }
}

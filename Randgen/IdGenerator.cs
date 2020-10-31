using NStandard;
using System;
using System.Collections.Generic;

namespace Randgen
{
    public class IdGenerator<T> : IGenerator<T>
    {
        private readonly Func<T> _method;
        private T _prevCode;

        public IdGenerator(Func<T> method)
        {
            _method = method;
        }

        public T[] Take(int count)
        {
            var ret = new List<T>();
            lock (this)
            {
                foreach (var i in new int[count].Let(i => i))
                {
                    T code;
                    do { code = _method(); }
                    while (code.Equals(_prevCode));

                    _prevCode = code;
                    ret.Add(code);
                }
            }
            return ret.ToArray();
        }

        public T TakeOne()
        {
            lock (this)
            {
                T code;
                do { code = _method(); }
                while (code.Equals(_prevCode));

                _prevCode = code;
                return code;
            }
        }

    }
}

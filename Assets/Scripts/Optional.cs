#nullable enable
using System;

namespace DefaultNamespace
{
    public readonly struct Optional<T>
    {
        private readonly T? _data;

        public Optional(T? data) {
            _data = data;
        }

        public T GetOr(T other) => _data ?? other;

        public bool Exist() => _data != null;

        public T Get() {
            if (!Exist()) {
                throw new ArgumentException("You forgot to check if the value does exist! For that, use the method 'Exist()' before accessing the value, or use 'GetOr()");
            }

            return _data;
        }
                
    }
}
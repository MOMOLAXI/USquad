﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or 

using System;

namespace UniverseEngine
{
    public readonly struct AsyncUnit : IEquatable<AsyncUnit>
    {
        public static readonly AsyncUnit Default = new();

        public override int GetHashCode()
        {
            return 0;
        }

        public bool Equals(AsyncUnit other)
        {
            return true;
        }

        public override string ToString()
        {
            return "()";
        }
    }
}

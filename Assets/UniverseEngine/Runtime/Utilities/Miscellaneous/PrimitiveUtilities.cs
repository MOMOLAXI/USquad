using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniverseEngine
{
    public static class PrimitiveUtilities
    {
        const float FLOAT_ELPISE = 0.0001f;

        public static bool FloatEquals(float lhs, float rhs)
        {
            return Math.Abs(lhs - rhs) < FLOAT_ELPISE;
        }

        public static bool SetColor(ref Color currentValue, Color newValue)
        {
            if (FloatEquals(currentValue.r, newValue.r)
             && FloatEquals(currentValue.g, newValue.g)
             && FloatEquals(currentValue.b, newValue.b)
             && FloatEquals(currentValue.a, newValue.a))
            {
                return false;
            }

            currentValue = newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if (currentValue == null && newValue == null ||
                currentValue != null && currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        /// <summary>
        /// Swap Generic
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <typeparam name="T"></typeparam>
        public static void Swap<T>(ref T left, ref T right)
        {
            (left, right) = (right, left);
        }

        /// <summary>
        /// Swap
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void PlusSwap(ref int left, ref int right)
        {
            left += right;
            right = left - right;
            left -= right;
        }

        /// <summary>
        /// Swap
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void Swap(ref int left, ref int right)
        {
            left ^= right;
            right ^= left;
            left ^= right;
        }

        /// <summary>
        /// Swap
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void Swap(ref long left, ref long right)
        {
            left ^= right;
            right ^= left;
            left ^= right;
        }

        /// <summary>
        /// Swap
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void Swap(ref ulong left, ref ulong right)
        {
            left ^= right;
            right ^= left;
            left ^= right;
        }
    }
}

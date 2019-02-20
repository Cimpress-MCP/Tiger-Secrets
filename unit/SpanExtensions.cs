using System;

namespace Test
{
    public static class SpanExtensions
    {
        public static void Deconstruct<TElement>(this in ReadOnlySpan<TElement> readOnlySpan, out TElement head, out ReadOnlySpan<TElement> tail)
        {
            if (readOnlySpan.Length == 0) { throw new ArgumentException("Empty collection cannot be deconstructed.", nameof(readOnlySpan)); }

            head = readOnlySpan[0];
            tail = readOnlySpan.Slice(1);
        }
    }
}

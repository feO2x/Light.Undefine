using System;
using Light.GuardClauses;

namespace Light.Undefine
{
    internal struct SourceCodeSink
    {
        private readonly char[] _internalArray;
        private int _currentIndex;

        public SourceCodeSink(char[] internalArray)
        {
            _internalArray = internalArray.MustNotBeNull(nameof(internalArray));
            _currentIndex = 0;
        }

        public void Append(ReadOnlySpan<char> code)
        {
            for (var i = 0; i < code.Length; i++)
                _internalArray[_currentIndex++] = code[i];
        }

        public Memory<char> ToMemory() => new Memory<char>(_internalArray, 0, _currentIndex);
    }
}
#nullable enable
using System;

namespace AQ.SharedKernel
{
    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? Error { get; }

        private Result(bool ok, T? value, string? error)
        {
            IsSuccess = ok; Value = value; Error = error;
        }

        public static Result<T> Ok(T value) => new Result<T>(true, value, null);
        public static Result<T> Fail(string message) =>
            new Result<T>(false, default, string.IsNullOrWhiteSpace(message) ? "Unspecified failure" : message);

        public T OrThrow() => IsSuccess
            ? (Value is null ? throw new InvalidOperationException("Ok(null) not allowed") : Value)
            : throw new InvalidOperationException(Error ?? "Result failed.");

        public override string ToString() => IsSuccess ? $"Ok({Value})" : $"Fail({Error})";
    }
}

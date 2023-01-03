namespace Leeve.Core.Common;

public enum ResultState : byte
{
    Faulted,
    Success
}

public readonly struct Result
{
    private readonly ResultState _state;
    private readonly Exception? _exception;

    public Result()
    {
        _state = ResultState.Success;
        _exception = null;
    }

    public Result(Exception e)
    {
        _state = ResultState.Faulted;
        _exception = e;
    }

    private Result(ResultState state)
    {
        _state = state;
        _exception = null;
    }

    public bool IsFaulted => _state == ResultState.Faulted;

    public bool IsSuccess => _state == ResultState.Success;

    public static Result Faulted => new(ResultState.Faulted);

    public override string ToString() =>
        IsFaulted
            ? _exception?.Message ?? "Faulted"
            : "Success";
}

public readonly struct Result<T>
{
    private readonly ResultState _state;
    private readonly Exception? _exception;
    private readonly T? _value;

    public Result(T value)
    {
        _state = ResultState.Success;
        _value = value;
        _exception = null;
    }

    public Result(Exception e)
    {
        _state = ResultState.Faulted;
        _exception = e;
        _value = default;
    }

    private Result(ResultState state)
    {
        _state = state;
        _exception = null;
        _value = default;
    }

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator T(Result<T> result) => result._value ?? throw new NullReferenceException();

    public T Value => _value ?? throw new NullReferenceException();

    public bool IsFaulted => _state == ResultState.Faulted;

    public bool IsSuccess => _state == ResultState.Success;

    public static Result<T> Faulted => new(ResultState.Faulted);

    public override string ToString() => IsFaulted
        ? _exception?.Message ?? "Faulted"
        : _value?.ToString() ?? "Success";
}
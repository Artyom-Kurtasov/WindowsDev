namespace WindowsDev.Business.Primitives
{
    public class Result <TValue>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        private readonly TValue _value;
        public TValue Value
        {
            get
            {
                if (IsFailure)
                    throw new InvalidOperationException(
                        "Cannot access Value when result is failure");

                return _value;
            }
        }
        public string Error { get; }

        public Result(bool isSuccess, TValue value, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
            _value = value;
        }

        public static Result<TValue> Success(TValue value)
            => new(true, value, string.Empty);
        public static Result<TValue> Failure(string error)
            => new(false, default!, error);
    }
}

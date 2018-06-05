using System.Collections.Generic;

namespace CheckerApi.Models
{
    public class Result
    {
        protected Result(bool status, params string[] messages)
        {
            Status = status;
            Messages = new List<string>();
            Messages.AddRange(messages);
        }

        public List<string> Messages { get; }

        public bool Status { get; }
        
        public bool IsSuccess()
        {
            return Status == true;
        }

        public bool HasFailed()
        {
            return Status == false;
        }

        public static Result Ok(params string[] messages)
        {
            return new Result(true, messages);
        }

        public static Result Fail(params string[] messages)
        {
            return new Result(false, messages);
        }
    }

    public class Result<T> : Result
    {
        protected Result(T value, bool status, params string[] messages) : base(status, messages)
        {
            Value = value;
        }

        public T Value { get; }

        public static Result<T> Ok(T value, params string[] messages)
        {
            return new Result<T>(value, true, messages);
        }

        public static Result<T> Fail(T value, params string[] messages)
        {
            return new Result<T>(value, false, messages);
        }

        public new static Result<T> Fail(params string[] messages)
        {
            return new Result<T>(default(T), false, messages);
        }
    }
}

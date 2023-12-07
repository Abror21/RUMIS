using System.Collections.Generic;

namespace Izm.Rumis.Application.Common
{
    public class AppResult<TData> : AppResult
    {
        public TData Data { get; set; }
    }

    public class AppResult
    {
        public AppResult() { }

        public AppResult(IEnumerable<string> errors)
        {
            Set(errors);
        }

        public AppResult(params string[] errors)
        {
            Set(errors);
        }

        public AppResult(bool success)
        {
            this.success = success;
        }

        private List<string> errors = new List<string>();
        public IEnumerable<string> Errors { get { return errors; } }

        private bool? success;
        public bool Success { get { return success.HasValue ? success.Value : errors.Count == 0; } }

        public AppResult Add(IEnumerable<string> errors)
        {
            foreach (string err in errors)
            {
                if (!string.IsNullOrEmpty(err))
                    this.errors.Add(err);
            }

            return this;
        }

        public AppResult Add(params string[] errors)
        {
            foreach (string err in errors)
            {
                if (!string.IsNullOrEmpty(err))
                    this.errors.Add(err);
            }

            return this;
        }

        public AppResult Set(IEnumerable<string> errors)
        {
            this.errors.Clear();
            Add(errors);

            return this;
        }

        public AppResult Set(params string[] errors)
        {
            this.errors.Clear();
            Add(errors);

            return this;
        }

        public AppResult Set(bool success)
        {
            this.success = success;
            return this;
        }

        public static AppResult Succeeded()
        {
            return new AppResult(true);
        }

        public static AppResult Failed()
        {
            return new AppResult(false);
        }

        public static AppResult Failed(params string[] errors)
        {
            return new AppResult(errors);
        }

        public static AppResult Failed(IEnumerable<string> errors)
        {
            return new AppResult(errors);
        }
    }
}

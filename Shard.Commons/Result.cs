using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shard.Commons
{
    public interface IResult
    {
        bool Success { get; }
        string Message { get; }
    }

    public class Result : IResult
    {
        public bool Success { get; protected set; }
        public string Message { get; protected set; }
        public string Category { get; protected set; }
        public string ErrorCode { get; protected set; }

        public Result(bool success, string message, string category = "", string errorCode = "")
        {
            Success = success;
            Message = message;
            Category = category;
            ErrorCode = errorCode;
        }

        public static implicit operator bool(Result res)
        {
            return res.Success;
        }

        public static implicit operator Result(bool res)
        {
            return new Result(res, res ? "Success" : "Failure");
        }

        public static implicit operator Result(ResultIntention res)
        {
            return new Result(res.Success, res.Message);
        }

        public override string ToString()
        {
            if (Success) return "Success";
            else return Message.IfNullOrBlank("Failure");
        }
    }

    public class Result<DType> : Result
    {
        public DType Data { get; protected set; }

        public Result(bool success, string message, DType data, string category = "", string errorCode = "")
            : base(success, message, category, errorCode)
        {
            Data = data;
        }

        public static implicit operator Result<DType>(bool res)
        {
            return new Result<DType>(res, res ? "Success" : "Failure", default(DType));
        }

        public static implicit operator Result<DType>(DType data)
        {
            var success = !ReferenceEquals(data, null);
            return new Result<DType>(success, success ? "Success" : "Failure", data);
        }

        public static implicit operator Result<DType>(ResultIntention res)
        {
            return new Result<DType>(res.Success, res.Message, res.DataAs<DType>(), res.Category, res.ErrorCode);
        }
    }

    public class Result<DType, FType> : Result<DType>
    {
        public FType FailData { get; protected set; }

        public Result(bool success, string message, DType data, FType failData)
            : base(success, message, data)
        {
            FailData = failData;
        }

        public static implicit operator Result<DType, FType>(bool res)
        {
            return new Result<DType, FType>(res, res ? "Success" : "Failure", default(DType), default(FType));
        }

        public static implicit operator Result<DType, FType>(DType data)
        {
            var success = !ReferenceEquals(data, null);
            return new Result<DType, FType>(success, success ? "Success" : "Failure", data, default(FType));
        }

        public static implicit operator Result<DType, FType>(FType fdata)
        {
            return new Result<DType, FType>(false, "Failure", default(DType), fdata);
        }

        public static implicit operator Result<DType, FType>(ResultIntention res)
        {
            var data = res.DataAs<DType>();
            var fdata = res.FailAs<FType>();
            return new Result<DType, FType>(res.Success, res.Message, data, fdata);
        }
    }

    public class ResultIntention
    {
        public ResultIntention(bool success) : this(success, success ? "Success" : "Failure") { }
        public ResultIntention(bool success, string message) { Success = success; Message = message; }
        public ResultIntention(bool success, string message, string category, string errorCode) { Success = success; Message = message; Category = category; ErrorCode = errorCode; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public object FailData { get; set; }
        public string Category { get; set; }
        public string ErrorCode { get; set; }

        public T DataAs<T>() { return As<T>(Data); }
        public T FailAs<T>() { return As<T>(FailData); }
        T As<T>(object o)
        {
            if (o == null) return default(T);
            if (o is T) return (T)o;
            var t = typeof(T);
            var tInfo = t.GetTypeInfo();
            if (tInfo.IsValueType)//If it's a value type, try a type change and hope for the best
                try
                {
                    if (tInfo.IsGenericType && tInfo.GetGenericTypeDefinition() == typeof(Nullable<>)) //deal with nullables..
                        t = Nullable.GetUnderlyingType(t);
                    return (T)Convert.ChangeType(o, t);
                }
                catch { return default(T); }
            else if (tInfo.IsAssignableFrom(o.GetType().GetTypeInfo())) return (T)Convert.ChangeType(o, t);
            else return default(T);
        }

        public TRet To<TRet>() => (TRet)To(typeof(TRet));
        public object To(Type type)
        {
            MethodInfo converter;

            if (this is ValidatedIntention)
            {
                converter = type.GetRuntimeMethod("op_Implicit", new[] { typeof(ValidatedIntention) });
                if (converter != null) return converter.Invoke(null, new[] { this }); ;
            }

            converter = type.GetRuntimeMethod("op_Implicit", new[] { typeof(ResultIntention) });
            if (converter != null) return converter.Invoke(null, new[] { this }); ;

            throw new ArgumentOutOfRangeException("ret", "Unable to convert to " + type.FullName);
        }

        public static implicit operator bool(ResultIntention res) => res.Success;
    }

    public static class Results
    {
        public static ResultIntention Success() { return new ResultIntention(true); }
        public static ResultIntention Success(object data) { return new ResultIntention(true) { Data = data }; }

        public static ResultIntention Failure(string error) { return new ResultIntention(false, error); }
        public static ResultIntention Failure(object data) { return new ResultIntention(false) { Data = data, FailData = data }; }
        public static ResultIntention Failure(object data, string error) { return new ResultIntention(false, error) { Data = data, FailData = data }; }
        public static ResultIntention Failure(object data, object fdata) { return new ResultIntention(false) { Data = data, FailData = fdata }; }
        public static ResultIntention Failure(object data, object fdata, string error) { return new ResultIntention(false, error) { Data = data, FailData = fdata }; }

        public static TRet OnFail<TRet>(this Result result, Func<Result, TRet> action) { if (!result) return action(result); else return default(TRet); }
        public static TRet OnFail<TRet, DType>(this Result<DType> result, Func<Result<DType>, TRet> action) { if (!result) return action(result); else return default(TRet); }
        public static TRet OnFail<TRet, DType, FType>(this Result<DType, FType> result, Func<Result<DType, FType>, TRet> action) { if (!result) return action(result); else return default(TRet); }
        public static TRet OnSuccess<TRet>(this Result result, Func<Result, TRet> action) { if (result) return action(result); else return default(TRet); }
        public static TRet OnSuccess<TRet, DType>(this Result<DType> result, Func<Result<DType>, TRet> action) { if (result) return action(result); else return default(TRet); }

        public static void OnFail(this Result result, Action<Result> action) { if (!result) action(result); }
        public static void OnFail<DType>(this Result<DType> result, Action<Result<DType>> action) { if (!result) action(result); }
        public static void OnFail<DType, FType>(this Result<DType, FType> result, Action<Result<DType, FType>> action) { if (!result) action(result); }
        public static void OnSuccess(this Result result, Action<Result> action) { if (result) action(result); }
        public static void OnSuccess<DType>(this Result<DType> result, Action<Result<DType>> action) { if (result) action(result); }

        public static ResultIntention From(Result result) { return new ResultIntention(result.Success, result.Message, result.Category, result.ErrorCode); }
        public static ResultIntention From<DType>(Result<DType> result) { return new ResultIntention(result.Success, result.Message) { Data = result.Data }; }
        public static ResultIntention From<DType, FType>(Result<DType, FType> result) { return new ResultIntention(result.Success, result.Message) { Data = result.Data, FailData = result.FailData }; }
    }
}

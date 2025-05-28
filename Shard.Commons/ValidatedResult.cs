using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard.Commons
{
    public interface IValidatedResult : IResult
    {
        List<ValidationError> ValidationErrors { get; }
    }

    public class ValidatedResult : Result, IValidatedResult
    {
        public List<ValidationError> ValidationErrors { get; protected set; }

        public ValidatedResult(bool success, string message, List<ValidationError> validationErrors, string category = "", string errorCode = "")
            : base(success, message, category, errorCode)
        {
            ValidationErrors = validationErrors ?? new List<ValidationError>();
        }

        public static implicit operator bool(ValidatedResult res)
        {
            return res.Success;
        }

        public static implicit operator ValidatedResult(bool res)
        {
            return new ValidatedResult(res, res ? "Success" : "Failure", null);
        }

        public static implicit operator ValidatedResult(ResultIntention res)
        {
            return new ValidatedResult(res.Success, res.Message, null);
        }
        public static implicit operator ValidatedResult(ValidatedIntention res)
        {
            return new ValidatedResult(res.Success, res.Message, res.Validation);
        }
    }

    public class ValidatedResult<DType> : ValidatedResult
    {
        public DType Data { get; protected set; }

        public ValidatedResult(bool success, string message, DType data, List<ValidationError> validationErrors)
            : base(success, message, validationErrors)
        {
            Data = data;
        }

        public static implicit operator Result<DType>(ValidatedResult<DType> res)
        {
            return new Result<DType>(res.Success, res.Message, res.Data);
        }

        public static implicit operator ValidatedResult<DType>(DType data)
        {
            var success = !ReferenceEquals(data, null);
            return new ValidatedResult<DType>(success, success ? "Success" : "Failure", data, null);
        }

        public static implicit operator ValidatedResult<DType>(ResultIntention res)
        {
            return new ValidatedResult<DType>(res.Success, res.Message, res.DataAs<DType>(), null);
        }

        public static implicit operator ValidatedResult<DType>(ValidatedIntention res)
        {
            return new ValidatedResult<DType>(res.Success, res.Message, res.DataAs<DType>(), res.Validation);
        }
    }

    public class ValidatedResult<DType, FType> : ValidatedResult<DType>
    {
        public FType FailData { get; protected set; }

        public ValidatedResult(bool success, string message, DType data, FType failData, List<ValidationError> validationErrors)
            : base(success, message, data, validationErrors)
        {
            FailData = failData;
        }

        public static implicit operator Result<DType, FType>(ValidatedResult<DType, FType> res)
        {
            return new Result<DType, FType>(res.Success, res.Message, res.Data, res.FailData);
        }
        public static implicit operator Result<DType>(ValidatedResult<DType, FType> res)
        {
            return new Result<DType>(res.Success, res.Message, res.Data);
        }

        public static implicit operator ValidatedResult<DType, FType>(DType data)
        {
            var success = !ReferenceEquals(data, null);
            return new ValidatedResult<DType, FType>(success, success ? "Success" : "Failure", data, default(FType), null);
        }

        public static implicit operator ValidatedResult<DType, FType>(FType fdata)
        {
            return new ValidatedResult<DType, FType>(false, "Failure", default(DType), fdata, null);
        }

        public static implicit operator ValidatedResult<DType, FType>(ResultIntention res)
        {
            var data = res.DataAs<DType>();
            var fdata = res.FailAs<FType>();
            return new ValidatedResult<DType, FType>(res.Success, res.Message, data, fdata, null);
        }

        public static implicit operator ValidatedResult<DType, FType>(ValidatedIntention res)
        {
            var data = res.DataAs<DType>();
            var fdata = res.FailAs<FType>();
            return new ValidatedResult<DType, FType>(res.Success, res.Message, data, fdata, res.Validation);
        }
    }

    public class ValidatedIntention : ResultIntention
    {
        public ValidatedIntention(bool result) : base(result) { }
        public ValidatedIntention(bool result, string message) : base(result, message) { }
        public ValidatedIntention(bool result, string message, string category, string errorCode) : base(result, message, category, errorCode) { }
        public List<ValidationError> Validation { get; set; }
    }

    public static class ValidatedResults
    {
        public static ValidatedIntention Success() { return new ValidatedIntention(true); }
        public static ValidatedIntention Success(object data) { return new ValidatedIntention(true) { Data = data }; }

        public static ValidatedIntention Failure(string error) { return new ValidatedIntention(false, error); }
        public static ValidatedIntention Failure(string error, string category, string errorCode) { return new ValidatedIntention(false, error, category, errorCode); }
        public static ValidatedIntention Failure(object data) { return new ValidatedIntention(false) { Data = data, FailData = data }; }
        public static ValidatedIntention Failure(object data, string error) { return new ValidatedIntention(false, error) { Data = data, FailData = data }; }
        public static ValidatedIntention Failure(object data, object fdata) { return new ValidatedIntention(false) { Data = data, FailData = fdata }; }
        public static ValidatedIntention Failure(object data, object fdata, string error) { return new ValidatedIntention(false, error) { Data = data, FailData = fdata }; }

        public static ValidatedIntention Failure(string error, IEnumerable<ValidationError> validation) { return new ValidatedIntention(false, error) { Validation = validation.ToList() }; }
        public static ValidatedIntention Failure(object data, IEnumerable<ValidationError> validation) { return new ValidatedIntention(false) { Data = data, FailData = data, Validation = validation.ToList() }; }
        public static ValidatedIntention Failure(object data, IEnumerable<ValidationError> validation, string error) { return new ValidatedIntention(false, error) { Data = data, FailData = data, Validation = validation.ToList() }; }
        public static ValidatedIntention Failure(object data, object fdata, IEnumerable<ValidationError> validation) { return new ValidatedIntention(false) { Data = data, FailData = fdata, Validation = validation.ToList() }; }
        public static ValidatedIntention Failure(object data, object fdata, string error, IEnumerable<ValidationError> validation) { return new ValidatedIntention(false, error) { Data = data, FailData = fdata, Validation = validation.ToList() }; }

        public static TRet OnFail<TRet>(this ValidatedResult result, Func<ValidatedResult, TRet> action) { if (!result) return action(result); else return default(TRet); }
        public static TRet OnFail<TRet, DType>(this ValidatedResult<DType> result, Func<ValidatedResult<DType>, TRet> action) { if (!result) return action(result); else return default(TRet); }
        public static TRet OnFail<TRet, DType, FType>(this ValidatedResult<DType, FType> result, Func<ValidatedResult<DType, FType>, TRet> action) { if (!result) return action(result); else return default(TRet); }
        public static TRet OnSuccess<TRet>(this ValidatedResult result, Func<ValidatedResult, TRet> action) { if (result) return action(result); else return default(TRet); }
        public static TRet OnSuccess<TRet, DType>(this ValidatedResult<DType> result, Func<ValidatedResult<DType>, TRet> action) { if (result) return action(result); else return default(TRet); }

        public static void OnFail(this ValidatedResult result, Action<ValidatedResult> action) { if (!result) action(result); }
        public static void OnFail<DType>(this ValidatedResult<DType> result, Action<ValidatedResult<DType>> action) { if (!result) action(result); }
        public static void OnFail<DType, FType>(this ValidatedResult<DType, FType> result, Action<ValidatedResult<DType, FType>> action) { if (!result) action(result); }
        public static void OnSuccess(this ValidatedResult result, Action<ValidatedResult> action) { if (result) action(result); }
        public static void OnSuccess<DType>(this ValidatedResult<DType> result, Action<ValidatedResult<DType>> action) { if (result) action(result); }

        public static ValidatedIntention From(ValidatedResult result) { return new ValidatedIntention(result.Success, result.Message) { Validation = result.ValidationErrors }; }
        public static ValidatedIntention From<DType>(ValidatedResult<DType> result) { return new ValidatedIntention(result.Success, result.Message) { Data = result.Data, Validation = result.ValidationErrors }; }
        public static ValidatedIntention From<DType, FType>(ValidatedResult<DType, FType> result) { return new ValidatedIntention(result.Success, result.Message) { Data = result.Data, FailData = result.FailData, Validation = result.ValidationErrors }; }
        public static ValidatedIntention From(Result result) { return new ValidatedIntention(result.Success, result.Message); }
        public static ValidatedIntention From<DType>(Result<DType> result) { return new ValidatedIntention(result.Success, result.Message) { Data = result.Data }; }
        public static ValidatedIntention From<DType, FType>(Result<DType, FType> result) { return new ValidatedIntention(result.Success, result.Message) { Data = result.Data, FailData = result.FailData }; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard.Commons
{
    public static class ResultExtensions
    {
        /// <summary>
        /// Throws failed results as a detailed exception
        /// </summary>
        /// <typeparam name="DT">The type of result data</typeparam>
        /// <typeparam name="FT">The type of fail data</typeparam>
        /// <param name="result">The result to check</param>
        /// <param name="dataTransformer">Optionally transform the result data for inclusion with the message. Appended directly after message; punctuation up to the caller</param>
        /// <returns>The original result</returns>
        public static Result<DT, FT> ThrowOnError<DT, FT>(this Result<DT, FT> result, Func<Result<DT, FT>, string> dataTransformer = null)
        {
            if (!result)
            {
                var xform = dataTransformer != null ? dataTransformer(result) : "";
                throw new Exception(result.Message.IfNullOrBlank("Operation failed") + xform);
            }
            return result;
        }

        /// <summary>
        /// Throws failed results as a detailed exception
        /// </summary>
        /// <typeparam name="T">The type of result data</typeparam>
        /// <param name="result">The result to check</param>
        /// <param name="dataTransformer">Optionally transform the result data for inclusion with the message. Appended directly after message; punctuation up to the caller. Only called if Data is non-null.</param>
        /// <returns>The original result</returns>
        public static Result<T> ThrowOnError<T>(this Result<T> result, Func<Result<T>, string> dataTransformer = null)
        {
            if (!result)
            {
                var xform = dataTransformer != null && result.Data != null ? dataTransformer(result) : "";
                throw new Exception(result.Message.IfNullOrBlank("Operation failed") + xform);
            }
            return result;
        }

        /// <summary>
        /// Throws failed results as an exception
        /// </summary>
        /// <param name="result">The result to check</param>
        /// <returns>The original result</returns>	
        public static Result ThrowOnError(this Result result)
        {
            if (!result)
                throw new Exception(result.Message.IfNullOrBlank("Operation failed"));
            return result;
        }

        /// <summary>
        /// Throws failed results as a detailed exception
        /// </summary>
        /// <typeparam name="DT">The type of result data</typeparam>
        /// <typeparam name="FT">The type of fail data</typeparam>
        /// <param name="result">The result to check</param>
        /// <param name="dataTransformer">Optionally transform the result data for inclusion with the message. Appended directly after message; punctuation up to the caller.</param>
        /// <returns>The original result</returns>
        public static ValidatedResult<DT, FT> ThrowOnError<DT, FT>(this ValidatedResult<DT, FT> result, Func<ValidatedResult<DT, FT>, string> dataTransformer = null)
        {
            if (!result)
            {
                var xform = dataTransformer != null ? dataTransformer(result) : "";
                throw new Exception(result.Message.IfNullOrBlank("Operation failed") + xform);
            }
            return result;
        }

        /// <summary>
        /// Throws failed results as a detailed exception
        /// </summary>
        /// <typeparam name="T">The type of result data</typeparam>
        /// <param name="result">The result to check</param>
        /// <param name="dataTransformer">Optionally transform the result data for inclusion with the message. Appended directly after message; punctuation up to the caller. Only called if Data is non-null.</param>
        /// <returns>The original result</returns>
        public static ValidatedResult<T> ThrowOnError<T>(this ValidatedResult<T> result, Func<ValidatedResult<T>, string> dataTransformer = null)
        {
            if (!result)
            {
                var xform = dataTransformer != null && result.Data != null ? dataTransformer(result) : "";
                throw new Exception(result.Message.IfNullOrBlank("Operation failed") + xform);
            }
            return result;
        }

        /// <summary>
        /// Throws failed results as an exception
        /// </summary>
        /// <param name="result">The result to check</param>
        /// <returns>The original result</returns>	
        public static ValidatedResult ThrowOnError(this ValidatedResult result)
        {
            if (!result)
                throw new Exception(result.Message.IfNullOrBlank("Operation failed"));
            return result;
        }
    }
}

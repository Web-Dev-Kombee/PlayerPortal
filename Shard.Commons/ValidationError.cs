using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard.Commons
{
    public sealed class ValidationError
    {
        public ValidationError(string message)
        {
            Message = message;
            Properties = new string[0];
        }

        public ValidationError(string message, string property)
        {
            Message = message;
            Properties = new[] { property };
        }

        public ValidationError(string message, params string[] properties)
        {
            Message = message;
            Properties = properties;
        }

        public string Message { get; private set; }
        public string[] Properties { get; private set; }

        public static ValidationError Required(string propertyName) => new ValidationError($"{propertyName} is required.", propertyName);
        public static ValidationError Invalid(string propertyName) => new ValidationError($"{propertyName} has an invalid value.", propertyName);
        public static ValidationError Invalid(string propertyName, object badValue = null) => new ValidationError($"'{badValue}' is not a valid value for {propertyName}.", propertyName);
    }
}

using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Base Data Transfer Object model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class BaseModel<T, U> where T : class where U : AbstractValidator<T>
    {
        /// <summary>
        /// Generic validator
        /// </summary>
        protected AbstractValidator<T> _validator;

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseModel()
        {
            _validator = Activator.CreateInstance<U>();
        }

        /// <summary>
        /// Validate the model
        /// </summary>
        /// <returns>True if the model is valid otherwise false.</returns>
        public bool Validate()
        {
            if (_validator == null) return true;

            var result = _validator.Validate<T>(this as T);

            return result.IsValid;
        }

        /// <summary>
        /// Validate the model and throw an exception if it fails.
        /// </summary>
        public void ValidateAndThrow()
        {
            if (_validator != null)
            {
                _validator.ValidateAndThrow<T>(this as T);
            }
        }
    }
}

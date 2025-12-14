using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VRC.SDK3A.Editor")]
[assembly: InternalsVisibleTo("VRC.SDK3.Editor")]
namespace VRC.SDKBase.Editor
{
    /// <summary>
    /// An internal SDK Builder error has occured
    /// </summary>
    public class BuilderException : Exception
    {
        public BuilderException(string message) : base(message)
        {
        }

        public BuilderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Build was blocked by the SDK
    /// </summary>
    public class BuildBlockedException : Exception
    {
        public BuildBlockedException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Content has validation errors
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// List of validation errors
        /// </summary>
        public List<string> Errors = new List<string>();
        
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, List<string> errors) : base(message)
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// Current user does not own the target content
    /// </summary>
    public class OwnershipException : Exception
    {
        public OwnershipException(string message) : base(message)
        {
        }
    }
    
    /// <summary>
    /// An error has occured during the upload process
    /// </summary>
    public class UploadException : Exception
    {
        public UploadException(string message) : base(message)
        {
        }
        
        public UploadException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    /// <summary>
    /// This bundle has already been uploaded
    /// </summary>
    public class BundleExistsException : Exception
    {
        public BundleExistsException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// The user has not confirmed copyright ownership
    /// </summary>
    public class CopyrightOwnershipAgreementException : Exception
    {
        public CopyrightOwnershipAgreementException(string message) : base(message)
        {
        }
    }
}
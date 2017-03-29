﻿using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Abp.BackgroundJobs
{
#if NET46
    [Serializable]
#endif
    public class BackgroundJobException : AbpException
    {
        [CanBeNull]
        public BackgroundJobInfo BackgroundJob { get; set; }

        [CanBeNull]
        public object JobObject { get; set; }

        /// <summary>
        /// Creates a new <see cref="BackgroundJobException"/> object.
        /// </summary>
        public BackgroundJobException()
        {

        }

#if NET46
        /// <summary>
        /// Creates a new <see cref="BackgroundJobException"/> object.
        /// </summary>
        public BackgroundJobException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }
#endif

        /// <summary>
        /// Creates a new <see cref="BackgroundJobException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public BackgroundJobException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}

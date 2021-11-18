// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Filter categories (used for grouping/ordering filters).
    /// </summary>
    internal enum AutofacFilterCategory
    {
        /// <summary>
        /// Authorization Filters
        /// </summary>
        AuthorizationFilter,

        /// <summary>
        /// Authorization Override Filters
        /// </summary>
        AuthorizationFilterOverride,

        /// <summary>
        /// Authentication filters
        /// </summary>
        AuthenticationFilter,

        /// <summary>
        /// Authentication Override Filters
        /// </summary>
        AuthenticationFilterOverride,

        /// <summary>
        /// Action filters
        /// </summary>
        ActionFilter,

        /// <summary>
        /// Action Override filters
        /// </summary>
        ActionFilterOverride,

        /// <summary>
        /// Exception filters
        /// </summary>
        ExceptionFilter,

        /// <summary>
        /// Exception override filters
        /// </summary>
        ExceptionFilterOverride,
    }
}

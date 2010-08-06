﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RT.Util
{
    static class AttributeTargetSets
    {
        public const AttributeTargets TypeDefinitions = AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate | AttributeTargets.Interface;
        public const AttributeTargets DefinitionsWithNames = AttributeTargetSets.TypeDefinitions | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.GenericParameter;
        public const AttributeTargets DefinitionsWithAccessModifiers = AttributeTargetSets.TypeDefinitions | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event;
    }

    /// <summary>Instructs Rummage to keep a specific type, method, constructor or field.</summary>
    [AttributeUsage(AttributeTargetSets.TypeDefinitions | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class RummageNoRemoveAttribute : Attribute
    {
    }

    /// <summary>Instructs Rummage to keep the original name of a specific element. </summary>
    [AttributeUsage(AttributeTargetSets.DefinitionsWithNames, Inherited = false, AllowMultiple = false)]
    public sealed class RummageNoRenameAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargetSets.TypeDefinitions, Inherited = false, AllowMultiple = false)]
    public sealed class RummageNoUnnestAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargetSets.DefinitionsWithAccessModifiers, Inherited = false, AllowMultiple = false)]
    public sealed class RummageNoMarkPublicAttribute : Attribute
    {
    }

    /// <summary>Instructs Rummage not to inline a specific method that would otherwise be automatically inlined. This attribute takes precedence over <see cref="RummageInlineAttribute"/> if both are specified on the same method.</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class RummageNoInlineAttribute : Attribute
    {
    }

    /// <summary>Instructs Rummage to inline a specific method that would otherwise not be automatically inlined.</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class RummageInlineAttribute : Attribute
    {
    }



    /// <summary>Instructs Rummage to refrain from making any changes to a specific type.</summary>
    [AttributeUsage(AttributeTargetSets.DefinitionsWithAccessModifiers | AttributeTargetSets.DefinitionsWithNames | AttributeTargetSets.TypeDefinitions, Inherited = false, AllowMultiple = false), RummageKeepUsersReflectionSafe]
    public sealed class RummageKeepReflectionSafeAttribute : Attribute
    {
    }

    /// <summary>Instructs Rummage to keep all the types reflection-safe which are passed in for the given generic parameter.</summary>
    [AttributeUsage(AttributeTargets.GenericParameter, Inherited = false, AllowMultiple = false)]
    public sealed class RummageKeepArgumentsReflectionSafeAttribute : Attribute
    {
    }

    /// <summary>Use only on custom-attribute class declarations. Instructs Rummage to keep everything reflection-safe that uses the given custom attribute.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class RummageKeepUsersReflectionSafeAttribute : Attribute
    {
    }
}

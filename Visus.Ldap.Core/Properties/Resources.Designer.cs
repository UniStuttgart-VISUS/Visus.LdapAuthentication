﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Visus.Ldap.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Visus.Ldap.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property storing the account name was already configured..
        /// </summary>
        internal static string ErrorAccountNamePropertyAlreadySet {
            get {
                return ResourceManager.GetString("ErrorAccountNamePropertyAlreadySet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given object does not have the requested attribute mapped to a property..
        /// </summary>
        internal static string ErrorAttributeNotMapped {
            get {
                return ResourceManager.GetString("ErrorAttributeNotMapped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property storing the distinguished name was already configured..
        /// </summary>
        internal static string ErrorDistinguishedNamePropertyAlreadySet {
            get {
                return ResourceManager.GetString("ErrorDistinguishedNamePropertyAlreadySet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The password for the LDAP service account must be specified unless no service user is specified..
        /// </summary>
        internal static string ErrorEmptyPassword {
            get {
                return ResourceManager.GetString("ErrorEmptyPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A search base must be a non-empty string..
        /// </summary>
        internal static string ErrorEmptySearchBase {
            get {
                return ResourceManager.GetString("ErrorEmptySearchBase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property storing the group memberships was already configured..
        /// </summary>
        internal static string ErrorGroupMembershipsPropertyAlreadySet {
            get {
                return ResourceManager.GetString("ErrorGroupMembershipsPropertyAlreadySet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property storing the identity was already configured..
        /// </summary>
        internal static string ErrorIdentityPropertyAlreadySet {
            get {
                return ResourceManager.GetString("ErrorIdentityPropertyAlreadySet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified value cannot be converted into a number..
        /// </summary>
        internal static string ErrorInconvertibleNumber {
            get {
                return ResourceManager.GetString("ErrorInconvertibleNumber", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to FILETIME values can only be converted to DateTime or DateTimeOffset..
        /// </summary>
        internal static string ErrorInvalidFileTimeTarget {
            get {
                return ResourceManager.GetString("ErrorInvalidFileTimeTarget", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified target type cannot be created from bitwise conversion of a number..
        /// </summary>
        internal static string ErrorInvalidNumberTarget {
            get {
                return ResourceManager.GetString("ErrorInvalidNumberTarget", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given byte sequence &quot;{0}&quot; does not represent a valid security identifier..
        /// </summary>
        internal static string ErrorInvalidSid {
            get {
                return ResourceManager.GetString("ErrorInvalidSid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SIDs can only be converted into strings..
        /// </summary>
        internal static string ErrorInvalidSidTarget {
            get {
                return ResourceManager.GetString("ErrorInvalidSidTarget", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The proeprty storing whether a group is the primary group was already configured..
        /// </summary>
        internal static string ErrorIsPrimaryGroupPropertyAlreadySet {
            get {
                return ResourceManager.GetString("ErrorIsPrimaryGroupPropertyAlreadySet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property {0} of {1} lacks the required [LdapAttribute] annotation. This indicates an internal error in the library. Please report this bug..
        /// </summary>
        internal static string ErrorMissingLdapAnnotation {
            get {
                return ResourceManager.GetString("ErrorMissingLdapAnnotation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified target type does not have a Parse method to convert from strings..
        /// </summary>
        internal static string ErrorMissingNumberParse {
            get {
                return ResourceManager.GetString("ErrorMissingNumberParse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The LDAP entry {0} is lacking the required attribute {1}..
        /// </summary>
        internal static string ErrorMissingRequiredAttribute {
            get {
                return ResourceManager.GetString("ErrorMissingRequiredAttribute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The property was already mapped to an attribute..
        /// </summary>
        internal static string ErrorPropertyAlreadyMapped {
            get {
                return ResourceManager.GetString("ErrorPropertyAlreadyMapped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given property {0} does not exist in type {1} or is not a public instance property..
        /// </summary>
        internal static string ErrorPropertyMissing {
            get {
                return ResourceManager.GetString("ErrorPropertyMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An attribute map can only be built for a single schema..
        /// </summary>
        internal static string ErrorSchemaChange {
            get {
                return ResourceManager.GetString("ErrorSchemaChange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The schema used by the attribute must match the schema used by the mapper..
        /// </summary>
        internal static string ErrorSchemaMismatch {
            get {
                return ResourceManager.GetString("ErrorSchemaMismatch", resourceCulture);
            }
        }
    }
}

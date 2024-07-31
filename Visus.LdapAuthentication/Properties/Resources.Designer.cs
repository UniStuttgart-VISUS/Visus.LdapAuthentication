﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Visus.LdapAuthentication.Properties {
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
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Visus.LdapAuthentication.Properties.Resources", typeof(Resources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot set &quot;SearchBases&quot; and the legacy option &quot;SearchBase&quot; at the same time. Move all LDAP search bases to the new &quot;SearchBases&quot; property..
        /// </summary>
        public static string ErrorAmbiguousSearchScope {
            get {
                return ResourceManager.GetString("ErrorAmbiguousSearchScope", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The search base must be a non-empty string..
        /// </summary>
        public static string ErrorEmptySearchBase {
            get {
                return ResourceManager.GetString("ErrorEmptySearchBase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An entry with a value of &quot;{1}&quot; for &quot;{0}&quot; does not exist in the directory..
        /// </summary>
        public static string ErrorEntryNotFound {
            get {
                return ResourceManager.GetString("ErrorEntryNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not set group claims for &quot;{0}&quot;..
        /// </summary>
        public static string ErrorGroupClaim {
            get {
                return ResourceManager.GetString("ErrorGroupClaim", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The group with identity &quot;{group}&quot; was not found in the directory..
        /// </summary>
        public static string ErrorGroupNotFound {
            get {
                return ResourceManager.GetString("ErrorGroupNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given byte sequence &quot;{0}&quot; does not represent a valid security identifier..
        /// </summary>
        public static string ErrorInvalidSid {
            get {
                return ResourceManager.GetString("ErrorInvalidSid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The default LdapOptions cannot be registered as interface, because this would result in an ambiguous constructor call. Use the non-generic variant of AddLdapOptions to register the default options type..
        /// </summary>
        public static string ErrorLdapOptionsRegistration {
            get {
                return ResourceManager.GetString("ErrorLdapOptionsRegistration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Paging of LDAP search results must start at 0..
        /// </summary>
        public static string ErrorLdapPage {
            get {
                return ResourceManager.GetString("ErrorLdapPage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The size of a page of LDAP search results must be at least 1..
        /// </summary>
        public static string ErrorLdapPageSize {
            get {
                return ResourceManager.GetString("ErrorLdapPageSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Moving to next LDAP entry failed..
        /// </summary>
        public static string ErrorMoveNext {
            get {
                return ResourceManager.GetString("ErrorMoveNext", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not retrieve the primary group claim for &quot;{0}&quot;..
        /// </summary>
        public static string ErrorPrimaryGroupClaim {
            get {
                return ResourceManager.GetString("ErrorPrimaryGroupClaim", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given property &quot;{0}&quot; was not found..
        /// </summary>
        public static string ErrorPropertyNotFound {
            get {
                return ResourceManager.GetString("ErrorPropertyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given property &quot;{0}&quot; has no LDAP attribute mapped for the current schema &quot;{1}&quot;..
        /// </summary>
        public static string ErrorPropertyNotMapped {
            get {
                return ResourceManager.GetString("ErrorPropertyNotMapped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified server selection policy is not implemented..
        /// </summary>
        public static string ErrorUnknownServerSelectionPolicy {
            get {
                return ResourceManager.GetString("ErrorUnknownServerSelectionPolicy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The login succeeded, but your user information could not be retrieved..
        /// </summary>
        public static string ErrorUserNotFound {
            get {
                return ResourceManager.GetString("ErrorUserNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The login of &quot;{0}&quot; succeeded, but the user information could not be retrieved. This usually indicates a misconfiguration of the search bases. Make sure that all user accounts that should be able to login are located in one of the configured search bases..
        /// </summary>
        public static string ErrorUserNotFoundDetailed {
            get {
                return ResourceManager.GetString("ErrorUserNotFoundDetailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Performing anonyomous bind..
        /// </summary>
        public static string InfoBindAnonymous {
            get {
                return ResourceManager.GetString("InfoBindAnonymous", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Performing LDAP bind as user &quot;{0}&quot;..
        /// </summary>
        public static string InfoBindingAsUser {
            get {
                return ResourceManager.GetString("InfoBindingAsUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Anonymous bind succeeded..
        /// </summary>
        public static string InfoBoundAnonymous {
            get {
                return ResourceManager.GetString("InfoBoundAnonymous", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Authenticated successfully as user &quot;{0}&quot;..
        /// </summary>
        public static string InfoBoundAsUser {
            get {
                return ResourceManager.GetString("InfoBoundAsUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connecting to LDAP server {0}:{1}..
        /// </summary>
        public static string InfoConnectingLdap {
            get {
                return ResourceManager.GetString("InfoConnectingLdap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The LDAP server {0} has been selected for a new connection..
        /// </summary>
        public static string InfoServerSelected {
            get {
                return ResourceManager.GetString("InfoServerSelected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LDAP SSL certificate check has been disabled..
        /// </summary>
        public static string WarnCertCheckDisabled {
            get {
                return ResourceManager.GetString("WarnCertCheckDisabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no redundant LDAP server left to fall back to that has not been blacklisted..
        /// </summary>
        public static string WarnNoFallback {
            get {
                return ResourceManager.GetString("WarnNoFallback", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The LDAP server {0} has been blacklisted until {1}..
        /// </summary>
        public static string WarnServerBlacklisted {
            get {
                return ResourceManager.GetString("WarnServerBlacklisted", resourceCulture);
            }
        }
    }
}

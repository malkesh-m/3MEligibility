using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;

namespace MEligibilityPlatform.Application.Services
{
    public class LdapService() : ILdapService
    {
        //private readonly string _ldapServer = configuration["Ldap:Server"]!;
        //private readonly int _ldapPort = int.Parse(configuration["Ldap:Port"]!);
        //private readonly bool _useSsl = bool.Parse(configuration["Ldap:UseSsl"]!);
        //private readonly string _bindDn = configuration["Ldap:BindDn"]!;
        //private readonly string _bindPassword = configuration["Ldap:BindPassword"]!;
        //private readonly string _searchBase = configuration["Ldap:SearchBase"]!;
        //private readonly string _userSearchFilter = configuration["Ldap:UserSearchFilter"]!;

        //public async Task<(bool IsAuthenticated, Dictionary<string, string> UserAttributes)> Authenticate(string username, string password)
        //{
        //    var attributes = new Dictionary<string, string>();

        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        //        return (false, attributes);

        //    try
        //    {
        //        using var connection = new LdapConnection { SecureSocketLayer = _useSsl };

        //        await connection.ConnectAsync(_ldapServer, _ldapPort);

        //        // Step 1: Bind with service account
        //        await connection.BindAsync(_bindDn, _bindPassword);

        //        // Find user DN
        //        var searchFilter = string.Format(_userSearchFilter, username);

        //        var results = await connection.SearchAsync(
        //            _searchBase,
        //            LdapConnection.ScopeSub,
        //            searchFilter,
        //            null,
        //            false
        //        );

        //        if (!await results.HasMoreAsync())
        //            return (false, attributes);

        //        var userEntry = await results.NextAsync();
        //        var userDn = userEntry.Dn;

        //        // Step 2: Bind as user — validates password
        //        await connection.BindAsync(userDn, password);

        //        if (!connection.Bound)
        //            return (false, attributes);

        //        // Extract user attributes
        //        var attrSet = userEntry.GetAttributeSet();
        //        foreach (LdapAttribute attr in attrSet)
        //            attributes[attr.Name] = attr.StringValue;

        //        return (true, attributes);
        //    }
        //    catch (LdapException ex)
        //    {
        //        // error 49 → Invalid credentials
        //        System.Diagnostics.Debug.WriteLine("LDAP Error: " + ex.Message);
        //        return (false, attributes);
        //    }
        //}
    }

}

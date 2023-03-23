// <copyright file="User.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Security.Principal;

namespace OnlineSales.Plugin.AzureAD.Entities
{
    public class User
    {
        public User()
        {
        }

        public User(IIdentity identity)
        {
            if (string.IsNullOrEmpty(identity.Name))
            {
                throw new ArgumentNullException(identity.Name);
            }

            this.Name = identity.Name;
        }

        public string Name { get; set; } = string.Empty;
    }
}

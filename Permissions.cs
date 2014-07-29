using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace EXPEDIT.Share {
    public class Permissions : IPermissionProvider {
        public static readonly Permission FormBuilder = new Permission { Description = "Form Builder", Name = "FormBuilder" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                FormBuilder
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {FormBuilder}
                }
            };
        }

    }
}
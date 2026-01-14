using System;
using System.ComponentModel.DataAnnotations;

namespace dawn.ftp.ui.Models {
    internal sealed class HostName : ValidationAttribute {
        public override bool IsValid(object value) {


            if (value is not string hostName) { 
                return false; 
            }

            //Empty strings should not generate an issue normally
            if (string.IsNullOrEmpty(hostName)) {
                return true;
            }

            var type = Uri.CheckHostName(hostName);
            
            if (type == UriHostNameType.Unknown) {
                return false;
            }

            return true;
        }

        public override string FormatErrorMessage(string name) {
            return base.FormatErrorMessage(name);
        }
    }
}

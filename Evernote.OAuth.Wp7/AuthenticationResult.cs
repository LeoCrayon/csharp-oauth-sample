using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Evernote.OAuth {
    public class AuthenticationResult {
        public string AuthenticationToken { get; set; }
        public long CurrentTime { get; set; }
        public long Expiration { get; set; }
        public string NoteStoreUrl { get; set; }
        public User User { get; set; }
        public string WebApiUrlPrefix { get; set; }

    }
}

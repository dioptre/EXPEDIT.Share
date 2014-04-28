using Orchard.UI.Resources;

namespace EXPEDIT.Share {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("Picker").SetUrl("picker.css");
            builder.Add().DefineStyle("Share").SetUrl("expedit-share.css");
        }
    }
}

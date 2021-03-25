using System.IO;
using System.IO.Compression;
using System.Text;

namespace shared.serialization {
    public static class Compress {
        public static byte[] Zip(string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream()) {
                using (var gs = new GZipStream(mso, CompressionLevel.Optimal)) {
                    msi.CopyTo(gs);
                }
                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes) {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream()) {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}
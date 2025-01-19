// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("39PnMo4svQsW1YgLU+v+DDv9ZXmqKScoGKopIiqqKSkonr1bTILPaZ8QeYfVH2sowhGsrT9BKOuoJRSBGKopChglLiECrmCu3yUpKSktKCsjCCX6s3lx30bKE8eWL8XdXbaZh6dFSPUiErDZnU4EvTsdR2zmf0IgtlKkm7cbasyMX6P0eYtixJmG3kRM2GHTTcVt2TL5P30ZsP3oErC8iE5i87VKzc444stSU8chbw8ot/DMclGyhqsqg20Lhlo1wHyJJ0niE7AZfN50JGVyMQY49ftoJFJuSowFcX5RdyjTFhL3JG+/hb5Onw8M/TCfQG/FmHeqp4R2FBDnzvAOEt0TAu02FA/y3OftR7EgWGvRGi2yuUJZNyBysyMbVGJegyorKSgp");
        private static int[] order = new int[] { 4,1,3,4,4,11,7,13,11,10,12,13,13,13,14 };
        private static int key = 40;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}

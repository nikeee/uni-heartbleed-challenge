using System;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Challenge
{
    class Program
    {
        const string ramBufferStr = "26fb96dabd51bdf235726be22030c29ccea3c5b01794a13783125466e15b33dfad6df808f072e50389dd33cdef192cc7aba267ab6afcea61748716ad5b4d2e140d13682c7818712056408a659425063c16e00cd75ac37743e0bb8da38b96f5a96c214dd5be483008740b3f43fad237aa95d806cf283d8cae5c2c8dc684529b4371d09960e0b7dd49314a36894fcdff251dd9d9408dd0cfaa2dd14a62af77b70e7578a1f27d21e34e084273b6d6b95300fe2dd3cd6edf2d127a5c8d7c07795eb930f4ddc299fda3f7a884a66d357e663b6bbb3dbb0757eb2b70010df699796efd1af20cd0165771d4cb041c28a583baa79165b82e8a9b308ebf4540cf76d6a7e7475c51b5edf46a746ad990eeaaef33f98c56f10ec9ea5f11b1300232fed305fe648406aefe4a2791c41f45677418d1dccb1d6faa664694bf06df4163a692fb8bc08fa1154cf0710893b643ae8a766a33f5147cc2b1e623db4791f82ce9f4092099ca9e0c1bbbb6c77ef800a725894f644ec80b3a68cc31d8360984883019b66eb65d44b722ec02a155c7cf90843593e33612a0b157c6f5ce635b7bacec57bd07b454520401fa1a02692cfb744f31d0860d7e2c7164c5d1e9d8fe38b5e2590f05703228d55cb3468ced34bf4e848749657c61dd84c4b8aac51b64a10e39d7c3ab5b267c7f0efd21455d245f22e5fa5ea39a91886d33622c4f5919f525e5a67fee5b663c0f903330cb52e5f44540c291adf8f28c910a150a7acc48748b4552092fa2bbdbf9366c0b710ac0f9af2f923f3150914bc841923816ea6048f2c9404eaa358facf93e81ce3f55b512aadc018b90dfc80228ba3ad17e93f7d1762845d750b44eec485e139830ba9be9aa4ceab43895f13548215174bc60f6b3bfc68ac471cbff964aaa05c2c06be5f212e8ca458f1f8d4b4b43f83ddea240130357c57231141e51dc230a5c08410c50781cc7b07936033df05f0037b79c5b26d00b90f0063c38a33f3ab722ec56026e29e6dd93b3125c713738be529656f8be25e12b97bffd9c4e8ff0076196d4760564f845d68e3455ec97c666f73a2e6b69e1a9a6b8f564941054c221b24af72cf7b9a7bfdd467cd809180e614d4c5e8e72c52be904c55e9449e6ca3a809d7bd1bd3144db4b7ea85f05230bd91436aaa0f84ada07daeb12cda898017d865c74a4d9e766613c76be7d29f4968081390aaeff0a25a47164118626f6c2951d88226e746683bb69e8ee191d9e69f88e4d31baa3f3b6af8c16353edfade38c78bae9ffa1311e6f82aab4fcf9746ab3eb4d8da169deea2db915805d0c96ab0d8074d37e9d006428cbe396a6b9a377d700f4c72288a4d043ac7c195c849b12f50f1e31d4461b2de66a0d2329c88f4e08639e873eaa84c72d3f8521dd6b17095b8969a6bc2c0faeae0894b763cfc5ed23f927b0d8a08dd7a33897a78e7954a396e3bfd4f1de8e3619807c3457a8b7c88c9ac8cd015baaafd872e2b5c4fb5d70622be7e0d2ec7765c09640706a9f0683a525b04a9bad7b1a1607386a749ab07f88c1af9fd889adc08dda1934bd7ebc7a1eb6aafd57e6182925535ca9ceda33108c2e3433ae910f16b9f26005fa0831afd49024c0d761304d3a66c3d0133c08c6afc5af588556854b6c42c4a0ad72c63b02fb7b7421dba904c75d7cb7f0d3d5f2beac00fa1e3cc1aaf7fb313b4702ccb3384ff2581ee835a0f356847c3299f3396055e2d91969b8bb93565783e0cfbca9daa12d9c79e7b265c3ce2fc5a52f87f9878575d085eaf22ac750c2b127121c586d9e73da8fcaa905b9a60448e87c831b6f112ec90fc00740f6f6533dba15386d6f6459f72f8bd9ac96b8dcda8bdf5e56a03fb7cc9219d7761c4e5156020a9f1c5b63c557433c41d30a53ee1b6a7a5a4d13adfeb78b8a9fbc7e";
        static readonly byte[] ramBuffer = ConvertHexStringToByteArray(ramBufferStr);

        static readonly BigInteger N = BigInteger.Parse("108795305821643854164532385081809608579909162489086869236550662890297424128637587583234907931888914514007048578832472961398814846760059656592124642435578018387739668196542410392948314630049006679345168493240223036951280197192761153682004949945739599410212714303869435263419367949353389402256552228090901241019");
        const int e = 65539;

        static readonly BigInteger message = new BigInteger(1337);
        static readonly BigInteger messageEncrypted = BigInteger.ModPow(message, e, N);

        static readonly int maximumDLength = N.GetByteCount() - 1;

        static void Main(string[] args)
        {
            var ram = ramBuffer.AsMemory();
            if (ram.Length < maximumDLength)
            {
                Console.WriteLine("The provided buffer is too short.");
                return;
            }

            for (int candidateLength = maximumDLength; candidateLength > 0; --candidateLength)
            {
                Parallel.For(0, ram.Length - maximumDLength, candidatePosition =>
                {
                    var dCandidateBuffer = ram.Slice(candidatePosition, candidateLength);

                    var dCandidate = new BigInteger(dCandidateBuffer.Span, true /* bigEndian */, true /* unsigned */);

                    if (dCandidate >= N)
                        return;

                    var decryptedSample = BigInteger.ModPow(messageEncrypted, dCandidate, N);

                    if (decryptedSample == message)
                    {
                        Console.WriteLine($"Found d at {candidatePosition}:{candidateLength}:");
                        Console.WriteLine(dCandidate.ToString());
                        Environment.Exit(0);
                        return;
                    }
                });
            }
            Console.WriteLine("Search exhausted.");
        }

        /// <summary> Taken from: https://stackoverflow.com/a/8235530 </summary>
        public static byte[] ConvertHexStringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace Common.SecurityCrypto.SMCrypto
{
    public class SM2AlgorithmIdentifier : AlgorithmIdentifier
    {
        private readonly bool parametersDefined;

        public SM2AlgorithmIdentifier(DerObjectIdentifier objectID)
          : base(objectID)
        {
        }

        public SM2AlgorithmIdentifier(DerObjectIdentifier objectID, Asn1Encodable parameters)
          : base(objectID, parameters)
        {
            parametersDefined = true;
        }

        public virtual Asn1Object ToAsn1Object() => new DerSequence(new Asn1EncodableVector(new Asn1Encodable[2]
        {
       ObjectID,
       new DerObjectIdentifier("1.2.156.10197.1.301")
        }));
    }
}
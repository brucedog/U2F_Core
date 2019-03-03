using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using U2F.Core.Exceptions;
using U2F.Core.Utils;

namespace U2F.Core.Models
{
    public class ClientData
    {
        private const string TypeParam = "typ";
        private const string ChallengeParam = "challenge";
        private const string OriginParam = "origin";

        public string Type { get; private set; }
        public string Challenge { get; private set; }
        public string Origin { get; private set; }
        public string RawClientData { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientData"/> class.
        /// </summary>
        /// <param name="clientData">The client data.</param>
        /// <exception cref="U2fException">ClientData has wrong format</exception>
        public ClientData(string clientData)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(clientData))
                    throw new U2fException(U2fException.InvalidArguments);

                RawClientData = clientData.Base64StringToByteArray().GetString();

                JObject element = JObject.Parse(RawClientData);
                if (element == null)
                    throw new U2fException("ClientData has wrong format");

                JToken theType, theChallenge, theOrgin;
                if (!element.TryGetValue(TypeParam, out theType))
                    throw new U2fException("Bad clientData: missing 'typ' param");
                if (!element.TryGetValue(ChallengeParam, out theChallenge))
                    throw new U2fException("Bad clientData: missing 'challenge' param");

                Type = theType.ToString();
                Challenge = theChallenge.ToString();
                if (element.TryGetValue(OriginParam, out theOrgin))
                    Origin = theOrgin.ToString();
            }
            catch (Exception exception)
            {
                throw new U2fException("Invalid clientData format.: " + exception.Message);
            }
        }

        public void CheckContent(string type, string challenge, HashSet<string> facets)
        {
            if (!type.Equals(Type) || string.IsNullOrWhiteSpace(type))
            {
                throw new U2fException("Bad clientData: bad type " + type);
            }
            if (!challenge.Equals(Challenge) || string.IsNullOrWhiteSpace(challenge))
            {
                throw new U2fException("Wrong challenge signed in clientData");
            }
            if (facets != null)
            {
                VerifyOrigin(Origin, CanonicalizeOrigins(facets));
            }
        }

        public string AsJson()
        {
            return RawClientData;
        }

        private void VerifyOrigin(string origin, HashSet<string> allowedOrigins)
        {
            if (!allowedOrigins.Contains(CanonicalizeOrigin(origin)))
            {
                throw new UriFormatException(origin + " is not a recognized home origin for this backend");
            }
        }

        private HashSet<string> CanonicalizeOrigins(HashSet<string> origins)
        {
            HashSet<string> result = new HashSet<string>();
            foreach (string orgin in origins)
            {
                result.Add(CanonicalizeOrigin(orgin));
            }
            return result;
        }

        private string CanonicalizeOrigin(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new U2fException(U2fException.InvalidArguments);
            try
            {
                Uri uri = new Uri(url);
                if (string.IsNullOrWhiteSpace(uri.Authority))
                    return url;

                return uri.Scheme + "://" + uri.Authority;
            }
            catch (UriFormatException e)
            {
                throw new UriFormatException("specified bad origin", e);
            }
        }
    }
}
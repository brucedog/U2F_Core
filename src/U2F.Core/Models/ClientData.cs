using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using U2F.Core.Exceptions;
using U2F.Core.Utils;

namespace U2F.Core.Models
{
    public class ClientData
    {
        private const String TypeParam = "typ";
        private const String ChallengeParam = "challenge";
        private const String OriginParam = "origin";

        public String Type { get; private set; }
        public String Challenge { get; private set; }
        public String Origin { get; private set; }
        public String RawClientData { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientData"/> class.
        /// </summary>
        /// <param name="clientData">The client data.</param>
        /// <exception cref="U2fException">ClientData has wrong format</exception>
        public ClientData(String clientData)
        {
            try
            {
                if(String.IsNullOrWhiteSpace(clientData))
                    throw  new ArgumentException("Invalid argument passed.");

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

        public void CheckContent(String type, String challenge, HashSet<String> facets)
        {
            if (!type.Equals(Type) || String.IsNullOrWhiteSpace(type))
            {
                throw new U2fException("Bad clientData: bad type " + type);
            }
            if (!challenge.Equals(Challenge) || String.IsNullOrWhiteSpace(challenge))
            {
                throw new U2fException("Wrong challenge signed in clientData");
            }
            if (facets != null)
            {
                VerifyOrigin(Origin, CanonicalizeOrigins(facets));
            }
        }

        public String AsJson()
        {
            return RawClientData;
        }

        private void VerifyOrigin(String origin, HashSet<String> allowedOrigins)
        {
            if (!allowedOrigins.Contains(CanonicalizeOrigin(origin)))
            {
                throw new UriFormatException(origin + " is not a recognized home origin for this backend");
            }
        }

        private HashSet<String> CanonicalizeOrigins(HashSet<String> origins)
        {
            HashSet<String> result = new HashSet<string>();
            foreach (string orgin in origins)
            {
                result.Add(CanonicalizeOrigin(orgin));
            }
            return result;
        }

        private String CanonicalizeOrigin(String url)
        {
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Invalid argument(s) were being passed.");
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